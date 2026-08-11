using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class WeaponEvolutionCatalog : IUtility
	{
		public WeaponEvolutionCatalogConfig Config { get; }

		public WeaponEvolutionCatalog()
		{
			Config = Resources.Load<WeaponEvolutionCatalogConfig>("Configs/Weapon/WeaponEvolutionCatalog");
		}

		public WeaponEvolutionConfig Get(int sourceWeaponId, int level)
		{
			return Config == null ? null : Config.Evolutions.Find(evolution =>
				evolution != null && evolution.SourceWeaponId == sourceWeaponId && level >= evolution.RequiredLevel);
		}
	}
}
