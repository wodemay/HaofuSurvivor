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
			this.SendEvent(new RunEndedEvent(RunPhase.Victory));
		}

		public void EndWithDefeat()
		{
			var runModel = this.GetModel<RunModel>();
			if (runModel.Phase != RunPhase.Active) return;

			runModel.Phase = RunPhase.Defeat;
			this.SendEvent(new RunEndedEvent(RunPhase.Defeat));
		}

		protected override void OnInit()
		{
		}
	}
}
