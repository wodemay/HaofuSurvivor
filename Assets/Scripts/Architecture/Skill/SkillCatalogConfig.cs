using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Skill/Skill Catalog")]
	public class SkillCatalogConfig : ScriptableObject
	{
		public List<SkillConfig> Skills = new();
	}
}
