using QFramework;

namespace HaoFuSurvivor
{
	public class RunSystem : AbstractSystem
	{
		public void StartRun()
		{
			var runModel = this.GetModel<RunModel>();
			if (runModel.Phase == RunPhase.Active) return;

			runModel.Phase = RunPhase.Active;
			this.GetSystem<RunTimerSystem>().StartTimer();
			this.GetSystem<EnemySystem>().Reset();
			this.SendEvent(new RunStartedEvent());
		}

		public void EndWithVictory()
		{
			var runModel = this.GetModel<RunModel>();
			if (runModel.Phase != RunPhase.Active) return;

			runModel.Phase = RunPhase.Victory;
			this.GetSystem<RunTimerSystem>().Stop();
			this.SendEvent(new RunEndedEvent(RunPhase.Victory));
		}

		public void EndWithDefeat()
		{
			var runModel = this.GetModel<RunModel>();
			if (runModel.Phase != RunPhase.Active) return;

			runModel.Phase = RunPhase.Defeat;
			this.GetSystem<RunTimerSystem>().Stop();
			this.SendEvent(new RunEndedEvent(RunPhase.Defeat));
		}

		public void Pause()
		{
			var runModel = this.GetModel<RunModel>();
			if (runModel.Phase != RunPhase.Active) return;

			runModel.Phase = RunPhase.Paused;
			this.GetSystem<RunTimerSystem>().Pause();
			this.SendEvent(new RunPausedEvent());
		}

		public void Resume()
		{
			var runModel = this.GetModel<RunModel>();
			if (runModel.Phase != RunPhase.Paused) return;

			runModel.Phase = RunPhase.Active;
			this.GetSystem<RunTimerSystem>().Resume();
			this.SendEvent(new RunResumedEvent());
		}

		public void ExitToCharacterSelection()
		{
			var runModel = this.GetModel<RunModel>();
			if (runModel.Phase == RunPhase.None) return;

			runModel.Phase = RunPhase.None;
			this.GetSystem<RunTimerSystem>().Stop();
			this.GetSystem<EnemySystem>().Reset();
			this.GetSystem<PlayerSpawnSystem>().DespawnCurrentCharacter();
		}

		protected override void OnInit()
		{
		}
	}
}
