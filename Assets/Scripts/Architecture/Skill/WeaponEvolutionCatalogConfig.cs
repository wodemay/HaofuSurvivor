using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Skill/Weapon Evolution Catalog")]
	public class WeaponEvolutionCatalogConfig : ScriptableObject
	{
		public List<WeaponEvolutionConfig> Evolutions = new();
	}
}
