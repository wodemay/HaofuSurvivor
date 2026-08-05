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

		public AttackExecutionContext(GameObject owner, CombatFaction ownerFaction, CombatEntity target, AttackConfig config)
		{
			Owner = owner;
			OwnerFaction = ownerFaction;
			Target = target;
			Config = config;
		}
	}

	public interface IAttackExecutor
	{
		string Id { get; }
		void ConfigureOwner(GameObject owner, AttackConfig config, CombatFaction ownerFaction);
		void Execute(AttackExecutionContext context);
	}

	public class AttackExecutorRegistry : IUtility
	{
		private readonly Dictionary<string, IAttackExecutor> mExecutors = new();

		public AttackExecutorRegistry()
		{
			Register(new CollisionAttackExecutor());
			Register(new ProjectileAttackExecutor());
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

		public void ConfigureOwner(GameObject owner, AttackConfig config, CombatFaction ownerFaction)
		{
			var trigger = AttackTriggerUtility.Find<CollisionAttackTrigger>(owner, config.Id);
			if (trigger == null) trigger = owner.AddComponent<CollisionAttackTrigger>();
			trigger.Initialize(config.Id, ownerFaction);
		}

		public void Execute(AttackExecutionContext context)
		{
			var multiplier = context.OwnerFaction == CombatFaction.Enemy
				? GameArchitecture.Interface.GetModel<RunTimerModel>().EnemyDamageMultiplier
				: 1f;
			GameArchitecture.Interface.GetSystem<DamageSystem>().ApplyDamage(context.Target, context.Config.Damage * multiplier);
		}
	}

	public class ProjectileAttackExecutor : IAttackExecutor
	{
		public string Id => "projectile";

		public void ConfigureOwner(GameObject owner, AttackConfig config, CombatFaction ownerFaction)
		{
			if (config.ExecutorParameterConfig is not ProjectileAttackParameterConfig parameters) return;
			var trigger = AttackTriggerUtility.Find<ProjectileAttackTrigger>(owner, config.Id);
			if (trigger == null) trigger = owner.AddComponent<ProjectileAttackTrigger>();
			trigger.Initialize(config.Id, ownerFaction, parameters.AttackRange);
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
			ProjectileFactory.Instance.Spawn(parameters, context.Owner.transform.position, direction.normalized,
				context.OwnerFaction, context.Config.Damage * multiplier);
		}
	}

	public interface IAttackTrigger
	{
		int AttackId { get; }
	}

	public static class AttackTriggerUtility
	{
		public static T Find<T>(GameObject owner, int attackId) where T : MonoBehaviour, IAttackTrigger
		{
			foreach (var trigger in owner.GetComponents<T>())
			{
				if (trigger.AttackId == attackId) return trigger;
			}
			return null;
		}
	}
}
