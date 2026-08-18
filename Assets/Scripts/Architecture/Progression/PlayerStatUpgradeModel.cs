using System.Collections.Generic;

namespace HaoFuSurvivor
{
	public class PlayerStatUpgradeModel : QFramework.AbstractModel
	{
		public const int MaxSlots = 6;
		private readonly Dictionary<int, int> mLevels = new();

		public int OccupiedSlotCount => mLevels.Count;

		public int GetLevel(int upgradeId)
		{
			return mLevels.TryGetValue(upgradeId, out var level) ? level : 0;
		}

		public void SetLevel(int upgradeId, int level)
		{
			if (upgradeId == 0) return;
			mLevels[upgradeId] = level;
		}

		public bool CanUpgrade(int upgradeId, int maxLevel)
		{
			var level = GetLevel(upgradeId);
			return level > 0 ? level < maxLevel : OccupiedSlotCount < MaxSlots;
		}

		public void Reset()
		{
			mLevels.Clear();
		}

		public IEnumerable<StatUpgradeSaveData> GetSaveData()
		{
			foreach (var entry in mLevels)
				yield return new StatUpgradeSaveData { UpgradeId = entry.Key, Level = entry.Value };
		}

		public void Restore(IEnumerable<StatUpgradeSaveData> entries)
		{
			mLevels.Clear();
			if (entries == null) return;
			foreach (var entry in entries)
				if (entry != null && entry.UpgradeId != 0 && entry.Level > 0) mLevels[entry.UpgradeId] = entry.Level;
		}

		protected override void OnInit()
		{
			Reset();
		}
	}
}
