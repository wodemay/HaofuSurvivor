using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class AttackCatalog : IUtility
	{
		public AttackCatalogConfig Config { get; }

		public AttackCatalog()
		{
			Config = Resources.Load<AttackCatalogConfig>("Configs/Combat/Attack/AttackCatalog");
		}

		public AttackConfig Get(int id)
		{
			return Config == null ? null : Config.Attacks.Find(attack => attack != null && attack.Id == id);
		}
	}
}
