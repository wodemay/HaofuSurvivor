using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Character Config")]
	public class CharacterConfig : ScriptableObject
	{
		public int Id;
		public string DisplayName;
		[TextArea] public string SkillDescription;
		public int SkillGroupId;
		public Sprite Icon;
		public GameObject PlayerPrefab;
		public int SortOrder;
		public float MaxHealth;
		public float MoveSpeed;
		public float AttackPower;
		public float BaseExperienceAbsorbRadius = 2.5f;
		public float BaseExperienceAbsorbAcceleration = 50f;
		public float BaseExperienceAbsorbMaxSpeed = 30f;
	}
}
