using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Skill/Weapon Combination Config")]
	public class WeaponCombinationConfig : ScriptableObject
	{
		public int Id;
		public string DisplayName;
		[TextArea] public string Description;
		public Sprite Icon;
		[Range(0f, 1f)] public float CandidateChance = 0.7f;
		public List<WeaponCombinationWeaponRequirement> RequiredWeapons = new();
		public List<WeaponCombinationStatRequirement> RequiredStatUpgrades = new();
		public int TargetWeaponId;
	}

	[System.Serializable]
	public class WeaponCombinationWeaponRequirement
	{
		public int WeaponId;
		public int RequiredLevel = 1;
	}

	[System.Serializable]
	public class WeaponCombinationStatRequirement
	{
		public int StatUpgradeId;
		public int RequiredLevel = 1;
	}
}
