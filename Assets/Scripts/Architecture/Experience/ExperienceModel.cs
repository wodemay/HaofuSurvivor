namespace HaoFuSurvivor
{
	public class ExperienceModel : QFramework.AbstractModel
	{
		public int Level { get; internal set; }
		public float CurrentExperience { get; internal set; }
		public float RequiredExperience { get; internal set; }

		public void Reset(float requiredExperience)
		{
			Level = 1;
			CurrentExperience = 0;
			RequiredExperience = requiredExperience;
		}

		public void RestoreProgress(int level, float currentExperience, float savedRequiredExperience, float requiredExperience)
		{
			Level = UnityEngine.Mathf.Max(1, level);
			RequiredExperience = UnityEngine.Mathf.Max(1f, requiredExperience);
			CurrentExperience = UnityEngine.Mathf.Clamp01(currentExperience / UnityEngine.Mathf.Max(1f, savedRequiredExperience))
				* RequiredExperience;
		}

		protected override void OnInit()
		{
			Reset(1);
		}
	}

}
