using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Skill/Dodge Config")]
	public class DodgeConfig : ScriptableObject
	{
		public int Id;
		public string DisplayName;
		[TextArea] public string Description;
		public Sprite Icon;
		public string ExecutorId = "dash";
		public float Cooldown = 1f;
		public float Duration = 0.2f;
		public float Distance = 2.5f;
		public float InvulnerabilityDuration = 0.15f;
		public int MaxLevel = 5;
		public bool CanUpgrade = true;
		public List<DodgeLevelUpgrade> LevelUpgrades = new();
	}

	[System.Serializable]
	public class DodgeLevelUpgrade
	{
		public int Level;
		[TextArea] public string Description;
		public float CooldownAdd;
		public float DurationAdd;
		public float DistanceAdd;
		public float InvulnerabilityDurationAdd;
	}
}
