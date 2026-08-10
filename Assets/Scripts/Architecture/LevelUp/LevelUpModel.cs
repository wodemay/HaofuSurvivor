using System.Collections.Generic;

namespace HaoFuSurvivor
{
	public class LevelUpModel : QFramework.AbstractModel
	{
		private readonly Queue<int> mPendingLevels = new();

		public int PendingSelectionCount => mPendingLevels.Count;
		public int CurrentLevel => mPendingLevels.Count > 0 ? mPendingLevels.Peek() : 0;

		public void Enqueue(int level)
		{
			mPendingLevels.Enqueue(level);
		}

		public void CompleteCurrent()
		{
			if (mPendingLevels.Count > 0) mPendingLevels.Dequeue();
		}

		public void Reset()
		{
			mPendingLevels.Clear();
		}

		protected override void OnInit()
		{
			Reset();
		}
	}
}
