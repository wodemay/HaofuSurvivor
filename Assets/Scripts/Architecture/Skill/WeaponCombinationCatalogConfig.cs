using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Skill/Weapon Combination Catalog")]
	public class WeaponCombinationCatalogConfig : ScriptableObject
	{
		public List<WeaponCombinationConfig> Combinations = new();
	}
}
