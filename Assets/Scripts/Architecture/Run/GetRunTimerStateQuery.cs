using QFramework;

namespace HaoFuSurvivor
{
	public struct RunTimerState
	{
		public readonly int ElapsedSeconds;
		public readonly int CurrentStageIndex;
		public readonly float EnemyHealthMultiplier;
		public readonly float EnemyDamageMultiplier;
		public readonly float EnemyMoveSpeedMultiplier;
		public readonly float SpawnRateMultiplier;

		public RunTimerState(RunTimerModel timer)
		{
			ElapsedSeconds = (int)timer.ElapsedSeconds;
			CurrentStageIndex = timer.CurrentStageIndex;
			EnemyHealthMultiplier = timer.EnemyHealthMultiplier;
			EnemyDamageMultiplier = timer.EnemyDamageMultiplier;
			EnemyMoveSpeedMultiplier = timer.EnemyMoveSpeedMultiplier;
			SpawnRateMultiplier = timer.SpawnRateMultiplier;
		}
	}

	public class GetRunTimerStateQuery : AbstractQuery<RunTimerState>
	{
		protected override RunTimerState OnDo()
		{
			return new RunTimerState(this.GetModel<RunTimerModel>());
		}
	}
}
