using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Experience Progression Config")]
	public class ExperienceProgressionConfig : ScriptableObject
	{
		public List<float> RequiredExperienceByLevel = new();
		[Min(1f)] public float ExperienceGrowthPerLevel = 8f;
		[Min(0f)] public float ExperienceGrowthAcceleration = 0.25f;

		public float GetRequiredExperience(int level)
		{
			if (RequiredExperienceByLevel.Count == 0) return 1f;
			var index = Mathf.Clamp(level - 1, 0, RequiredExperienceByLevel.Count - 1);
			var baseExperience = Mathf.Max(1f, RequiredExperienceByLevel[index]);
			var extraLevels = Mathf.Max(0f, (float)level - RequiredExperienceByLevel.Count);
			return Mathf.Ceil(baseExperience + Mathf.Max(1f, ExperienceGrowthPerLevel) * extraLevels
				+ Mathf.Max(0f, ExperienceGrowthAcceleration) * extraLevels * extraLevels);
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
