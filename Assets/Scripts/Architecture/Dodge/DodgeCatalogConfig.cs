using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Skill/Dodge Catalog")]
	public class DodgeCatalogConfig : ScriptableObject
	{
		public List<DodgeConfig> Dodges = new();
	}
}
