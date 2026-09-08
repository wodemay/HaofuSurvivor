using System.Collections.Generic;
using QFramework;

namespace HaoFuSurvivor
{
	public class MetaUpgradeModel : AbstractModel
	{
		private readonly Dictionary<int, int> mLevels = new();
		public int GetLevel(int upgradeId) => mLevels.TryGetValue(upgradeId, out var level) ? level : 0;
		public void SetLevel(int upgradeId, int level) { if (upgradeId != 0) mLevels[upgradeId] = level; }
		public void Reset() => mLevels.Clear();
		public IEnumerable<MetaUpgradeSaveData> GetSaveData()
		{
			foreach (var entry in mLevels) yield return new MetaUpgradeSaveData { UpgradeId = entry.Key, Level = entry.Value };
		}
		public void Restore(IEnumerable<MetaUpgradeSaveData> entries)
		{
			mLevels.Clear();
			if (entries == null) return;
			foreach (var entry in entries)
				if (entry != null && entry.UpgradeId != 0 && entry.Level > 0) mLevels[entry.UpgradeId] = entry.Level;
		}
		protected override void OnInit() => Reset();
	}
}
