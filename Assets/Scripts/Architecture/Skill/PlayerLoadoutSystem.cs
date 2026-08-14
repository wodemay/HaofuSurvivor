using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public readonly struct SkillGroupEquipResult
	{
		public readonly bool CoreSucceeded;
		public readonly int EquippedWeaponCount;
		public readonly int EquippedSkillCount;
		public readonly bool DodgeEquipped;

		public SkillGroupEquipResult(bool coreSucceeded, int equippedWeaponCount, int equippedSkillCount, bool dodgeEquipped)
		{
			CoreSucceeded = coreSucceeded;
			EquippedWeaponCount = equippedWeaponCount;
			EquippedSkillCount = equippedSkillCount;
			DodgeEquipped = dodgeEquipped;
		}
	}

	public class PlayerLoadoutSystem : AbstractSystem
	{
		public SkillGroupEquipResult EquipInitialSkillGroup(GameObject owner, int skillGroupId)
		{
			Reset();
			this.GetModel<PlayerLoadoutModel>().BindOwner(owner);
			if (skillGroupId == 0) return new SkillGroupEquipResult(true, 0, 0, false);
			var skillGroup = this.GetUtility<SkillGroupCatalog>().Get(skillGroupId);
			if (skillGroup == null)
			{
				Debug.LogError($"Skill group {skillGroupId} was not found; required weapon loadout cannot be resolved.");
				return new SkillGroupEquipResult(false, 0, 0, false);
			}

			var equippedWeapons = 0;
			foreach (var weaponId in skillGroup.StartingWeaponIds)
			{
				if (EquipWeapon(owner, weaponId))
				{
					equippedWeapons++;
					continue;
				}
				if (skillGroup.RequireStartingWeapons)
				{
					Reset();
					return new SkillGroupEquipResult(false, equippedWeapons, 0, false);
				}
				Debug.LogWarning($"Optional weapon {weaponId} was skipped.");
			}
			var equippedSkills = 0;
			foreach (var skillId in skillGroup.StartingSkillIds)
			{
				if (skillId == 0) continue;
				AddSkill(skillId);
				equippedSkills++;
			}
			var dodgeEquipped = SetDodge(skillGroup.StartingDodgeId);
			return new SkillGroupEquipResult(true, equippedWeapons, equippedSkills, dodgeEquipped);
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

		public WeaponRuntimeData GetOrEquipWeapon(int weaponId)
		{
			foreach (var weapon in this.GetModel<PlayerLoadoutModel>().Weapons)
				if (weapon.WeaponId == weaponId) return weapon;
			return EquipWeapon(this.GetModel<PlayerLoadoutModel>().Owner, weaponId)
				? this.GetModel<PlayerLoadoutModel>().Weapons[this.GetModel<PlayerLoadoutModel>().Weapons.Count - 1]
				: null;
		}

		public bool RestoreWeapon(WeaponRuntimeData runtime, WeaponSaveData data)
		{
			if (runtime == null || data == null) return false;
			if (!ReplaceWeaponAttacks(runtime.RuntimeId, data.AttackIds)) return false;
			runtime.Level = Mathf.Max(1, data.Level);
			runtime.CanUpgrade = data.CanUpgrade;
			runtime.RestoreModifiers(data.Modifiers.ConvertAll(item => new WeaponModifierSnapshot(item.AttackId, item.Key, item.Value)));
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

		public bool SetDodge(int dodgeId)
		{
			this.GetModel<PlayerLoadoutModel>().DodgeId = dodgeId;
			if (this.GetSystem<DodgeSystem>().Equip(dodgeId)) return true;
			this.GetModel<PlayerLoadoutModel>().DodgeId = 0;
			this.GetSystem<DodgeSystem>().Reset();
			Debug.LogWarning($"Dodge {dodgeId} could not be equipped; dodge is disabled.");
			return false;
		}

		public bool RemoveWeapon(int runtimeId)
		{
			var model = this.GetModel<PlayerLoadoutModel>();
			var runtime = model.GetWeapon(runtimeId);
			if (runtime == null) return false;
			ReleaseAttacks(model.Owner, runtime.RuntimeId);
			model.RemoveWeapon(runtime);
			return true;
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
			this.GetSystem<DodgeSystem>().Reset();
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
