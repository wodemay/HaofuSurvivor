using System.Collections.Generic;
using QFramework;

namespace HaoFuSurvivor
{
	public class ProfileModel : AbstractModel
	{
		public BigCoin ProfileCoin { get; internal set; } = BigCoin.Zero;
		public bool IsLoaded { get; internal set; }
		private readonly HashSet<string> mSettledRunIds = new();

		public bool HasSettledRun(string runId) => !string.IsNullOrEmpty(runId) && mSettledRunIds.Contains(runId);
		public IReadOnlyCollection<string> SettledRunIds => mSettledRunIds;

		internal void RestoreSettledRunIds(IEnumerable<string> runIds)
		{
			mSettledRunIds.Clear();
			if (runIds == null) return;
			foreach (var runId in runIds)
				if (!string.IsNullOrEmpty(runId)) mSettledRunIds.Add(runId);
		}

		internal bool AddSettledRun(string runId) => !string.IsNullOrEmpty(runId) && mSettledRunIds.Add(runId);
		internal void RemoveSettledRun(string runId)
		{
			if (!string.IsNullOrEmpty(runId)) mSettledRunIds.Remove(runId);
		}

		protected override void OnInit()
		{
			ProfileCoin = BigCoin.Zero;
			IsLoaded = false;
			mSettledRunIds.Clear();
		}
	}
}
