using System;
using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[Serializable]
	public class CharacterExclusiveSkillUpgradeDefinition
	{
		public int CharacterId;
		public int SkillId;
		public int ExclusiveWeaponId;
		public List<int> CompletedWeaponIds = new();
		public int ExclusiveDodgeId;
		public List<int> CompletedDodgeIds = new();
		public string DisplayName;
		[TextArea] public string Description;
		public Sprite Icon;
	}

	[CreateAssetMenu(menuName = "ProjectSurvivor/Progression/Character Exclusive Skill Upgrade Catalog")]
	public class CharacterExclusiveSkillUpgradeConfig : ScriptableObject
	{
		public List<CharacterExclusiveSkillUpgradeDefinition> Upgrades = new();
	}
}
