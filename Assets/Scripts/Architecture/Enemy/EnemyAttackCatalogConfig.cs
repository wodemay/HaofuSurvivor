using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Enemy Attack Catalog")]
	public class EnemyAttackCatalogConfig : ScriptableObject
	{
		public List<EnemyAttackConfig> Attacks = new();
	}
}
