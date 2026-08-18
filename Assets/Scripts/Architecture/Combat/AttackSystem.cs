using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class AttackSystem : AbstractSystem, IRunUpdateable
	{
		private readonly Dictionary<int, AttackRuntime> mRuntimes = new();
		private readonly List<int> mRuntimeIdsToRemove = new();
		private readonly List<int> mRuntimeIdsToExecute = new();
		private int mNextAutomaticRuntimeId = -1;
		public bool HasRuntimes => mRuntimes.Count > 0;

		public void Register(int runtimeId, int attackId, GameObject owner, CombatFaction ownerFaction, int weaponRuntimeId)
		{
			var config = this.GetUtility<AttackCatalog>().Get(attackId);
			if (config == null) return;
			mRuntimes[runtimeId] = new AttackRuntime(config, owner, ownerFaction, weaponRuntimeId);
			if (this.GetSystem<RunTimerSystem>().IsRunning()) this.GetSystem<GameLoopSystem>().RegisterUpdateable(this);
		}

		public int RegisterAutomatic(GameObject owner, AttackConfig config, CombatFaction ownerFaction, int weaponRuntimeId)
		{
			return RegisterGenerated(owner, config, ownerFaction, weaponRuntimeId);
		}

		public int RegisterManual(GameObject owner, AttackConfig config, CombatFaction ownerFaction, int weaponRuntimeId)
		{
			return RegisterGenerated(owner, config, ownerFaction, weaponRuntimeId);
		}

		public void TryExecuteLoadout(GameObject owner, int weaponRuntimeId)
		{
			if (owner == null || !this.GetSystem<RunTimerSystem>().IsRunning()) return;
			mRuntimeIdsToExecute.Clear();
			foreach (var pair in mRuntimes)
				if (pair.Value.Owner == owner && pair.Value.WeaponRuntimeId == weaponRuntimeId) mRuntimeIdsToExecute.Add(pair.Key);
			foreach (var runtimeId in mRuntimeIdsToExecute) TryExecute(runtimeId, null);
			mRuntimeIdsToExecute.Clear();
		}

		private int RegisterGenerated(GameObject owner, AttackConfig config, CombatFaction ownerFaction, int weaponRuntimeId)
		{
			if (owner == null || config == null) return 0;
			while (mRuntimes.ContainsKey(mNextAutomaticRuntimeId)) mNextAutomaticRuntimeId--;
			var runtimeId = mNextAutomaticRuntimeId--;
			mRuntimes.Add(runtimeId, new AttackRuntime(config, owner, ownerFaction, weaponRuntimeId));
			if (this.GetSystem<RunTimerSystem>().IsRunning()) this.GetSystem<GameLoopSystem>().RegisterUpdateable(this);
			return runtimeId;
		}

		public void Unregister(int runtimeId)
		{
			mRuntimes.Remove(runtimeId);
			if (mRuntimes.Count == 0) this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		public void UnregisterOwner(GameObject owner, int weaponRuntimeId)
		{
			if (owner == null) return;
			mRuntimeIdsToRemove.Clear();
			foreach (var pair in mRuntimes)
				if (pair.Value.Owner == owner && pair.Value.WeaponRuntimeId == weaponRuntimeId) mRuntimeIdsToRemove.Add(pair.Key);
			foreach (var runtimeId in mRuntimeIdsToRemove) mRuntimes.Remove(runtimeId);
			mRuntimeIdsToRemove.Clear();
			if (mRuntimes.Count == 0) this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		public void TryExecute(int runtimeId, CombatEntity target)
		{
			if (!this.GetSystem<RunTimerSystem>().IsRunning()) return;
			if (!mRuntimes.TryGetValue(runtimeId, out var runtime)) return;
			var executor = this.GetUtility<AttackExecutorRegistry>().Get(runtime.Config.ExecutorId);
			if (executor == null) return;
			if ((target == null && executor.RequiresTarget) || (target != null && runtime.OwnerFaction == target.Faction) || runtime.CooldownRemaining > 0f) return;

			var weaponRuntime = GetWeaponRuntime(runtime);
			var cooldownMultiplier = weaponRuntime == null
				? 1f
				: weaponRuntime.GetModifierValue(runtime.Config.Id, WeaponUpgradeModifierKeys.AttackCooldownMultiplier, 1f);
			if (runtime.OwnerFaction == CombatFaction.Player)
			{
				cooldownMultiplier *= this.GetSystem<StatSystem>().GetCooldownMultiplier();
				if (weaponRuntime != null) cooldownMultiplier *= this.GetSystem<CharacterExclusivePerkSystem>().GetWeaponCooldownMultiplier();
			}
			runtime.CooldownRemaining = Mathf.Max(0.01f, runtime.Config.Cooldown * Mathf.Max(0.01f, cooldownMultiplier));
			var skillRuntime = GetSkillRuntime(runtime);
			executor.Execute(new AttackExecutionContext(runtime.Owner, runtime.OwnerFaction, target, runtime.Config, weaponRuntime, skillRuntime));
			if (skillRuntime != null) this.SendEvent(new SkillUsedEvent(skillRuntime.SkillId));
		}

		public void OnRunUpdate(float deltaTime)
		{
			foreach (var pair in mRuntimes)
			{
				var runtime = pair.Value;
				runtime.CooldownRemaining = Mathf.Max(0f, runtime.CooldownRemaining - deltaTime);
				if (runtime.CooldownRemaining > 0f) continue;
				var attackExecutor = this.GetUtility<AttackExecutorRegistry>().Get(runtime.Config.ExecutorId);
				if (attackExecutor is not IAutomaticAttackExecutor executor) continue;
				var target = executor.FindTarget(new AttackExecutionContext(runtime.Owner, runtime.OwnerFaction, null, runtime.Config, GetWeaponRuntime(runtime), GetSkillRuntime(runtime)));
				if (target != null || !attackExecutor.RequiresTarget) TryExecute(pair.Key, target);
			}
		}

		private WeaponRuntimeData GetWeaponRuntime(AttackRuntime runtime)
		{
			return runtime.WeaponRuntimeId == 0 ? null : this.GetModel<PlayerLoadoutModel>().GetWeapon(runtime.WeaponRuntimeId);
		}

		public IEnumerable<AttackCooldownSaveData> GetPlayerCooldownSaveData(GameObject owner)
		{
			foreach (var runtime in mRuntimes.Values)
				if (runtime.Owner == owner && runtime.OwnerFaction == CombatFaction.Player)
					yield return new AttackCooldownSaveData
					{
						RuntimeId = runtime.WeaponRuntimeId,
						AttackId = runtime.Config.Id,
						CooldownRemaining = runtime.CooldownRemaining
					};
		}

		public IEnumerable<AttackCooldownSaveData> GetCooldownSaveData(GameObject owner)
		{
			foreach (var runtime in mRuntimes.Values)
				if (runtime.Owner == owner)
					yield return new AttackCooldownSaveData { AttackId = runtime.Config.Id, CooldownRemaining = runtime.CooldownRemaining };
		}

		public void RestorePlayerCooldowns(GameObject owner, IEnumerable<AttackCooldownSaveData> entries)
		{
			if (owner == null || entries == null) return;
			foreach (var entry in entries)
			{
				if (entry == null) continue;
				foreach (var runtime in mRuntimes.Values)
					if (runtime.Owner == owner && runtime.OwnerFaction == CombatFaction.Player &&
						runtime.WeaponRuntimeId == entry.RuntimeId && runtime.Config.Id == entry.AttackId)
						runtime.CooldownRemaining = Mathf.Max(0f, entry.CooldownRemaining);
			}
		}

		public void RestoreCooldowns(GameObject owner, IEnumerable<AttackCooldownSaveData> entries)
		{
			if (owner == null || entries == null) return;
			foreach (var entry in entries)
			{
				if (entry == null) continue;
				foreach (var runtime in mRuntimes.Values)
					if (runtime.Owner == owner && runtime.Config.Id == entry.AttackId)
						runtime.CooldownRemaining = Mathf.Max(0f, entry.CooldownRemaining);
			}
		}

		private SkillRuntimeData GetSkillRuntime(AttackRuntime runtime)
		{
			return runtime.WeaponRuntimeId == 0 ? null : this.GetModel<PlayerLoadoutModel>().GetSkill(runtime.WeaponRuntimeId);
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
