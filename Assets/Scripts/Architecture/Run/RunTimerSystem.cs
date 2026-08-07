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
			timer.DeltaTime = 0f;
			timer.FixedDeltaTime = 0f;
			timer.IsPaused = false;
			Time.timeScale = 1f;
			timer.CurrentStageIndex = -1;
			timer.EnemyHealthMultiplier = 1f;
			timer.EnemyDamageMultiplier = 1f;
			timer.EnemyMoveSpeedMultiplier = 1f;
			timer.SpawnRateMultiplier = 1f;
			this.SendEvent(new RunTimerUpdatedEvent(0));
		}

		public void Pause()
		{
			var timer = this.GetModel<RunTimerModel>();
			timer.IsPaused = true;
			timer.DeltaTime = 0f;
			timer.FixedDeltaTime = 0f;
			Time.timeScale = 0f;
		}

		public void Resume()
		{
			this.GetModel<RunTimerModel>().IsPaused = false;
			Time.timeScale = 1f;
		}

		public void Stop()
		{
			var timer = this.GetModel<RunTimerModel>();
			timer.DeltaTime = 0f;
			timer.FixedDeltaTime = 0f;
			Time.timeScale = 0f;
		}

		public void Advance(float unscaledDeltaTime)
		{
			var timer = this.GetModel<RunTimerModel>();
			timer.DeltaTime = GetGameDeltaTime(unscaledDeltaTime);
			if (timer.DeltaTime <= 0f) return;

			timer.ElapsedSeconds += timer.DeltaTime;
			ApplyReachedStages(timer);
			this.SendEvent(new RunTimerUpdatedEvent((int)timer.ElapsedSeconds));
		}

		public void AdvanceFixed(float unscaledFixedDeltaTime)
		{
			this.GetModel<RunTimerModel>().FixedDeltaTime = GetGameDeltaTime(unscaledFixedDeltaTime);
		}

		public bool IsRunning()
		{
			return this.GetModel<RunModel>().Phase == RunPhase.Active && !this.GetModel<RunTimerModel>().IsPaused;
		}

		private float GetGameDeltaTime(float unscaledDeltaTime)
		{
			return IsRunning() ? Mathf.Max(0f, unscaledDeltaTime) : 0f;
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
