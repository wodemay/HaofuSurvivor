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
			runModel.RunId = System.Guid.NewGuid().ToString("N");
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
			this.GetSystem<RunSaveSystem>().SaveCurrentRun();
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
			this.GetSystem<RunSaveSystem>().SaveCurrentRun();
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
			if (!this.GetSystem<RunSettlementSystem>().Settle(RunPhase.Defeat)) return;

			ReleaseRunRuntime();
			if (!this.GetSystem<PlayerSpawnSystem>().SpawnSelectedCharacter()) return;
			StartRun();
		}

		public void ContinueSavedRun()
		{
			var save = this.GetSystem<RunSaveSystem>().Load();
			if (save == null)
			{
				ContinueFailed("存档不存在、已损坏或版本不兼容，无法继续游戏。请检查存档目录。");
				return;
			}
			if (this.GetModel<ProfileModel>().HasSettledRun(save.RunId))
			{
				this.GetSystem<RunSaveSystem>().Clear();
				ContinueFailed("这局游戏已经结算，不能重复领取奖励。");
				return;
			}
			this.GetModel<CharacterSelectionModel>().SelectedCharacterId = save.CharacterId;
			ReleaseRunRuntime();
			if (!this.GetSystem<PlayerSpawnSystem>().SpawnSelectedCharacter())
			{
				ContinueFailed("角色资源无法加载，已返回主菜单。");
				return;
			}
			StartRun();
			if (!this.GetSystem<RunSaveSystem>().Restore(save))
			{
				ContinueFailed("存档恢复失败，已清理本次恢复的对象并返回主菜单。");
				return;
			}
			if (save.SavedPhase == (int)RunPhase.Defeat) EndWithDefeat();
			else if (save.SavedPhase == (int)RunPhase.Victory) EndWithVictory();
		}

		private void ContinueFailed(string message)
		{
			this.GetModel<RunModel>().Phase = RunPhase.None;
			ReleaseRunRuntime();
			this.GetSystem<RunTimerSystem>().Stop();
			this.SendEvent(new RunContinueFailedEvent(message));
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
