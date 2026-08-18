using System;
using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	public enum PlayerStatUpgradeType
	{
		AttackDamage,
		CooldownReduction,
		ExperienceAbsorbRadius,
		ExperienceMultiplier,
		MoveSpeed,
		NaturalHealthRegeneration,
		RecoveryEfficiency
	}

	[Serializable]
	public class StatUpgradeDefinition
	{
		public int Id;
		public PlayerStatUpgradeType Type;
		public string DisplayName;
		[TextArea] public string Description;
		public Sprite Icon;
		[Range(0f, 1f)] public float PercentPerLevel;
		public int MaxLevel = 1;
	}

	[CreateAssetMenu(menuName = "ProjectSurvivor/Progression/Stat Upgrade Catalog")]
	public class StatUpgradeCatalogConfig : ScriptableObject
	{
		public List<StatUpgradeDefinition> Upgrades = new();
	}

	public class StatUpgradeCatalog : QFramework.IUtility
	{
		public StatUpgradeCatalogConfig Config { get; }

		public StatUpgradeCatalog()
		{
			Config = Resources.Load<StatUpgradeCatalogConfig>("Configs/Progression/StatUpgradeCatalog");
		}

		public StatUpgradeDefinition Get(int id)
		{
			return Config == null ? null : Config.Upgrades.Find(upgrade => upgrade != null && upgrade.Id == id);
		}
	}
}
