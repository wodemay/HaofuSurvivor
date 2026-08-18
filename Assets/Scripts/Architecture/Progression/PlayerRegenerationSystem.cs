using QFramework;

namespace HaoFuSurvivor
{
	public class PlayerRegenerationSystem : AbstractSystem, IRunUpdateable
	{
		private const float TickInterval = 1f;
		private float mElapsedSeconds;

		public void RefreshRegistration()
		{
			var player = this.GetModel<PlayerModel>();
			var shouldTick = player.IsRegistered && !player.IsDead &&
				this.GetSystem<StatSystem>().GetNaturalHealthRegenerationRatio() > 0f;
			if (shouldTick) this.GetSystem<GameLoopSystem>().RegisterUpdateable(this);
			else this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		public void OnRunUpdate(float deltaTime)
		{
			var player = this.GetModel<PlayerModel>();
			if (!player.IsRegistered || player.IsDead) return;
			mElapsedSeconds += deltaTime;
			while (mElapsedSeconds >= TickInterval)
			{
				mElapsedSeconds -= TickInterval;
				var amount = this.GetSystem<StatSystem>().GetMaxHealth() *
					this.GetSystem<StatSystem>().GetNaturalHealthRegenerationRatio();
				this.GetSystem<PlayerSystem>().RestoreHealth(amount, false);
			}
		}

		private void OnRunStarted(RunStartedEvent runStartedEvent)
		{
			mElapsedSeconds = 0f;
			RefreshRegistration();
		}

		private void OnRunEnded(RunEndedEvent runEndedEvent)
		{
			mElapsedSeconds = 0f;
			this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		private void OnPlayerStatUpgraded(PlayerStatUpgradedEvent upgradeEvent)
		{
			RefreshRegistration();
		}

		protected override void OnInit()
		{
			this.RegisterEvent<RunStartedEvent>(OnRunStarted);
			this.RegisterEvent<RunEndedEvent>(OnRunEnded);
			this.RegisterEvent<PlayerStatUpgradedEvent>(OnPlayerStatUpgraded);
		}
	}
}
