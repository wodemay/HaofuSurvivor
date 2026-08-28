using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Coin Economy Config")]
	public class CoinEconomyConfig : ScriptableObject
	{
		public int NormalKillCoins = 1;
		public int BossKillCoins = 50;
		public int CoinsPerSurvivalMinute = 5;
		public int VictoryCoins = 100;
		public int EndlessRoundCoins = 20;
	}

	public class CoinEconomyCatalog : QFramework.IUtility
	{
		public CoinEconomyConfig Config { get; }

		public CoinEconomyCatalog()
		{
			Config = Resources.Load<CoinEconomyConfig>("Configs/Progression/CoinEconomy");
		}
	}
}
