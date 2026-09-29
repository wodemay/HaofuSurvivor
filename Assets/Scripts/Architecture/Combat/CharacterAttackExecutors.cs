using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public abstract class CharacterSequenceExecutor : IAttackExecutor, IPreparedAttackExecutor, ICharacterSequenceExecutor
	{
		public abstract string Id { get; }
		public virtual bool RequiresTarget => false;
		public virtual void ConfigureOwner(GameObject owner, AttackConfig config, CombatFaction faction, int runtimeId = 0)
			=> GameArchitecture.Interface.GetSystem<AttackSystem>().RegisterManual(owner, config, faction, runtimeId);
		public virtual bool CanExecute(AttackExecutionContext context) => context.Owner != null && context.Config.ExecutorParameterConfig is CharacterAttackParameterConfig;
		public float Cooldown(AttackExecutionContext context)
			=> (context.Config.ExecutorParameterConfig as CharacterAttackParameterConfig)?.AtLevel(context.SkillRuntime?.Level ?? 1)?.Cooldown ?? context.Config.Cooldown;
		public virtual void Execute(AttackExecutionContext context)
		{
			System.Schedule(context);
			System.Sound(((CharacterAttackParameterConfig)context.Config.ExecutorParameterConfig).StartSound);
		}
		public abstract bool Advance(CharacterActionState state, CharacterAttackParameterConfig config, GameObject owner);
		protected static CharacterCombatSystem System => GameArchitecture.Interface.GetSystem<CharacterCombatSystem>();
		protected static float DirectionAngle(CharacterActionState state) => Mathf.Atan2(state.DirectionY, state.DirectionX) * Mathf.Rad2Deg;
		protected static void SwordVisual(CharacterActionState state, CharacterAttackParameterConfig config, GameObject owner, float angle)
		{
			if (state.Visuals.Count == 0) state.Visuals.Add(System.Visual(config.WeaponVisual, owner.transform.position, angle, 1.6f));
			var sword = state.Visuals[0];
			if (sword == null) return;
			var direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
			sword.transform.position = owner.transform.position + (Vector3)(direction * 0.2f) + Vector3.down * 0.15f;
			sword.transform.rotation = Quaternion.Euler(0, 0, angle);
		}
	}

	public class SwordAttackExecutor : CharacterSequenceExecutor, IAutomaticAttackExecutor, ITransientAttackSequence
	{
		public override string Id => "sword";
		public override bool RequiresTarget => true;
		public override void ConfigureOwner(GameObject owner, AttackConfig config, CombatFaction faction, int runtimeId = 0)
			=> GameArchitecture.Interface.GetSystem<AttackSystem>().RegisterAutomatic(owner, config, faction, runtimeId);
		public CombatEntity FindTarget(AttackExecutionContext context)
		{
			var config = context.Config.ExecutorParameterConfig as CharacterAttackParameterConfig;
			if (config == null || context.Owner == null) return null;
			var origin = (Vector2)context.Owner.transform.position;
			CombatEntity best = null; var distance = float.MaxValue;
			foreach (var target in GameArchitecture.Interface.GetSystem<CombatTargetSystem>().FindOpponentsInRange(origin, context.OwnerFaction, config.Radius + context.GetModifierValue("Melee.RadiusAdd", 0)))
			{
				var d = ((Vector2)target.transform.position - origin).sqrMagnitude;
				if (d >= distance || !CharacterCombatSystem.Visible(origin, target.transform.position)) continue;
				best = target; distance = d;
			}
			return best;
		}
		public override bool Advance(CharacterActionState state, CharacterAttackParameterConfig config, GameObject owner)
		{
			if (state.Step == 0 && state.Target != null && state.Target.isActiveAndEnabled && state.Target.SpawnId == state.TargetSpawnId)
			{
				var direction = ((Vector2)(state.Target.transform.position - owner.transform.position)).normalized;
				if (direction.sqrMagnitude > 0.001f) { state.DirectionX = direction.x; state.DirectionY = direction.y; }
			}
			var progress = Mathf.Clamp01((state.Age - config.Windup) / 0.12f);
			var angle = DirectionAngle(state) + Mathf.Lerp(-state.Angle * 0.5f, state.Angle * 0.5f, progress) * state.Swing;
			SwordVisual(state, config, owner, angle);
			if (state.Age >= config.Windup && (state.Step == 0 || state.Age < config.Windup + 0.12f))
			{
				if (state.Step == 0) System.Flash(config.EffectVisual, owner.transform.position, DirectionAngle(state), state.Radius * 2, 0.15f, Color.white);
				state.Step = 1;
				System.HitArea(owner.transform.position, new Vector2(state.DirectionX, state.DirectionY), state.Radius, state.Angle, state.Damage, config.Knockback, state.HitTargets);
			}
			return state.Age < config.Duration;
		}
	}

	public class SpinSwordAttackExecutor : CharacterSequenceExecutor
	{
		public override string Id => "spin-sword";
		public override void Execute(AttackExecutionContext context)
		{
			var config = (CharacterAttackParameterConfig)context.Config.ExecutorParameterConfig;
			System.State.ReductionRemaining = config.AtLevel(context.SkillRuntime?.Level ?? 1)?.ReductionDuration ?? config.ReductionDuration;
			System.State.Reduction = config.Reduction;
			base.Execute(context);
		}
		public override bool Advance(CharacterActionState state, CharacterAttackParameterConfig config, GameObject owner)
		{
			SwordVisual(state, config, owner, DirectionAngle(state) + Mathf.Clamp01((state.Age - config.Windup) / 0.25f) * 360);
			if (state.Step == 0 && state.Age >= config.Windup)
			{
				state.Step = 1;
				System.HitArea(owner.transform.position, Vector2.right, state.Radius, 360, state.Damage, config.Knockback);
				System.Flash(config.EffectVisual, owner.transform.position, 0, state.Radius * 2, 0.3f, new Color(1, 0.8f, 0.35f));
			}
			return state.Age < config.Duration;
		}
	}

	public class PiercingBoltAttackExecutor : ProjectileAttackExecutor
	{
		public override string Id => "piercing-bolt";
		public override void Execute(AttackExecutionContext context)
		{
			if (context.Owner == null || context.Target == null || context.Config.ExecutorParameterConfig is not CharacterAttackParameterConfig config) return;
			var system = GameArchitecture.Interface.GetSystem<CharacterCombatSystem>();
			var empowered = system.State.EmpowerRemaining > 0 && context.WeaponRuntime != null;
			var damage = (context.Config.Damage + context.GetModifierValue(WeaponUpgradeModifierKeys.ProjectileDamageAdd, 0)) * context.GetDamageMultiplier();
			var pierce = config.Pierce + Mathf.RoundToInt(context.GetModifierValue(WeaponUpgradeModifierKeys.ProjectilePierceAdd, 0));
			if (empowered) { damage *= system.State.EmpowerMultiplier; pierce += 2; }
			var direction = (Vector2)(context.Target.transform.position - context.Owner.transform.position);
			if (direction.sqrMagnitude < 0.001f) direction = Vector2.right;
			var count = ProjectileAttackUtility.GetWeaponProjectileCount(context);
			var speed = config.MoveSpeed * Mathf.Max(0, context.GetModifierValue(WeaponUpgradeModifierKeys.ProjectileSpeedMultiplier, 1));
			var spawned = false;
			for (var i = 0; i < count; i++)
			{
				var spread = Quaternion.Euler(0, 0, (i - (count - 1) * 0.5f) * 10) * direction.normalized;
				spawned |= ProjectileFactory.Instance.Spawn(config, context.Owner.transform.position, spread, context.OwnerFaction, damage, speed, pierce) != null;
			}
			if (empowered && spawned) system.State.EmpowerRemaining = 0;
		}
	}

	public class HunterVolleyAttackExecutor : CharacterSequenceExecutor
	{
		public override string Id => "hunter-volley";
		public override bool CanExecute(AttackExecutionContext context)
		{
			return base.CanExecute(context) && GameArchitecture.Interface.GetSystem<CombatTargetSystem>().FindClosestOpponent(context.Owner.transform.position,
				context.OwnerFaction, ((CharacterAttackParameterConfig)context.Config.ExecutorParameterConfig).AttackRange) != null;
		}
		public override bool Advance(CharacterActionState state, CharacterAttackParameterConfig config, GameObject owner)
		{
			var values = config.AtLevel(state.Level);
			while (state.Step < (values?.Shots ?? config.Shots) && state.Age >= state.Step * config.Interval)
			{
				state.Step++;
				if (state.Target == null || !state.Target.isActiveAndEnabled || state.Target.SpawnId != state.TargetSpawnId || Vector2.Distance(owner.transform.position, state.Target.transform.position) > config.AttackRange)
					state.Target = GameArchitecture.Interface.GetSystem<CombatTargetSystem>().FindClosestOpponent(owner.transform.position, CombatFaction.Player, config.AttackRange);
				if (state.Target == null) continue;
				state.TargetSpawnId = state.Target.SpawnId;
				System.Sound(config.ShotSound);
				var direction = ((Vector2)(state.Target.transform.position - owner.transform.position)).normalized;
				ProjectileFactory.Instance.Spawn(config, owner.transform.position, direction, CombatFaction.Player, state.Damage, config.MoveSpeed, config.Pierce);
				System.Flash(config.EffectVisual, owner.transform.position, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, 0.4f, 0.12f, Color.cyan);
			}
			return state.Age < (values?.Duration ?? config.Duration);
		}
	}

	public class StarOverloadAttackExecutor : CharacterSequenceExecutor
	{
		public override string Id => "star-overload";
		public override bool Advance(CharacterActionState state, CharacterAttackParameterConfig config, GameObject owner)
		{
			var values = config.AtLevel(state.Level);
			if (state.Visuals.Count == 0 && state.Age < config.Windup)
				System.Flash(config.EffectVisual, owner.transform.position, 0, 1.4f, config.Windup, new Color(0.3f, 0.85f, 1, 0.65f));
			while (state.Visuals.Count < 4) state.Visuals.Add(System.Visual(config.WeaponVisual, owner.transform.position, 0, 0.6f));
			for (var i = 0; i < 4; i++)
			{
				var angle = i * 90 + state.Age * 90;
				var radius = config.OrbitRadius * Mathf.Clamp01(state.Age / config.Windup) * Mathf.Clamp01((config.Windup + config.Duration - state.Age) / 0.2f);
				var visual = state.Visuals[i];
				if (visual == null) continue;
				visual.transform.position = owner.transform.position + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
				visual.transform.rotation = Quaternion.Euler(0, 0, angle);
			}
			while (state.Step < config.Shots && state.Age >= config.Windup + state.Step * config.Interval)
			{
				state.Step++;
				var targets = new System.Collections.Generic.List<CombatEntity>(GameArchitecture.Interface.GetSystem<CombatTargetSystem>().FindOpponentsInRange(owner.transform.position, CombatFaction.Player, config.AttackRange));
				targets.Sort((a,b) => (a.transform.position-owner.transform.position).sqrMagnitude.CompareTo((b.transform.position-owner.transform.position).sqrMagnitude));
				if (targets.Count > 0) System.Sound(config.ShotSound);
				for (var i = 0; i < 4 && targets.Count > 0; i++)
				{
					var position = state.Visuals[i] == null ? owner.transform.position : state.Visuals[i].transform.position;
					if (!CharacterCombatSystem.Visible(owner.transform.position, position)) position = owner.transform.position;
					var direction = ((Vector2)(targets[i % targets.Count].transform.position - position)).normalized;
					ProjectileFactory.Instance.Spawn(config, position, direction, CombatFaction.Player, state.Damage, config.MoveSpeed, 0);
				}
			}
			if (state.Age < config.Windup + config.Duration) return true;
			System.HitArea(owner.transform.position, Vector2.right, config.Radius, 360,
				(values?.FinalDamage ?? config.FinalDamage) * GameArchitecture.Interface.GetSystem<StatSystem>().GetAttackDamageMultiplier(), 0);
			System.Flash(config.EffectVisual, owner.transform.position, 0, config.Radius * 2, 0.3f, Color.white);
			System.Sound(config.EndSound);
			if (GameArchitecture.Interface.GetSystem<RunTimerSystem>().IsRunning()) GameArchitecture.Interface.SendEvent(new CombatFinisherEvent());
			return false;
		}
	}
}
