using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Skill/Skill Config")]
	public class SkillConfig : ScriptableObject
	{
		public int Id;
		public string DisplayName;
		[TextArea] public string Description;
		public Sprite Icon;
		public List<int> InitialAttackIds = new();
		public int MaxLevel = 1;
		public bool CanUpgrade;
	}
}
