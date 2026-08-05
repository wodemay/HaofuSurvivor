using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Skill/Weapon Config")]
	public class WeaponConfig : ScriptableObject
	{
		public int Id;
		public List<int> AttackIds = new();
	}
}
