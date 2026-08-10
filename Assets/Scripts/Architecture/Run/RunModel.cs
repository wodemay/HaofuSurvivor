namespace HaoFuSurvivor
{
	public enum RunPhase
	{
		None,
		Active,
		Paused,
		LevelUpSelection,
		Defeat,
		Victory
	}

	public class RunModel : QFramework.AbstractModel
	{
		public RunPhase Phase { get; internal set; }

		protected override void OnInit()
		{
			Phase = RunPhase.None;
		}
	}
}
