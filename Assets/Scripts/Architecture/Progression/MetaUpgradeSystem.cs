using System.Collections.Generic;
using QFramework;

namespace HaoFuSurvivor
{
	public class MetaUpgradeSystem : AbstractSystem
	{
		public IReadOnlyList<MetaUpgradeDefinition> Definitions => this.GetUtility<MetaUpgradeCatalog>().Config?.Upgrades;
		public int GetLevel(int upgradeId) => this.GetModel<MetaUpgradeModel>().GetLevel(upgradeId);
		public bool CanUpgrade(int upgradeId)
		{
			var definition = this.GetUtility<MetaUpgradeCatalog>().Get(upgradeId);
			return definition != null && GetLevel(upgradeId) < definition.MaxLevel;
		}
		public BigCoin GetNextCost(int upgradeId)
		{
			var definition = this.GetUtility<MetaUpgradeCatalog>().Get(upgradeId);
			return definition == null ? BigCoin.Zero : new BigCoin(definition.GetCost(GetLevel(upgradeId) + 1));
		}
		public bool TryUpgrade(int upgradeId)
		{
			if (!CanUpgrade(upgradeId)) return false;
			if (!this.GetSystem<ProfileSystem>().TrySpendCoins(GetNextCost(upgradeId))) return false;
			var model = this.GetModel<MetaUpgradeModel>();
			var level = model.GetLevel(upgradeId) + 1;
			model.SetLevel(upgradeId, level);
			this.GetSystem<ProfileSystem>().SaveNow();
			this.SendEvent(new MetaUpgradeChangedEvent(upgradeId, level));
			return true;
		}
		public float GetTotalPercent(MetaUpgradeType type)
		{
			var config = this.GetUtility<MetaUpgradeCatalog>().Config;
			if (config == null) return 0f;
			var model = this.GetModel<MetaUpgradeModel>();
			var total = 0f;
			foreach (var definition in config.Upgrades)
				if (definition != null && definition.Type == type) total += definition.PercentPerLevel * model.GetLevel(definition.Id);
			return total;
		}
		public float GetAttackDamageMultiplier() => 1f + GetTotalPercent(MetaUpgradeType.AttackDamage);
		public void Restore(IEnumerable<MetaUpgradeSaveData> entries)
		{
			var catalog = this.GetUtility<MetaUpgradeCatalog>();
			var restored = new List<MetaUpgradeSaveData>();
			if (entries != null)
				foreach (var entry in entries)
				{
					var definition = entry == null ? null : catalog.Get(entry.UpgradeId);
					if (definition != null && entry.Level > 0) restored.Add(new MetaUpgradeSaveData { UpgradeId = definition.Id, Level = System.Math.Min(entry.Level, definition.MaxLevel) });
				}
			this.GetModel<MetaUpgradeModel>().Restore(restored);
		}
		protected override void OnInit() => this.GetModel<MetaUpgradeModel>().Reset();
	}
}
