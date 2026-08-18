using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class CharacterExclusiveSkillUpgradeCatalog : IUtility
	{
		public CharacterExclusiveSkillUpgradeConfig Config { get; }

		public CharacterExclusiveSkillUpgradeCatalog()
		{
			Config = Resources.Load<CharacterExclusiveSkillUpgradeConfig>("Configs/Progression/CharacterExclusiveSkillUpgradeCatalog");
		}

		public CharacterExclusiveSkillUpgradeDefinition Get(int characterId)
		{
			return Config == null ? null : Config.Upgrades.Find(upgrade => upgrade != null && upgrade.CharacterId == characterId);
		}
	}
}
