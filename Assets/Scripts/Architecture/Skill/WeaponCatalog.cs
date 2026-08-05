using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class WeaponCatalog : IUtility
	{
		public WeaponCatalogConfig Config { get; }

		public WeaponCatalog()
		{
			Config = Resources.Load<WeaponCatalogConfig>("Configs/Skills/WeaponCatalog");
		}

		public WeaponConfig Get(int id)
		{
			return Config == null ? null : Config.Weapons.Find(weapon => weapon != null && weapon.Id == id);
		}
	}
}
