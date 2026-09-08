using System;
using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	public enum MetaUpgradeType { MaxHealth, AttackDamage, MoveSpeed }

	[Serializable]
	public class MetaUpgradeDefinition
	{
		public int Id;
		public MetaUpgradeType Type;
		public string DisplayName;
		[TextArea] public string Description;
		public Sprite Icon;
		[Range(0f, 1f)] public float PercentPerLevel;
		public int MaxLevel = 5;
		public List<string> Costs = new();

		public string GetCost(int level) => Costs != null && level > 0 && level <= Costs.Count ? Costs[level - 1] : "0";
	}

	[CreateAssetMenu(menuName = "ProjectSurvivor/Progression/Meta Upgrade Catalog")]
	public class MetaUpgradeCatalogConfig : ScriptableObject
	{
		public List<MetaUpgradeDefinition> Upgrades = new();
	}

	public class MetaUpgradeCatalog : QFramework.IUtility
	{
		public MetaUpgradeCatalogConfig Config { get; }

		public MetaUpgradeCatalog() => Config = Resources.Load<MetaUpgradeCatalogConfig>("Configs/Progression/MetaUpgradeCatalog");

		public MetaUpgradeDefinition Get(int id) => Config == null ? null : Config.Upgrades.Find(item => item != null && item.Id == id);
	}
}
