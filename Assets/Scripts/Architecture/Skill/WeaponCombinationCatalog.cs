using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class WeaponCombinationCatalog : IUtility
	{
		public WeaponCombinationCatalogConfig Config { get; }

		public WeaponCombinationCatalog()
		{
			Config = Resources.Load<WeaponCombinationCatalogConfig>("Configs/Combat/Weapon/WeaponCombinationCatalog");
		}

		public WeaponCombinationConfig Get(int id)
		{
			return Config == null ? null : Config.Combinations.Find(combination => combination != null && combination.Id == id);
		}
	}
}
