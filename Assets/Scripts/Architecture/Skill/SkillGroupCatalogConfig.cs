using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Skill/Skill Group Catalog")]
	public class SkillGroupCatalogConfig : ScriptableObject
	{
		public List<SkillGroupConfig> SkillGroups = new();
	}
}
