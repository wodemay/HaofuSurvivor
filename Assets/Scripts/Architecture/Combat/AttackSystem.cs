using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class AttackSystem : AbstractSystem
	{
		private readonly Dictionary<int, AttackRuntime> mRuntimes = new();

		public void Register(int runtimeId, int attackId, GameObject owner, CombatFaction ownerFaction)
		{
			var config = this.GetUtility<AttackCatalog>().Get(attackId);
			if (config == null) return;
			mRuntimes[runtimeId] = new AttackRuntime(config, owner, ownerFaction);
		}

		public void Unregister(int runtimeId)
		{
			mRuntimes.Remove(runtimeId);
		}

		public void TryExecute(int runtimeId, CombatEntity target)
		{
			if (this.GetModel<RunModel>().Phase != RunPhase.Active) return;
			if (!mRuntimes.TryGetValue(runtimeId, out var runtime)) return;
			if (target == null || runtime.OwnerFaction == target.Faction || runtime.CooldownRemaining > 0f) return;

			var executor = this.GetUtility<AttackExecutorRegistry>().Get(runtime.Config.ExecutorId);
			if (executor == null) return;

			runtime.CooldownRemaining = Mathf.Max(0.01f, runtime.Config.Cooldown);
			executor.Execute(new AttackExecutionContext(runtime.Owner, runtime.OwnerFaction, target, runtime.Config));
		}

		public void Advance(float deltaTime)
		{
			foreach (var runtime in mRuntimes.Values)
			{
				runtime.CooldownRemaining = Mathf.Max(0f, runtime.CooldownRemaining - deltaTime);
			}
		}

		protected override void OnInit()
		{
		}

		private class AttackRuntime
		{
			public readonly AttackConfig Config;
			public readonly GameObject Owner;
			public readonly CombatFaction OwnerFaction;
			public float CooldownRemaining;

			public AttackRuntime(AttackConfig config, GameObject owner, CombatFaction ownerFaction)
			{
				Config = config;
				Owner = owner;
				OwnerFaction = ownerFaction;
			}
		}
	}
}
