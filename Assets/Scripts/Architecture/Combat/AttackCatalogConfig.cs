using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Combat/Attack Catalog")]
	public class AttackCatalogConfig : ScriptableObject
	{
		public List<AttackConfig> Attacks = new();
	}
}
