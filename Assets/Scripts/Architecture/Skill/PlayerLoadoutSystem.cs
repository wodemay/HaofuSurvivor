using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class PlayerLoadoutSystem : AbstractSystem
	{
		public bool EquipInitialSkillGroup(GameObject owner, int skillGroupId)
		{
			Reset();
			if (skillGroupId == 0) return true;
			var skillGroup = this.GetUtility<SkillGroupCatalog>().Get(skillGroupId);
			if (skillGroup == null)
			{
				Debug.LogError($"Skill group {skillGroupId} was not found.");
				return false;
			}

			foreach (var weaponId in skillGroup.StartingWeaponIds) EquipWeapon(owner, weaponId);
			foreach (var skillId in skillGroup.StartingSkillIds) AddSkill(skillId);
			SetDodge(skillGroup.StartingDodgeId);
			return true;
		}

		public bool EquipWeapon(GameObject owner, int weaponId)
		{
			var model = this.GetModel<PlayerLoadoutModel>();
			if (model.WeaponIds.Contains(weaponId)) return true;
			var weapon = this.GetUtility<WeaponCatalog>().Get(weaponId);
			if (owner == null || weapon == null)
			{
				Debug.LogError($"Weapon {weaponId} could not be equipped.");
				return false;
			}

			model.WeaponIds.Add(weaponId);
			foreach (var attackId in weapon.AttackIds)
			{
				var attack = this.GetUtility<AttackCatalog>().Get(attackId);
				var executor = attack == null ? null : this.GetUtility<AttackExecutorRegistry>().Get(attack.ExecutorId);
				if (executor == null)
				{
					Debug.LogError($"Weapon {weaponId} references an unavailable attack {attackId}.");
					continue;
				}
				executor.ConfigureOwner(owner, attack, CombatFaction.Player);
			}
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

		public void Reset()
		{
			this.GetModel<PlayerLoadoutModel>().Reset();
		}

		protected override void OnInit()
		{
		}
	}
}
