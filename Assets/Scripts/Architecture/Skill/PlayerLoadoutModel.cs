using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class PlayerLoadoutModel : AbstractModel
	{
		public const int MaxWeaponSlots = 6;

		private readonly List<WeaponRuntimeData> mWeapons = new();
		private readonly List<SkillRuntimeData> mSkills = new();
		private readonly HashSet<int> mRetiredWeaponIds = new();
		private int mNextWeaponRuntimeId = 1;
		private int mNextSkillRuntimeId = -1;

		public IReadOnlyList<WeaponRuntimeData> Weapons => mWeapons;
		public IReadOnlyList<SkillRuntimeData> Skills => mSkills;
		public bool HasAvailableWeaponSlot => mWeapons.Count < MaxWeaponSlots;
		public int DodgeId { get; internal set; }
		public GameObject Owner { get; private set; }

		public void BindOwner(GameObject owner)
		{
			Owner = owner;
		}

		public WeaponRuntimeData AddWeapon(int weaponId, bool canUpgrade, IEnumerable<int> attackIds, int runtimeId = 0)
		{
			var resolvedRuntimeId = runtimeId > 0 && GetWeapon(runtimeId) == null ? runtimeId : mNextWeaponRuntimeId;
			mNextWeaponRuntimeId = Mathf.Max(mNextWeaponRuntimeId, resolvedRuntimeId + 1);
			var weapon = new WeaponRuntimeData(resolvedRuntimeId, weaponId, canUpgrade, attackIds);
			mWeapons.Add(weapon);
			return weapon;
		}

		public WeaponRuntimeData GetWeapon(int runtimeId)
		{
			return mWeapons.Find(weapon => weapon.RuntimeId == runtimeId);
		}

		public bool HasWeapon(int weaponId)
		{
			return mWeapons.Exists(weapon => weapon.WeaponId == weaponId);
		}

		public bool IsWeaponRetired(int weaponId)
		{
			return mRetiredWeaponIds.Contains(weaponId);
		}

		public void RetireWeapon(int weaponId)
		{
			if (weaponId != 0) mRetiredWeaponIds.Add(weaponId);
		}

		public SkillRuntimeData AddSkill(int skillId, bool canUpgrade, IEnumerable<int> attackIds, int runtimeId = 0)
		{
			var resolvedRuntimeId = runtimeId < 0 && GetSkill(runtimeId) == null ? runtimeId : mNextSkillRuntimeId;
			mNextSkillRuntimeId = Mathf.Min(mNextSkillRuntimeId, resolvedRuntimeId - 1);
			var skill = new SkillRuntimeData(resolvedRuntimeId, skillId, canUpgrade, attackIds);
			mSkills.Add(skill);
			return skill;
		}

		public SkillRuntimeData GetSkill(int runtimeId)
		{
			return mSkills.Find(skill => skill.RuntimeId == runtimeId);
		}

		public SkillRuntimeData GetSkillById(int skillId)
		{
			return mSkills.Find(skill => skill.SkillId == skillId);
		}

		public void SetWeaponLevel(WeaponRuntimeData weapon, int level)
		{
			if (weapon != null) weapon.Level = level;
		}

		public void ReplaceWeapon(WeaponRuntimeData weapon, int weaponId, bool canUpgrade, IEnumerable<int> attackIds)
		{
			if (weapon == null) return;
			weapon.WeaponId = weaponId;
			weapon.Level = 1;
			weapon.CanUpgrade = canUpgrade;
			weapon.SetAttackIds(attackIds);
			weapon.ResetModifiers();
		}

		public void SetWeaponAttackIds(WeaponRuntimeData weapon, IEnumerable<int> attackIds)
		{
			weapon?.SetAttackIds(attackIds);
		}

		public void RemoveWeapon(WeaponRuntimeData weapon)
		{
			if (weapon != null) mWeapons.Remove(weapon);
		}

		public void Reset()
		{
			mWeapons.Clear();
			mSkills.Clear();
			mRetiredWeaponIds.Clear();
			DodgeId = 0;
			Owner = null;
			mNextWeaponRuntimeId = 1;
			mNextSkillRuntimeId = -1;
		}

		protected override void OnInit()
		{
			Reset();
		}
	}
}
