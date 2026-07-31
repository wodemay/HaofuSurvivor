using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Character Config")]
	public class CharacterConfig : ScriptableObject
	{
		public int Id;
		public string DisplayName;
		[TextArea] public string SkillDescription;
		public int SkillId;
		public Sprite Icon;
		public GameObject PlayerPrefab;
		public int SortOrder;
		public float MaxHealth;
		public float MoveSpeed;
		public float AttackPower;
	}
}
