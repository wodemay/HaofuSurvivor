using System.Collections.Generic;
using QFramework;

namespace HaoFuSurvivor
{
	public class CharacterExclusivePerkModel : AbstractModel
	{
		private readonly Dictionary<int, int> mLevels = new();

		public int GetLevel(int perkId)
		{
			return mLevels.TryGetValue(perkId, out var level) ? level : 0;
		}

		public void SetLevel(int perkId, int level)
		{
			if (perkId != 0) mLevels[perkId] = level;
		}

		public void Reset()
		{
			mLevels.Clear();
		}

		public IEnumerable<CharacterExclusivePerkSaveData> GetSaveData()
		{
			foreach (var entry in mLevels)
				yield return new CharacterExclusivePerkSaveData { PerkId = entry.Key, Level = entry.Value };
		}

		public void Restore(IEnumerable<CharacterExclusivePerkSaveData> entries)
		{
			mLevels.Clear();
			if (entries == null) return;
			foreach (var entry in entries)
				if (entry != null && entry.PerkId != 0 && entry.Level > 0) mLevels[entry.PerkId] = entry.Level;
		}

		protected override void OnInit()
		{
			Reset();
		}
	}
}
