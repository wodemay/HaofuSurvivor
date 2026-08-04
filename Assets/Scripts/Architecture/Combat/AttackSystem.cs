using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class AttackSystem : AbstractSystem
	{
		private readonly Dictionary<int, AttackRuntime> mRuntimes = new();

		public void Register(int runtimeId, int attackId, CombatFaction ownerFaction)
		{
			var config = this.GetUtility<AttackCatalog>().Get(attackId);
			if (config == null) return;
			mRuntimes[runtimeId] = new AttackRuntime(config, ownerFaction);
		}

		public void Unregister(int runtimeId)
		{
			mRuntimes.Remove(runtimeId);
		}

		public void TryExecute(int runtimeId, CombatFaction targetFaction)
		{
			if (this.GetModel<RunModel>().Phase != RunPhase.Active) return;
			if (!mRuntimes.TryGetValue(runtimeId, out var runtime)) return;
			if (runtime.Config.TargetFaction != targetFaction || runtime.CooldownRemaining > 0f) return;

			var executor = this.GetUtility<AttackExecutorRegistry>().Get(runtime.Config.ExecutorId);
			if (executor == null) return;

			runtime.CooldownRemaining = Mathf.Max(0.01f, runtime.Config.Cooldown);
			executor.Execute(new AttackExecutionContext(runtime.OwnerFaction, targetFaction, runtime.Config), this.GetSystem<DamageSystem>());
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
			public readonly CombatFaction OwnerFaction;
			public float CooldownRemaining;

			public AttackRuntime(AttackConfig config, CombatFaction ownerFaction)
			{
				Config = config;
				OwnerFaction = ownerFaction;
			}
		}
	}
}
