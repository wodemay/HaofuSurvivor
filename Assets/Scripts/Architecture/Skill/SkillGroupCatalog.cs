using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class SkillGroupCatalog : IUtility
	{
		public SkillGroupCatalogConfig Config { get; }

		public SkillGroupCatalog()
		{
			Config = Resources.Load<SkillGroupCatalogConfig>("Configs/Combat/Skill/SkillGroupCatalog");
		}

		public SkillGroupConfig Get(int id)
		{
			return Config == null ? null : Config.SkillGroups.Find(skillGroup => skillGroup != null && skillGroup.Id == id);
		}
	}
}
