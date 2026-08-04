using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class EnemyAttackCatalog : IUtility
	{
		private const string ConfigPath = "Configs/Enemies/EnemyAttackCatalog";

		public EnemyAttackCatalogConfig Config { get; }

		public EnemyAttackCatalog()
		{
			Config = Resources.Load<EnemyAttackCatalogConfig>(ConfigPath);
		}

		public EnemyAttackConfig Get(int id)
		{
			return Config == null ? null : Config.Attacks.Find(attack => attack != null && attack.Id == id);
		}
	}
}
