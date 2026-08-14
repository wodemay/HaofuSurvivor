using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Skill/Skill Group Config")]
	public class SkillGroupConfig : ScriptableObject
	{
		public int Id;
		public List<int> StartingWeaponIds = new();
		public List<int> StartingSkillIds = new();
		public int StartingDodgeId;
		public bool RequireStartingWeapons = true;
	}
}
