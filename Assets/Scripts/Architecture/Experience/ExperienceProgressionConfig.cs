using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Experience Progression Config")]
	public class ExperienceProgressionConfig : ScriptableObject
	{
		public List<int> RequiredExperienceByLevel = new();

		public int GetRequiredExperience(int level)
		{
			if (RequiredExperienceByLevel.Count == 0) return 1;
			var index = Mathf.Clamp(level - 1, 0, RequiredExperienceByLevel.Count - 1);
			return Mathf.Max(1, RequiredExperienceByLevel[index]);
		}
	}

	public class ExperienceProgressionCatalog : QFramework.IUtility
	{
		public ExperienceProgressionConfig Config { get; }

		public ExperienceProgressionCatalog()
		{
			Config = Resources.Load<ExperienceProgressionConfig>("Configs/Progression/Experience/ExperienceProgression");
		}
	}
}
