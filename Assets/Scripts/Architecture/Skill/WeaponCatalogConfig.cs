using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Skill/Weapon Catalog")]
	public class WeaponCatalogConfig : ScriptableObject
	{
		public List<WeaponConfig> Weapons = new();
	}
}
