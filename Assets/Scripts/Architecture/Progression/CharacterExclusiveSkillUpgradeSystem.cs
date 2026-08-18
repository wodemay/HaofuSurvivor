using QFramework;

namespace HaoFuSurvivor
{
	public class CharacterExclusiveSkillUpgradeSystem : AbstractSystem
	{
		public CharacterExclusiveSkillUpgradeDefinition GetEligible()
		{
			var player = this.GetModel<PlayerModel>();
			var definition = this.GetUtility<CharacterExclusiveSkillUpgradeCatalog>().Get(player.CharacterId);
			var character = this.GetUtility<CharacterCatalog>().Get(player.CharacterId);
			var skillGroup = character == null ? null : this.GetUtility<SkillGroupCatalog>().Get(character.SkillGroupId);
			var skill = this.GetModel<PlayerLoadoutModel>().GetSkillById(definition?.SkillId ?? 0);
			var skillConfig = skill == null ? null : this.GetUtility<SkillCatalog>().Get(skill.SkillId);
			if (!player.IsRegistered || definition == null || skillGroup == null || skillConfig == null || skill == null ||
				!skillGroup.StartingWeaponIds.Contains(definition.ExclusiveWeaponId) ||
				!skillGroup.StartingSkillIds.Contains(definition.SkillId) ||
				skillGroup.StartingDodgeId != definition.ExclusiveDodgeId || !skill.CanUpgrade || skill.Level >= skillConfig.MaxLevel)
				return null;
			return IsWeaponComplete(definition) && IsDodgeComplete(definition) ? definition : null;
		}

		public bool Upgrade(int skillId)
		{
			var definition = GetEligible();
			if (definition == null || definition.SkillId != skillId) return false;
			var skill = this.GetModel<PlayerLoadoutModel>().GetSkillById(skillId);
			return skill != null && this.GetSystem<PlayerLoadoutSystem>().UpgradeSkill(skill.RuntimeId);
		}

		private bool IsWeaponComplete(CharacterExclusiveSkillUpgradeDefinition definition)
		{
			var weapons = this.GetModel<PlayerLoadoutModel>().Weapons;
			foreach (var weapon in weapons)
			{
				if (definition.CompletedWeaponIds.Contains(weapon.WeaponId)) return true;
				if (weapon.WeaponId != definition.ExclusiveWeaponId) continue;
				var config = this.GetUtility<WeaponCatalog>().Get(weapon.WeaponId);
				return config != null && weapon.Level >= config.MaxLevel;
			}
			return false;
		}

		private bool IsDodgeComplete(CharacterExclusiveSkillUpgradeDefinition definition)
		{
			var dodge = this.GetModel<DodgeModel>().Runtime;
			if (dodge == null) return false;
			if (definition.CompletedDodgeIds.Contains(dodge.DodgeId)) return true;
			if (dodge.DodgeId != definition.ExclusiveDodgeId) return false;
			var config = this.GetUtility<DodgeCatalog>().Get(dodge.DodgeId);
			return config != null && dodge.Level >= config.MaxLevel;
		}

		protected override void OnInit()
		{
		}
	}
}
