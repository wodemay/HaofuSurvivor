using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public interface IDodgeEffectExecutor
	{
		void Begin(CharacterCombatSystem system, DodgeConfig config, DodgeRuntimeData runtime);
		void Tick(CharacterCombatSystem system, DodgeConfig config, DodgeRuntimeData runtime, Vector2 before, Vector2 after);
		void End(CharacterCombatSystem system, DodgeConfig config, DodgeRuntimeData runtime);
	}

	public class DashEffectExecutor : IDodgeEffectExecutor
	{
		protected virtual Color Tint => new Color(0.25f, 0.8f, 1, 0.6f);
		protected virtual int TrailLimit => 3;
		public virtual void Begin(CharacterCombatSystem system, DodgeConfig config, DodgeRuntimeData runtime)
		{
			if (!runtime.IsRestoring && system.Owner != null)
				system.Flash(config.BurstPrefab, system.Owner.transform.position, 0, 0.8f, 0.15f, Tint);
		}
		public virtual void Tick(CharacterCombatSystem system, DodgeConfig config, DodgeRuntimeData runtime, Vector2 before, Vector2 after)
		{
			if (system.Owner == null || system.State.TrailCount >= TrailLimit) return;
			var total = GameArchitecture.Interface.GetSystem<DodgeSystem>().GetDuration(config, runtime.Level);
			if (total - runtime.DurationRemaining < system.State.TrailCount * total / TrailLimit) return;
			system.State.TrailCount++;
			var body = system.Owner.GetComponentInChildren<ActorFrameAnimationView>();
			var source = body == null ? system.Owner.GetComponentInChildren<SpriteRenderer>() : body.GetComponent<SpriteRenderer>();
			if (source == null) return;
			var ghost = system.Flash(config.TrailPrefab, source.transform.position, source.transform.eulerAngles.z, 1, 0.18f, Tint);
			if (ghost == null) return;
			var renderer = ghost.GetComponent<SpriteRenderer>();
			renderer.sprite = source.sprite; renderer.flipX = source.flipX; renderer.flipY = source.flipY;
			ghost.transform.localScale = source.transform.lossyScale;
			renderer.sortingOrder = source.sortingOrder - 1;
		}
		public virtual void End(CharacterCombatSystem system, DodgeConfig config, DodgeRuntimeData runtime)
		{
			if (system.Owner != null) system.Flash(config.BurstPrefab, GameArchitecture.Interface.GetModel<PlayerModel>().Position, 0, 0.7f, 0.15f, Tint);
		}
	}

	public class SwordDashEffectExecutor : DashEffectExecutor
	{
		protected override Color Tint => new Color(1, 0.8f, 0.35f, 0.7f);
		public override void Tick(CharacterCombatSystem system, DodgeConfig config, DodgeRuntimeData runtime, Vector2 before, Vector2 after)
		{
			var count = system.State.TrailCount;
			base.Tick(system, config, runtime, before, after);
			if (system.State.TrailCount != count)
				system.Flash(config.ThrustPrefab, after, Mathf.Atan2(runtime.Direction.y, runtime.Direction.x) * Mathf.Rad2Deg, 1.6f, 0.12f, Tint);
			var segment = after - before;
			var radius = config.PathWidth * 0.5f;
			var targets = GameArchitecture.Interface.GetSystem<CombatTargetSystem>().FindOpponentsInRange((before + after) * 0.5f, CombatFaction.Player, segment.magnitude * 0.5f + radius + 0.5f);
			foreach (var target in targets)
			{
				if (target == null || !target.isActiveAndEnabled || system.State.DodgeHits.Contains(target.SpawnId)) continue;
				var position = (Vector2)target.transform.position;
				var t = segment.sqrMagnitude < 0.00001f ? 0 : Mathf.Clamp01(Vector2.Dot(position - before, segment) / segment.sqrMagnitude);
				var closest = before + segment * t;
				var collider = target.GetComponent<Collider2D>();
				var point = collider == null ? position : collider.ClosestPoint(closest);
				if ((point - closest).sqrMagnitude > radius * radius || !CharacterCombatSystem.Visible(closest, position)) continue;
				system.State.DodgeHits.Add(target.SpawnId);
				GameArchitecture.Interface.GetSystem<EnemySystem>().Knockback(target, runtime.Direction * config.KnockbackDistance);
				var damage = runtime.Level >= config.DamageUpgradeLevel ? config.UpgradedDamage : config.Damage;
				GameArchitecture.Interface.GetSystem<DamageSystem>().ApplyDamage(target, damage * GameArchitecture.Interface.GetSystem<StatSystem>().GetAttackDamageMultiplier());
				system.Flash(config.BurstPrefab, position, 0, 0.45f, 0.1f, Tint);
			}
		}
	}

	public class ReloadDashEffectExecutor : DashEffectExecutor
	{
		protected override Color Tint => new Color(0.2f, 1, 0.65f, 0.6f);
		protected override int TrailLimit => 4;
		public override void End(CharacterCombatSystem system, DodgeConfig config, DodgeRuntimeData runtime)
		{
			base.End(system, config, runtime);
			system.State.EmpowerRemaining = config.ReloadBuffDuration;
			system.State.EmpowerMultiplier = runtime.Level >= config.ReloadUpgradeLevel ? config.UpgradedReloadDamageMultiplier : config.ReloadDamageMultiplier;
		}
	}
}
