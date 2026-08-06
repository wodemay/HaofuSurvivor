using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Skill/Weapon Config")]
	public class WeaponConfig : ScriptableObject
	{
		public int Id;
		public List<int> InitialAttackIds = new();
		public int MaxLevel = 5;
		public bool CanUpgrade = true;
		public List<WeaponLevelUpgrade> LevelUpgrades = new();
	}

	[System.Serializable]
	public class WeaponLevelUpgrade
	{
		public int Level;
		public List<int> AddAttackIds = new();
		public List<int> RemoveAttackIds = new();
	}
}
