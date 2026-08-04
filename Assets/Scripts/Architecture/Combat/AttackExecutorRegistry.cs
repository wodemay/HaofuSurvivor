using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public readonly struct AttackExecutionContext
	{
		public readonly CombatFaction OwnerFaction;
		public readonly CombatFaction TargetFaction;
		public readonly AttackConfig Config;

		public AttackExecutionContext(CombatFaction ownerFaction, CombatFaction targetFaction, AttackConfig config)
		{
			OwnerFaction = ownerFaction;
			TargetFaction = targetFaction;
			Config = config;
		}
	}

	public interface IAttackExecutor
	{
		string Id { get; }
		void ConfigureOwner(GameObject owner, AttackConfig config, CombatFaction ownerFaction);
		void Execute(AttackExecutionContext context, DamageSystem damageSystem);
	}

	public class AttackExecutorRegistry : IUtility
	{
		private readonly Dictionary<string, IAttackExecutor> mExecutors = new();

		public AttackExecutorRegistry()
		{
			Register(new ContactAttackExecutor());
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

	public class ContactAttackExecutor : IAttackExecutor
	{
		public string Id => "contact";

		public void ConfigureOwner(GameObject owner, AttackConfig config, CombatFaction ownerFaction)
		{
			var trigger = owner.GetComponent<ContactAttackTrigger>();
			if (trigger == null) trigger = owner.AddComponent<ContactAttackTrigger>();
			trigger.Initialize(config.Id, ownerFaction);
		}

		public void Execute(AttackExecutionContext context, DamageSystem damageSystem)
		{
			var multiplier = context.OwnerFaction == CombatFaction.Enemy
				? GameArchitecture.Interface.GetModel<RunTimerModel>().EnemyDamageMultiplier
				: 1f;
			damageSystem.ApplyDamage(context.TargetFaction, context.Config.Damage * multiplier);
		}
	}
}
