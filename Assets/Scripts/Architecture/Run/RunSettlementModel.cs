namespace HaoFuSurvivor
{
	public readonly struct RunSettlementData
	{
		public readonly int CharacterId;
		public readonly RunPhase Result;
		public readonly int SurvivalSeconds;
		public readonly int Level;
		public readonly float RunExperience;
		public readonly BigCoin Coins;
		public readonly int MetaExperience;
		public readonly int NormalKillCount;
		public readonly int BossKillCount;

		public RunSettlementData(int characterId, RunPhase result, int survivalSeconds, int level, float runExperience, BigCoin coins, int metaExperience, int normalKillCount, int bossKillCount)
		{
			CharacterId = characterId;
			Result = result;
			SurvivalSeconds = survivalSeconds;
			Level = level;
			RunExperience = runExperience;
			Coins = coins;
			MetaExperience = metaExperience;
			NormalKillCount = normalKillCount;
			BossKillCount = bossKillCount;
		}
	}

	public class RunSettlementModel : QFramework.AbstractModel
	{
		public bool HasSettlement { get; internal set; }
		public RunSettlementData LastSettlement { get; internal set; }

		public void Reset()
		{
			HasSettlement = false;
			LastSettlement = default;
		}

		protected override void OnInit()
		{
			Reset();
		}
	}
}
