namespace HaoFuSurvivor
{
	public class ExperienceModel : QFramework.AbstractModel
	{
		public int Level { get; internal set; }
		public int CurrentExperience { get; internal set; }
		public int RequiredExperience { get; internal set; }

		public void Reset(int requiredExperience)
		{
			Level = 1;
			CurrentExperience = 0;
			RequiredExperience = requiredExperience;
		}

		protected override void OnInit()
		{
			Reset(1);
		}
	}

}
