using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class WeaponCombinationSystem : AbstractSystem
	{
		public IReadOnlyList<WeaponCombinationConfig> GetEligible()
		{
			var eligible = new List<WeaponCombinationConfig>();
			var catalog = this.GetUtility<WeaponCombinationCatalog>().Config;
			if (catalog == null) return eligible;
			foreach (var combination in catalog.Combinations)
				if (CanCombine(combination)) eligible.Add(combination);
			return eligible;
		}

		public bool Combine(int combinationId)
		{
			var combination = this.GetUtility<WeaponCombinationCatalog>().Get(combinationId);
			if (!CanCombine(combination)) return false;
			var runtimeIds = new List<int>();
			foreach (var requirement in combination.RequiredWeapons)
			{
				var runtime = FindWeapon(requirement.WeaponId);
				if (runtime == null || runtimeIds.Contains(runtime.RuntimeId)) return false;
				runtimeIds.Add(runtime.RuntimeId);
			}
			if (!this.GetSystem<PlayerLoadoutSystem>().CombineWeapons(runtimeIds, combination.TargetWeaponId)) return false;
			this.SendEvent(new WeaponCombinedEvent(combination.Id, combination.TargetWeaponId));
			return true;
		}

		private bool CanCombine(WeaponCombinationConfig combination)
		{
			if (combination == null || combination.TargetWeaponId == 0 || combination.RequiredWeapons == null || combination.RequiredWeapons.Count == 0) return false;
			var target = this.GetUtility<WeaponCatalog>().Get(combination.TargetWeaponId);
			var loadout = this.GetModel<PlayerLoadoutModel>();
			if (target == null || target.CanUpgrade || target.MaxLevel != 1 || loadout.HasWeapon(target.Id)) return false;
			var requiredWeaponIds = new HashSet<int>();
			foreach (var requirement in combination.RequiredWeapons)
			{
				if (requirement == null || requirement.WeaponId == 0 || !requiredWeaponIds.Add(requirement.WeaponId)) return false;
				var runtime = FindWeapon(requirement.WeaponId);
				if (runtime == null || runtime.Level < Mathf.Max(1, requirement.RequiredLevel)) return false;
			}
			if (combination.RequiredStatUpgrades == null) return true;
			foreach (var requirement in combination.RequiredStatUpgrades)
			{
				if (requirement == null || requirement.StatUpgradeId == 0 ||
					this.GetSystem<PlayerStatUpgradeSystem>().GetLevel(requirement.StatUpgradeId) < Mathf.Max(1, requirement.RequiredLevel)) return false;
			}
			return true;
		}

		private WeaponRuntimeData FindWeapon(int weaponId)
		{
			foreach (var runtime in this.GetModel<PlayerLoadoutModel>().Weapons)
				if (runtime.WeaponId == weaponId) return runtime;
			return null;
		}

		protected override void OnInit()
		{
		}
	}
}
