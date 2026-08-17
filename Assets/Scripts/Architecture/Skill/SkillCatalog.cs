using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class SkillCatalog : IUtility
	{
		public SkillCatalogConfig Config { get; }

		public SkillCatalog()
		{
			Config = Resources.Load<SkillCatalogConfig>("Configs/Combat/Skill/SkillCatalog");
		}

		public SkillConfig Get(int id)
		{
			return Config == null ? null : Config.Skills.Find(skill => skill != null && skill.Id == id);
		}
	}
}
