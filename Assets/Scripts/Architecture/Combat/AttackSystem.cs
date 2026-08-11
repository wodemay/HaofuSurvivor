using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class AttackSystem : AbstractSystem
	{
		private readonly Dictionary<int, AttackRuntime> mRuntimes = new();

		public void Register(int runtimeId, int attackId, GameObject owner, CombatFaction ownerFaction, int weaponRuntimeId)
		{
			var config = this.GetUtility<AttackCatalog>().Get(attackId);
			if (config == null) return;
			mRuntimes[runtimeId] = new AttackRuntime(config, owner, ownerFaction, weaponRuntimeId);
		}

		public void Unregister(int runtimeId)
		{
			mRuntimes.Remove(runtimeId);
		}

		public void TryExecute(int runtimeId, CombatEntity target)
		{
			if (!this.GetSystem<RunTimerSystem>().IsRunning()) return;
			if (!mRuntimes.TryGetValue(runtimeId, out var runtime)) return;
			if (target == null || runtime.OwnerFaction == target.Faction || runtime.CooldownRemaining > 0f) return;

			var executor = this.GetUtility<AttackExecutorRegistry>().Get(runtime.Config.ExecutorId);
			if (executor == null) return;

			var weaponRuntime = runtime.WeaponRuntimeId == 0 ? null : this.GetModel<PlayerLoadoutModel>().GetWeapon(runtime.WeaponRuntimeId);
			var cooldownMultiplier = weaponRuntime == null
				? 1f
				: weaponRuntime.GetModifierValue(runtime.Config.Id, WeaponUpgradeModifierKeys.AttackCooldownMultiplier, 1f);
			runtime.CooldownRemaining = Mathf.Max(0.01f, runtime.Config.Cooldown * Mathf.Max(0.01f, cooldownMultiplier));
			executor.Execute(new AttackExecutionContext(runtime.Owner, runtime.OwnerFaction, target, runtime.Config, weaponRuntime));
		}

		public void Advance()
		{
			var deltaTime = this.GetModel<RunTimerModel>().DeltaTime;
			if (deltaTime <= 0f) return;
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
			public readonly int WeaponRuntimeId;
			public float CooldownRemaining;

			public AttackRuntime(AttackConfig config, GameObject owner, CombatFaction ownerFaction, int weaponRuntimeId)
			{
				Config = config;
				Owner = owner;
				OwnerFaction = ownerFaction;
				WeaponRuntimeId = weaponRuntimeId;
			}
		}
	}
}
