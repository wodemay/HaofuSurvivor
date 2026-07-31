using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class RunTimerSystem : AbstractSystem
	{
		public void StartTimer()
		{
			var timer = this.GetModel<RunTimerModel>();
			timer.ElapsedSeconds = 0f;
			timer.CurrentStageIndex = -1;
			timer.EnemyHealthMultiplier = 1f;
			timer.EnemyDamageMultiplier = 1f;
			timer.EnemyMoveSpeedMultiplier = 1f;
			timer.SpawnRateMultiplier = 1f;
			this.SendEvent(new RunTimerUpdatedEvent(0));
		}

		public void Advance(float deltaTime)
		{
			if (deltaTime <= 0f || this.GetModel<RunModel>().Phase != RunPhase.Active) return;

			var timer = this.GetModel<RunTimerModel>();
			timer.ElapsedSeconds += deltaTime;
			ApplyReachedStages(timer);
			this.SendEvent(new RunTimerUpdatedEvent((int)timer.ElapsedSeconds));
		}

		private void ApplyReachedStages(RunTimerModel timer)
		{
			var config = this.GetUtility<RunTimelineCatalog>().Config;
			if (config == null) return;

			while (timer.CurrentStageIndex + 1 < config.Stages.Count &&
				config.Stages[timer.CurrentStageIndex + 1].TimeSeconds <= timer.ElapsedSeconds)
			{
				timer.CurrentStageIndex++;
				var stage = config.Stages[timer.CurrentStageIndex];
				timer.EnemyHealthMultiplier = stage.EnemyHealthMultiplier;
				timer.EnemyDamageMultiplier = stage.EnemyDamageMultiplier;
				timer.EnemyMoveSpeedMultiplier = stage.EnemyMoveSpeedMultiplier;
				timer.SpawnRateMultiplier = stage.SpawnRateMultiplier;
				this.SendEvent(new RunTimelineStageReachedEvent(timer.CurrentStageIndex, stage));
			}
		}

		protected override void OnInit()
		{
		}
	}
}
