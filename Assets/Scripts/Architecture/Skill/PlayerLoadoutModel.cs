using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class PlayerLoadoutModel : AbstractModel
	{
		private readonly List<WeaponRuntimeData> mWeapons = new();
		private readonly List<SkillRuntimeData> mSkills = new();
		private int mNextWeaponRuntimeId = 1;
		private int mNextSkillRuntimeId = -1;

		public IReadOnlyList<WeaponRuntimeData> Weapons => mWeapons;
		public IReadOnlyList<SkillRuntimeData> Skills => mSkills;
		public int DodgeId { get; internal set; }
		public GameObject Owner { get; private set; }

		public void BindOwner(GameObject owner)
		{
			Owner = owner;
		}

		public WeaponRuntimeData AddWeapon(int weaponId, bool canUpgrade, IEnumerable<int> attackIds)
		{
			var weapon = new WeaponRuntimeData(mNextWeaponRuntimeId++, weaponId, canUpgrade, attackIds);
			mWeapons.Add(weapon);
			return weapon;
		}

		public WeaponRuntimeData GetWeapon(int runtimeId)
		{
			return mWeapons.Find(weapon => weapon.RuntimeId == runtimeId);
		}

		public SkillRuntimeData AddSkill(int skillId, bool canUpgrade, IEnumerable<int> attackIds)
		{
			var skill = new SkillRuntimeData(mNextSkillRuntimeId--, skillId, canUpgrade, attackIds);
			mSkills.Add(skill);
			return skill;
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
