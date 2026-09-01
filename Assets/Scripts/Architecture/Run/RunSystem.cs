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
			this.GetSystem<RunSaveSystem>().ResetAutoSaveTimer();
			this.GetSystem<RunSettlementSystem>().Reset();
			this.GetSystem<RunEconomySystem>().Reset();
			this.GetSystem<EnemySystem>().Reset();
			this.GetSystem<ExperienceSystem>().Reset();
			this.GetSystem<LevelUpSystem>().Reset();
			this.GetSystem<PlayerStatUpgradeSystem>().Reset();
			this.GetSystem<CharacterExclusivePerkSystem>().Reset();
			this.GetSystem<MapSystem>().Reset();
			this.GetSystem<MapEventSystem>().Reset();
			this.GetSystem<MapSystem>().PrepareForRun();
			this.GetSystem<MapNavMeshSystem>().Reset();
			this.GetSystem<BarrageProjectileSystem>().Reset();
			this.GetSystem<ExplosiveAreaSystem>().Reset();
			this.GetSystem<GameLoopSystem>().BeginRun();
			this.SendEvent(new RunStartedEvent());
		}

		public void EndWithVictory()
		{
			var runModel = this.GetModel<RunModel>();
			if (runModel.Phase != RunPhase.Active) return;

			runModel.Phase = RunPhase.Victory;
			this.GetSystem<RunTimerSystem>().Stop();
			this.GetSystem<GameLoopSystem>().EndRun();
			this.GetSystem<RunSaveSystem>().Clear();
			this.GetSystem<RunSettlementSystem>().Settle(RunPhase.Victory);
			this.SendEvent(new RunEndedEvent(RunPhase.Victory));
		}

		public void EndWithDefeat()
		{
			var runModel = this.GetModel<RunModel>();
			if (runModel.Phase != RunPhase.Active) return;

			runModel.Phase = RunPhase.Defeat;
			this.GetSystem<RunTimerSystem>().Stop();
			this.GetSystem<GameLoopSystem>().EndRun();
			this.GetSystem<RunSaveSystem>().Clear();
			this.GetSystem<RunSettlementSystem>().Settle(RunPhase.Defeat);
			this.SendEvent(new RunEndedEvent(RunPhase.Defeat));
		}

		public void Pause()
		{
			var runModel = this.GetModel<RunModel>();
			if (runModel.Phase != RunPhase.Active) return;

			runModel.Phase = RunPhase.Paused;
			this.GetSystem<RunTimerSystem>().Pause();
			this.GetSystem<InputSystem>().Clear();
			this.GetSystem<RunSaveSystem>().SaveCurrentRun();
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

		public void BeginLevelUpSelection()
		{
			var runModel = this.GetModel<RunModel>();
			if (runModel.Phase != RunPhase.Active) return;

			runModel.Phase = RunPhase.LevelUpSelection;
			this.GetSystem<RunTimerSystem>().Pause();
		}

		public void EndLevelUpSelection()
		{
			var runModel = this.GetModel<RunModel>();
			if (runModel.Phase != RunPhase.LevelUpSelection) return;

			runModel.Phase = RunPhase.Active;
			this.GetSystem<RunTimerSystem>().Resume();
		}

		public void ExitToCharacterSelection()
		{
			var runModel = this.GetModel<RunModel>();
			if (runModel.Phase == RunPhase.None) return;

			this.GetSystem<RunSaveSystem>().SaveCurrentRun();
			runModel.Phase = RunPhase.None;
			this.GetSystem<RunTimerSystem>().Stop();
			this.GetSystem<GameLoopSystem>().EndRun();
			ReleaseRunRuntime();
			this.SendEvent(new RunExitedEvent());
		}

		public void RestartSelectedCharacterRun()
		{
			if (this.GetModel<RunModel>().Phase != RunPhase.Defeat) return;

			ReleaseRunRuntime();
			if (!this.GetSystem<PlayerSpawnSystem>().SpawnSelectedCharacter()) return;
			StartRun();
		}

		public void ContinueSavedRun()
		{
			var save = this.GetSystem<RunSaveSystem>().Load();
			if (save == null) return;
			this.GetModel<CharacterSelectionModel>().SelectedCharacterId = save.CharacterId;
			ReleaseRunRuntime();
			if (!this.GetSystem<PlayerSpawnSystem>().SpawnSelectedCharacter()) return;
			StartRun();
			if (!this.GetSystem<RunSaveSystem>().Restore(save))
			{
				ReleaseRunRuntime();
				this.GetModel<RunModel>().Phase = RunPhase.None;
				this.GetSystem<RunTimerSystem>().Stop();
				return;
			}
		}

		private void ReleaseRunRuntime()
		{
			this.GetSystem<GameLoopSystem>().EndRun();
			this.GetSystem<EnemySystem>().Reset();
			this.GetSystem<ExperienceSystem>().Reset();
			this.GetSystem<LevelUpSystem>().Reset();
			this.GetSystem<DodgeSystem>().Reset();
			this.GetSystem<ProjectileSystem>().Reset();
			this.GetSystem<BarrageProjectileSystem>().Reset();
			this.GetSystem<ExplosiveAreaSystem>().Reset();
			this.GetSystem<CharacterExclusivePerkSystem>().Reset();
			this.GetSystem<MapSystem>().Reset();
			this.GetSystem<MapEventSystem>().Reset();
			this.GetSystem<MapNavMeshSystem>().Reset();
			this.GetSystem<PlayerSpawnSystem>().DespawnCurrentCharacter();
		}

		protected override void OnInit()
		{
			this.RegisterEvent<BossDefeatedEvent>(_ => EndWithVictory());
		}
	}
}
