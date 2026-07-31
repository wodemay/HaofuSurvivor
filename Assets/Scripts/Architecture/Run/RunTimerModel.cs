using QFramework;

namespace HaoFuSurvivor
{
	public class RunTimerModel : AbstractModel
	{
		public float ElapsedSeconds { get; internal set; }
		public int CurrentStageIndex { get; internal set; }
		public float EnemyHealthMultiplier { get; internal set; }
		public float EnemyDamageMultiplier { get; internal set; }
		public float EnemyMoveSpeedMultiplier { get; internal set; }
		public float SpawnRateMultiplier { get; internal set; }

		protected override void OnInit()
		{
			ElapsedSeconds = 0f;
			CurrentStageIndex = -1;
			EnemyHealthMultiplier = 1f;
			EnemyDamageMultiplier = 1f;
			EnemyMoveSpeedMultiplier = 1f;
			SpawnRateMultiplier = 1f;
		}
	}
}
