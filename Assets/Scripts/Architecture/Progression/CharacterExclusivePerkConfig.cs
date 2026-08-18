using System;
using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	public enum CharacterExclusivePerkType
	{
		LowHealthDamage,
		DodgeWeaponProjectileCount,
		SkillWeaponCooldownReduction
	}

	[Serializable]
	public class CharacterExclusivePerkLevel
	{
		public int Level;
		public float Value;
		public float Duration;
	}

	[Serializable]
	public class CharacterExclusivePerkDefinition
	{
		public int Id;
		public int CharacterId;
		public CharacterExclusivePerkType Type;
		public string DisplayName;
		[TextArea] public string Description;
		public Sprite Icon;
		public float CandidateWeight = 2f;
		public float HealthThreshold = 0.5f;
		public int TriggerDodgeId;
		public int TriggerSkillId;
		public List<CharacterExclusivePerkLevel> LevelUpgrades = new();

		public int MaxLevel => LevelUpgrades?.Count ?? 0;
		public CharacterExclusivePerkLevel GetLevel(int level)
		{
			return LevelUpgrades?.Find(item => item != null && item.Level == level);
		}
	}

	[CreateAssetMenu(menuName = "ProjectSurvivor/Progression/Character Exclusive Perk Catalog")]
	public class CharacterExclusivePerkConfig : ScriptableObject
	{
		public List<CharacterExclusivePerkDefinition> Perks = new();
	}
}
