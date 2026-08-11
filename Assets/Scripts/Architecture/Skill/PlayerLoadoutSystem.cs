using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class PlayerLoadoutSystem : AbstractSystem
	{
		public bool EquipInitialSkillGroup(GameObject owner, int skillGroupId)
		{
			Reset();
			this.GetModel<PlayerLoadoutModel>().BindOwner(owner);
			if (skillGroupId == 0) return true;
			var skillGroup = this.GetUtility<SkillGroupCatalog>().Get(skillGroupId);
			if (skillGroup == null)
			{
				Debug.LogError($"Skill group {skillGroupId} was not found.");
				return false;
			}

			foreach (var weaponId in skillGroup.StartingWeaponIds)
			{
				if (!EquipWeapon(owner, weaponId)) return false;
			}
			foreach (var skillId in skillGroup.StartingSkillIds) AddSkill(skillId);
			SetDodge(skillGroup.StartingDodgeId);
			return true;
		}

		public bool EquipWeapon(GameObject owner, int weaponId)
		{
			var model = this.GetModel<PlayerLoadoutModel>();
			var weapon = this.GetUtility<WeaponCatalog>().Get(weaponId);
			if (owner == null || weapon == null)
			{
				Debug.LogError($"Weapon {weaponId} could not be equipped.");
				return false;
			}

			if (!ValidateAttackIds(weapon.InitialAttackIds, weaponId)) return false;
			var runtime = model.AddWeapon(weaponId, weapon.CanUpgrade && weapon.MaxLevel > 1, weapon.InitialAttackIds);
			ConfigureAttacks(owner, runtime);
			this.SendEvent(new WeaponEquippedEvent(runtime.RuntimeId, weaponId));
			return true;
		}

		public bool UpgradeWeapon(int runtimeId)
		{
			var model = this.GetModel<PlayerLoadoutModel>();
			var runtime = model.GetWeapon(runtimeId);
			var config = runtime == null ? null : this.GetUtility<WeaponCatalog>().Get(runtime.WeaponId);
			if (runtime == null || config == null || !runtime.CanUpgrade || runtime.Level >= config.MaxLevel) return false;

			var nextLevel = runtime.Level + 1;
			var attackIds = new List<int>(runtime.AttackIds);
			var upgrade = config.LevelUpgrades.Find(item => item != null && item.Level == nextLevel);
			if (upgrade != null)
			{
				foreach (var attackId in upgrade.RemoveAttackIds) attackIds.Remove(attackId);
				foreach (var attackId in upgrade.AddAttackIds)
				{
					if (attackId != 0 && !attackIds.Contains(attackId)) attackIds.Add(attackId);
				}
			}

			if (!ReplaceWeaponAttacks(runtimeId, attackIds)) return false;
			runtime.ApplyModifiers(upgrade?.AttackModifiers);
			model.SetWeaponLevel(runtime, nextLevel);
			this.SendEvent(new WeaponUpgradedEvent(runtimeId, runtime.WeaponId, nextLevel));
			return true;
		}

		public bool TryEvolveWeapon(int runtimeId)
		{
			var runtime = this.GetModel<PlayerLoadoutModel>().GetWeapon(runtimeId);
			if (runtime == null) return false;
			var evolution = this.GetUtility<WeaponEvolutionCatalog>().Get(runtime.WeaponId, runtime.Level);
			var targetConfig = evolution == null ? null : this.GetUtility<WeaponCatalog>().Get(evolution.TargetWeaponId);
			if (targetConfig == null || targetConfig.CanUpgrade || targetConfig.MaxLevel != 1)
			{
				if (evolution != null) Debug.LogError($"Evolution target weapon {evolution.TargetWeaponId} must be level 1 and non-upgradeable.");
				return false;
			}
			if (!ReplaceWeapon(runtimeId, evolution.TargetWeaponId)) return false;
			this.SendEvent(new WeaponEvolvedEvent(runtimeId, evolution.SourceWeaponId, evolution.TargetWeaponId));
			return true;
		}

		public bool ReplaceWeapon(int runtimeId, int targetWeaponId)
		{
			var model = this.GetModel<PlayerLoadoutModel>();
			var runtime = model.GetWeapon(runtimeId);
			var targetConfig = this.GetUtility<WeaponCatalog>().Get(targetWeaponId);
			if (runtime == null || targetConfig == null || !ValidateAttackIds(targetConfig.InitialAttackIds, targetWeaponId)) return false;

			ReleaseAttacks(model.Owner, runtime.RuntimeId);
			model.ReplaceWeapon(runtime, targetConfig.Id, targetConfig.CanUpgrade && targetConfig.MaxLevel > 1, targetConfig.InitialAttackIds);
			ConfigureAttacks(model.Owner, runtime);
			this.SendEvent(new WeaponReplacedEvent(runtime.RuntimeId, targetConfig.Id));
			return true;
		}

		public bool ReplaceWeaponAttacks(int runtimeId, IEnumerable<int> attackIds)
		{
			var model = this.GetModel<PlayerLoadoutModel>();
			var runtime = model.GetWeapon(runtimeId);
			var newAttackIds = attackIds == null ? new List<int>() : new List<int>(attackIds);
			if (runtime == null || !ValidateAttackIds(newAttackIds, runtime.WeaponId)) return false;

			ReleaseAttacks(model.Owner, runtime.RuntimeId);
			model.SetWeaponAttackIds(runtime, newAttackIds);
			ConfigureAttacks(model.Owner, runtime);
			return true;
		}

		public void AddSkill(int skillId)
		{
			if (skillId == 0) return;
			var skills = this.GetModel<PlayerLoadoutModel>().SkillIds;
			if (!skills.Contains(skillId)) skills.Add(skillId);
		}

		public void SetDodge(int dodgeId)
		{
			this.GetModel<PlayerLoadoutModel>().DodgeId = dodgeId;
		}

		public bool HasUpgradeableWeapon()
		{
			foreach (var runtime in this.GetModel<PlayerLoadoutModel>().Weapons)
			{
				var config = this.GetUtility<WeaponCatalog>().Get(runtime.WeaponId);
				if (config != null && runtime.CanUpgrade && runtime.Level < config.MaxLevel) return true;
			}
			return false;
		}

		public bool HasEvolution(int runtimeId)
		{
			var runtime = this.GetModel<PlayerLoadoutModel>().GetWeapon(runtimeId);
			var evolution = runtime == null ? null : this.GetUtility<WeaponEvolutionCatalog>().Get(runtime.WeaponId, runtime.Level);
			var target = evolution == null ? null : this.GetUtility<WeaponCatalog>().Get(evolution.TargetWeaponId);
			return runtime != null && target != null && !target.CanUpgrade && target.MaxLevel == 1;
		}

		public void Reset()
		{
			var model = this.GetModel<PlayerLoadoutModel>();
			foreach (var weapon in model.Weapons) ReleaseAttacks(model.Owner, weapon.RuntimeId);
			model.Reset();
		}

		private bool ValidateAttackIds(IEnumerable<int> attackIds, int weaponId)
		{
			if (attackIds == null) return true;
			foreach (var attackId in attackIds)
			{
				var attack = this.GetUtility<AttackCatalog>().Get(attackId);
				var executor = attack == null ? null : this.GetUtility<AttackExecutorRegistry>().Get(attack.ExecutorId);
				if (executor != null) continue;
				Debug.LogError($"Weapon {weaponId} references an unavailable attack {attackId}.");
				return false;
			}
			return true;
		}

		private void ConfigureAttacks(GameObject owner, WeaponRuntimeData runtime)
		{
			if (owner == null || runtime == null) return;
			foreach (var attackId in runtime.AttackIds)
			{
				var attack = this.GetUtility<AttackCatalog>().Get(attackId);
				this.GetUtility<AttackExecutorRegistry>().Get(attack.ExecutorId)
					.ConfigureOwner(owner, attack, CombatFaction.Player, runtime.RuntimeId);
			}
		}

		private static void ReleaseAttacks(GameObject owner, int weaponRuntimeId)
		{
			AttackTriggerUtility.Remove(owner, weaponRuntimeId);
		}

		protected override void OnInit()
		{
		}
	}
}
