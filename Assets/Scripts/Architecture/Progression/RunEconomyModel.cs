using QFramework;

namespace HaoFuSurvivor
{
	public class RunEconomyModel : AbstractModel
	{
		public BigCoin RunCoin { get; internal set; } = BigCoin.Zero;
		public int NormalKillCount { get; internal set; }
		public int BossKillCount { get; internal set; }
		public int EndlessRound { get; internal set; }

		public void Reset()
		{
			RunCoin = BigCoin.Zero;
			NormalKillCount = 0;
			BossKillCount = 0;
			EndlessRound = 0;
		}

		protected override void OnInit()
		{
			Reset();
		}
	}
}
