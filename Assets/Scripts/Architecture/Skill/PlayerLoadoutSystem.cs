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
				if (EquipSkill(owner, skillId)) equippedSkills++;
				else Debug.LogWarning($"Optional skill {skillId} was skipped.");
			}
			var dodgeEquipped = SetDodge(skillGroup.StartingDodgeId);
			return new SkillGroupEquipResult(true, equippedWeapons, equippedSkills, dodgeEquipped);
		}

		public bool EquipWeapon(GameObject owner, int weaponId)
		{
			var model = this.GetModel<PlayerLoadoutModel>();
			var weapon = this.GetUtility<WeaponCatalog>().Get(weaponId);
			if (owner == null || weapon == null || !model.HasAvailableWeaponSlot || model.HasWeapon(weaponId))
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

		public bool CanAcquireWeapon(int weaponId)
		{
			var model = this.GetModel<PlayerLoadoutModel>();
			var weapon = this.GetUtility<WeaponCatalog>().Get(weaponId);
			return weapon != null && weapon.CanAcquireDuringRun && model.HasAvailableWeaponSlot && !model.HasWeapon(weaponId) && !model.IsWeaponRetired(weaponId);
		}

		public bool AcquireWeapon(int weaponId)
		{
			return CanAcquireWeapon(weaponId) && EquipWeapon(this.GetModel<PlayerLoadoutModel>().Owner, weaponId);
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

		public bool CombineWeapons(IReadOnlyList<int> sourceRuntimeIds, int targetWeaponId)
		{
			var model = this.GetModel<PlayerLoadoutModel>();
			var target = this.GetUtility<WeaponCatalog>().Get(targetWeaponId);
			if (sourceRuntimeIds == null || sourceRuntimeIds.Count == 0 || model.Owner == null || target == null ||
				target.CanUpgrade || target.MaxLevel != 1 || model.HasWeapon(targetWeaponId) || !ValidateAttackIds(target.InitialAttackIds, targetWeaponId)) return false;
			var sources = new List<WeaponRuntimeData>();
			foreach (var runtimeId in sourceRuntimeIds)
			{
				var source = model.GetWeapon(runtimeId);
				if (source == null || sources.Contains(source)) return false;
				sources.Add(source);
			}
			if (sources.Count == 1)
			{
				var sourceWeaponId = sources[0].WeaponId;
				if (!ReplaceWeapon(sources[0].RuntimeId, targetWeaponId)) return false;
				model.RetireWeapon(sourceWeaponId);
				return true;
			}
			foreach (var source in sources) ReleaseAttacks(model.Owner, source.RuntimeId);
			foreach (var source in sources)
			{
				model.RetireWeapon(source.WeaponId);
				model.RemoveWeapon(source);
			}
			var runtime = model.AddWeapon(target.Id, false, target.InitialAttackIds);
			ConfigureAttacks(model.Owner, runtime);
			this.SendEvent(new WeaponEquippedEvent(runtime.RuntimeId, target.Id));
			return true;
		}

		public WeaponRuntimeData RestoreWeapon(GameObject owner, WeaponSaveData data)
		{
			var model = this.GetModel<PlayerLoadoutModel>();
			var weapon = data == null ? null : this.GetUtility<WeaponCatalog>().Get(data.WeaponId);
			if (owner == null || data == null || weapon == null || !model.HasAvailableWeaponSlot || model.HasWeapon(data.WeaponId) ||
				!ValidateAttackIds(weapon.InitialAttackIds, data.WeaponId)) return null;
			var runtime = model.AddWeapon(weapon.Id, weapon.CanUpgrade && weapon.MaxLevel > 1, weapon.InitialAttackIds, data.RuntimeId);
			ConfigureAttacks(owner, runtime);
			if (RestoreWeapon(runtime, data)) return runtime;
			RemoveWeapon(runtime.RuntimeId);
			return null;
		}

		public void RestoreRetiredWeaponsFromCombinations()
		{
			var combinations = this.GetUtility<WeaponCombinationCatalog>().Config;
			if (combinations == null) return;
			foreach (var combination in combinations.Combinations)
			{
				if (combination == null || !this.GetModel<PlayerLoadoutModel>().HasWeapon(combination.TargetWeaponId)) continue;
				foreach (var requirement in combination.RequiredWeapons)
					this.GetModel<PlayerLoadoutModel>().RetireWeapon(requirement?.WeaponId ?? 0);
			}
		}

		public bool ReplaceWeapon(int runtimeId, int targetWeaponId)
		{
			var model = this.GetModel<PlayerLoadoutModel>();
			var runtime = model.GetWeapon(runtimeId);
			var targetConfig = this.GetUtility<WeaponCatalog>().Get(targetWeaponId);
			if (runtime == null || targetConfig == null || (runtime.WeaponId != targetWeaponId && model.HasWeapon(targetWeaponId)) || !ValidateAttackIds(targetConfig.InitialAttackIds, targetWeaponId)) return false;

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

		public bool EquipSkill(GameObject owner, int skillId, int runtimeId = 0)
		{
			var skill = this.GetUtility<SkillCatalog>().Get(skillId);
			if (owner == null || skill == null || !ValidateAttackIds(skill.InitialAttackIds, $"Skill {skillId}")) return false;
			foreach (var existingSkill in this.GetModel<PlayerLoadoutModel>().Skills)
				if (existingSkill.SkillId == skillId) return true;
			var runtime = this.GetModel<PlayerLoadoutModel>().AddSkill(skill.Id, skill.CanUpgrade && skill.MaxLevel > 1, skill.InitialAttackIds, runtimeId);
			ConfigureAttacks(owner, runtime.AttackIds, runtime.RuntimeId);
			return true;
		}

		public void TryUseSkills()
		{
			var model = this.GetModel<PlayerLoadoutModel>();
			if (model.Owner == null) return;
			foreach (var skill in model.Skills)
				this.GetSystem<AttackSystem>().TryExecuteLoadout(model.Owner, skill.RuntimeId);
		}

		public bool UpgradeSkill(int runtimeId)
		{
			var runtime = this.GetModel<PlayerLoadoutModel>().GetSkill(runtimeId);
			var config = runtime == null ? null : this.GetUtility<SkillCatalog>().Get(runtime.SkillId);
			if (runtime == null || config == null || !runtime.CanUpgrade || runtime.Level >= config.MaxLevel) return false;
			runtime.Level++;
			this.SendEvent(new SkillUpgradedEvent(runtime.RuntimeId, runtime.SkillId, runtime.Level));
			return true;
		}

		public bool RestoreSkill(SkillRuntimeData runtime, int level)
		{
			var config = runtime == null ? null : this.GetUtility<SkillCatalog>().Get(runtime.SkillId);
			if (runtime == null || config == null) return false;
			runtime.Level = Mathf.Clamp(level, 1, Mathf.Max(1, config.MaxLevel));
			return true;
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

		public void Reset()
		{
			var model = this.GetModel<PlayerLoadoutModel>();
			foreach (var weapon in model.Weapons) ReleaseAttacks(model.Owner, weapon.RuntimeId);
			foreach (var skill in model.Skills) ReleaseAttacks(model.Owner, skill.RuntimeId);
			model.Reset();
			this.GetSystem<DodgeSystem>().Reset();
		}

		private bool ValidateAttackIds(IEnumerable<int> attackIds, object ownerId)
		{
			if (attackIds == null) return true;
			foreach (var attackId in attackIds)
			{
				var attack = this.GetUtility<AttackCatalog>().Get(attackId);
				var executor = attack == null ? null : this.GetUtility<AttackExecutorRegistry>().Get(attack.ExecutorId);
				if (executor != null) continue;
				Debug.LogError($"{ownerId} references an unavailable attack {attackId}.");
				return false;
			}
			return true;
		}

		private void ConfigureAttacks(GameObject owner, WeaponRuntimeData runtime)
		{
			if (runtime == null) return;
			ConfigureAttacks(owner, runtime.AttackIds, runtime.RuntimeId);
		}

		private void ConfigureAttacks(GameObject owner, IEnumerable<int> attackIds, int runtimeId)
		{
			if (owner == null || attackIds == null) return;
			foreach (var attackId in attackIds)
			{
				var attack = this.GetUtility<AttackCatalog>().Get(attackId);
				var executor = attack == null ? null : this.GetUtility<AttackExecutorRegistry>().Get(attack.ExecutorId);
				executor?.ConfigureOwner(owner, attack, CombatFaction.Player, runtimeId);
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
