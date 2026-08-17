using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public readonly struct AttackExecutionContext
	{
		public readonly GameObject Owner;
		public readonly CombatFaction OwnerFaction;
		public readonly CombatEntity Target;
		public readonly AttackConfig Config;
		public readonly WeaponRuntimeData WeaponRuntime;

		public AttackExecutionContext(GameObject owner, CombatFaction ownerFaction, CombatEntity target, AttackConfig config,
			WeaponRuntimeData weaponRuntime)
		{
			Owner = owner;
			OwnerFaction = ownerFaction;
			Target = target;
			Config = config;
			WeaponRuntime = weaponRuntime;
		}

		public float GetModifierValue(string key, float defaultValue)
		{
			return WeaponRuntime == null ? defaultValue : WeaponRuntime.GetModifierValue(Config.Id, key, defaultValue);
		}
	}

	public interface IAttackExecutor
	{
		string Id { get; }
		void ConfigureOwner(GameObject owner, AttackConfig config, CombatFaction ownerFaction, int weaponRuntimeId = 0);
		void Execute(AttackExecutionContext context);
	}

	public interface IAutomaticAttackExecutor
	{
		bool RequiresTarget { get; }
		CombatEntity FindTarget(AttackExecutionContext context);
	}

	public class AttackExecutorRegistry : IUtility
	{
		private readonly Dictionary<string, IAttackExecutor> mExecutors = new();

		public AttackExecutorRegistry()
		{
			Register(new CollisionAttackExecutor());
			Register(new ProjectileAttackExecutor());
			Register(new BarrageProjectileAttackExecutor());
		}

		public void Register(IAttackExecutor executor)
		{
			mExecutors[executor.Id] = executor;
		}

		public IAttackExecutor Get(string id)
		{
			return string.IsNullOrEmpty(id) || !mExecutors.TryGetValue(id, out var executor) ? null : executor;
		}
	}

	public class CollisionAttackExecutor : IAttackExecutor
	{
		public string Id => "collision";

		public void ConfigureOwner(GameObject owner, AttackConfig config, CombatFaction ownerFaction, int weaponRuntimeId = 0)
		{
			var trigger = AttackTriggerUtility.Find<CollisionAttackTrigger>(owner, config.Id, weaponRuntimeId);
			if (trigger == null) trigger = owner.AddComponent<CollisionAttackTrigger>();
			trigger.Initialize(config.Id, ownerFaction, weaponRuntimeId);
		}

		public void Execute(AttackExecutionContext context)
		{
			var multiplier = context.OwnerFaction == CombatFaction.Enemy
				? GameArchitecture.Interface.GetModel<RunTimerModel>().EnemyDamageMultiplier
				: 1f;
			GameArchitecture.Interface.GetSystem<DamageSystem>().ApplyDamage(context.Target, context.Config.Damage * multiplier);
		}
	}

	public class ProjectileAttackExecutor : IAttackExecutor, IAutomaticAttackExecutor
	{
		public string Id => "projectile";
		public bool RequiresTarget => true;

		public void ConfigureOwner(GameObject owner, AttackConfig config, CombatFaction ownerFaction, int weaponRuntimeId = 0)
		{
			if (config.ExecutorParameterConfig is not ProjectileAttackParameterConfig) return;
			GameArchitecture.Interface.GetSystem<AttackSystem>().RegisterAutomatic(owner, config, ownerFaction, weaponRuntimeId);
		}

		public CombatEntity FindTarget(AttackExecutionContext context)
		{
			if (context.Config.ExecutorParameterConfig is not ProjectileAttackParameterConfig parameters || context.Owner == null) return null;
			return GameArchitecture.Interface.SendQuery(
				new FindClosestCombatTargetQuery(context.Owner.transform.position, context.OwnerFaction, parameters.AttackRange));
		}

		public void Execute(AttackExecutionContext context)
		{
			if (context.Config.ExecutorParameterConfig is not ProjectileAttackParameterConfig parameters ||
				parameters.ProjectilePrefab == null || context.Owner == null || context.Target == null) return;

			var direction = (Vector2)(context.Target.transform.position - context.Owner.transform.position);
			if (direction.sqrMagnitude <= Mathf.Epsilon) return;

			var multiplier = context.OwnerFaction == CombatFaction.Enemy
				? GameArchitecture.Interface.GetModel<RunTimerModel>().EnemyDamageMultiplier
				: 1f;
			var projectileCount = Mathf.Max(1, Mathf.RoundToInt(context.GetModifierValue(WeaponUpgradeModifierKeys.ProjectileCountAdd, 0f) + 1f));
			var damage = Mathf.Max(0f, context.Config.Damage + context.GetModifierValue(WeaponUpgradeModifierKeys.ProjectileDamageAdd, 0f));
			var speed = Mathf.Max(0f, parameters.MoveSpeed * context.GetModifierValue(WeaponUpgradeModifierKeys.ProjectileSpeedMultiplier, 1f));
			var pierce = Mathf.Max(0, Mathf.FloorToInt(context.GetModifierValue(WeaponUpgradeModifierKeys.ProjectilePierceAdd, 0f)));
			for (var index = 0; index < projectileCount; index++)
			{
				var angle = (index - (projectileCount - 1) * 0.5f) * 10f;
				var projectileDirection = Quaternion.Euler(0f, 0f, angle) * direction.normalized;
				ProjectileFactory.Instance.Spawn(parameters, context.Owner.transform.position, projectileDirection,
					context.OwnerFaction, damage * multiplier, speed, pierce);
			}
		}
	}

	public class BarrageProjectileAttackExecutor : IAttackExecutor, IAutomaticAttackExecutor
	{
		public string Id => "barrage-projectile";
		public bool RequiresTarget => false;

		public void ConfigureOwner(GameObject owner, AttackConfig config, CombatFaction ownerFaction, int weaponRuntimeId = 0)
		{
			if (config.ExecutorParameterConfig is not BarrageProjectileAttackParameterConfig) return;
			GameArchitecture.Interface.GetSystem<AttackSystem>().RegisterAutomatic(owner, config, ownerFaction, weaponRuntimeId);
		}

		public CombatEntity FindTarget(AttackExecutionContext context)
		{
			return null;
		}

		public void Execute(AttackExecutionContext context)
		{
			if (context.Config.ExecutorParameterConfig is not BarrageProjectileAttackParameterConfig parameters || context.Owner == null) return;
			var multiplier = context.OwnerFaction == CombatFaction.Enemy
				? GameArchitecture.Interface.GetModel<RunTimerModel>().EnemyDamageMultiplier
				: 1f;
			GameArchitecture.Interface.GetSystem<BarrageProjectileSystem>().Schedule(
				context.Owner, context.OwnerFaction, parameters, context.Config.Damage * multiplier);
		}
	}

	public interface IAttackTrigger
	{
		int AttackId { get; }
		int WeaponRuntimeId { get; }
		bool IsRegistered { get; }
		void Unregister();
	}

	public static class AttackTriggerUtility
	{
		public static T Find<T>(GameObject owner, int attackId, int weaponRuntimeId) where T : MonoBehaviour, IAttackTrigger
		{
			if (owner == null) return null;
			foreach (var trigger in owner.GetComponents<T>())
			{
				if (trigger.AttackId != attackId || trigger.WeaponRuntimeId != weaponRuntimeId) continue;
				if (weaponRuntimeId == 0 || trigger.IsRegistered) return trigger;
			}
			return null;
		}

		public static void Remove(GameObject owner, int weaponRuntimeId)
		{
			if (owner == null) return;
			GameArchitecture.Interface.GetSystem<AttackSystem>().UnregisterOwner(owner, weaponRuntimeId);
			foreach (var component in owner.GetComponents<MonoBehaviour>())
			{
				if (component is not IAttackTrigger trigger || trigger.WeaponRuntimeId != weaponRuntimeId) continue;
				trigger.Unregister();
				if (Application.isPlaying) Object.Destroy(component);
				else Object.DestroyImmediate(component);
			}
		}
	}
}
