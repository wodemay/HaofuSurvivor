using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class PlayerStatUpgradeSystem : AbstractSystem
	{
		public bool HasUpgrade(int upgradeId)
		{
			var definition = this.GetUtility<StatUpgradeCatalog>().Get(upgradeId);
			return definition != null && this.GetModel<PlayerStatUpgradeModel>().CanUpgrade(upgradeId, definition.MaxLevel);
		}

		public bool Upgrade(int upgradeId)
		{
			var definition = this.GetUtility<StatUpgradeCatalog>().Get(upgradeId);
			var model = this.GetModel<PlayerStatUpgradeModel>();
			if (definition == null || !model.CanUpgrade(upgradeId, definition.MaxLevel)) return false;
			model.SetLevel(upgradeId, model.GetLevel(upgradeId) + 1);
			Recalculate();
			this.SendEvent(new PlayerStatUpgradedEvent(upgradeId, model.GetLevel(upgradeId)));
			return true;
		}

		public int GetLevel(int upgradeId)
		{
			return this.GetModel<PlayerStatUpgradeModel>().GetLevel(upgradeId);
		}

		public void Reset()
		{
			this.GetModel<PlayerStatUpgradeModel>().Reset();
			Recalculate();
		}

		public void Restore(System.Collections.Generic.IEnumerable<StatUpgradeSaveData> entries)
		{
			var catalog = this.GetUtility<StatUpgradeCatalog>();
			var restored = new System.Collections.Generic.List<StatUpgradeSaveData>();
			if (entries != null)
				foreach (var entry in entries)
				{
					var definition = entry == null ? null : catalog.Get(entry.UpgradeId);
					if (definition != null && entry.Level > 0)
						restored.Add(new StatUpgradeSaveData { UpgradeId = definition.Id, Level = Mathf.Min(entry.Level, definition.MaxLevel) });
				}
			this.GetModel<PlayerStatUpgradeModel>().Restore(restored);
			Recalculate();
		}

		private void Recalculate()
		{
			var stats = this.GetModel<PlayerStatModel>();
			stats.AttackDamageMultiplier = 1f + GetTotalPercent(PlayerStatUpgradeType.AttackDamage);
			stats.CooldownMultiplier = Mathf.Max(0.01f, 1f - GetTotalPercent(PlayerStatUpgradeType.CooldownReduction));
			stats.ExperienceMultiplier = 1f + GetTotalPercent(PlayerStatUpgradeType.ExperienceMultiplier);
			stats.RecoveryEfficiencyMultiplier = 1f + GetTotalPercent(PlayerStatUpgradeType.RecoveryEfficiency);
			stats.NaturalHealthRegenerationRatio = GetTotalPercent(PlayerStatUpgradeType.NaturalHealthRegeneration);
			stats.MoveSpeed = stats.BaseMoveSpeed * (1f + GetTotalPercent(PlayerStatUpgradeType.MoveSpeed));
			stats.ExperienceAbsorbRadius = stats.BaseExperienceAbsorbRadius * (1f + GetTotalPercent(PlayerStatUpgradeType.ExperienceAbsorbRadius));
		}

		private float GetTotalPercent(PlayerStatUpgradeType type)
		{
			var catalog = this.GetUtility<StatUpgradeCatalog>().Config;
			if (catalog == null) return 0f;
			var model = this.GetModel<PlayerStatUpgradeModel>();
			var total = 0f;
			foreach (var definition in catalog.Upgrades)
				if (definition != null && definition.Type == type) total += definition.PercentPerLevel * model.GetLevel(definition.Id);
			return total;
		}

		protected override void OnInit()
		{
			Reset();
		}
	}
}
