using System.Globalization;
using QFramework;

namespace HaoFuSurvivor
{
	public readonly struct RunEconomyState
	{
		public readonly BigCoin RunCoin;
		public readonly int NormalKillCount;
		public readonly int BossKillCount;
		public readonly int EndlessRound;

		public RunEconomyState(RunEconomyModel model)
		{
			RunCoin = model.RunCoin;
			NormalKillCount = model.NormalKillCount;
			BossKillCount = model.BossKillCount;
			EndlessRound = model.EndlessRound;
		}
	}

	public class GetRunEconomyStateQuery : AbstractQuery<RunEconomyState>
	{
		protected override RunEconomyState OnDo() => new RunEconomyState(this.GetModel<RunEconomyModel>());
	}

	public class RunEconomySystem : AbstractSystem
	{
		public void Reset() => this.GetModel<RunEconomyModel>().Reset();

		public void AddCoins(BigCoin amount)
		{
			if (amount == null) return;
			var model = this.GetModel<RunEconomyModel>();
			model.RunCoin = model.RunCoin.AddCoins(amount);
			PublishChanged();
		}

		public void Restore(BigCoin runCoin, int normalKillCount, int bossKillCount, int endlessRound)
		{
			var model = this.GetModel<RunEconomyModel>();
			model.RunCoin = runCoin ?? BigCoin.Zero;
			model.NormalKillCount = System.Math.Max(0, normalKillCount);
			model.BossKillCount = System.Math.Max(0, bossKillCount);
			model.EndlessRound = System.Math.Max(0, endlessRound);
			PublishChanged();
		}

		public BigCoin CalculateSettlementCoins(RunPhase result, int survivalSeconds)
		{
			var model = this.GetModel<RunEconomyModel>();
			var config = this.GetUtility<CoinEconomyCatalog>().Config;
			var normalKillCoins = config == null ? 1 : config.NormalKillCoins;
			var bossKillCoins = config == null ? 50 : config.BossKillCoins;
			var coinsPerMinute = config == null ? 5 : config.CoinsPerSurvivalMinute;
			var victoryCoins = config == null ? 100 : config.VictoryCoins;
			var roundCoins = config == null ? 20 : config.EndlessRoundCoins;
			var total = model.RunCoin
				.AddCoins(ToCoins((long)model.NormalKillCount * normalKillCoins))
				.AddCoins(ToCoins((long)model.BossKillCount * bossKillCoins))
				.AddCoins(ToCoins((long)(survivalSeconds / 60) * coinsPerMinute))
				.AddCoins(ToCoins((long)model.EndlessRound * roundCoins));
			if (result == RunPhase.Victory) total = total.AddCoins(ToCoins(victoryCoins));
			return total;
		}

		private void OnEnemyDied(EnemyDiedEvent enemyDiedEvent)
		{
			var model = this.GetModel<RunEconomyModel>();
			if (enemyDiedEvent.IsBoss)
			{
				model.BossKillCount++;
				this.SendEvent(new BossDefeatedEvent());
			}
			else model.NormalKillCount++;
			PublishChanged();
		}

		private void PublishChanged()
		{
			this.SendEvent(new RunEconomyChangedEvent(new RunEconomyState(this.GetModel<RunEconomyModel>())));
		}

		private static BigCoin ToCoins(long value)
		{
			return new BigCoin(System.Math.Max(0L, value).ToString(CultureInfo.InvariantCulture));
		}

		protected override void OnInit()
		{
			this.RegisterEvent<EnemyDiedEvent>(OnEnemyDied);
			Reset();
		}
	}
}
