using QFramework;

namespace HaoFuSurvivor
{
	public readonly struct RunTimeState
	{
		public readonly float DeltaTime;
		public readonly float FixedDeltaTime;
		public readonly bool IsRunning;

		public RunTimeState(RunTimerModel timer, RunPhase phase)
		{
			DeltaTime = timer.DeltaTime;
			FixedDeltaTime = timer.FixedDeltaTime;
			IsRunning = phase == RunPhase.Active && !timer.IsPaused;
		}
	}

	public class GetRunTimeStateQuery : AbstractQuery<RunTimeState>
	{
		protected override RunTimeState OnDo()
		{
			return new RunTimeState(this.GetModel<RunTimerModel>(), this.GetModel<RunModel>().Phase);
		}
	}
}
