using System.Collections.Generic;
using System.Globalization;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public readonly struct ExperienceState
	{
		public readonly int Level;
		public readonly float CurrentExperience;
		public readonly float RequiredExperience;
		public string CurrentExperienceText => CurrentExperience.ToString("F1", CultureInfo.InvariantCulture);
		public string RequiredExperienceText => RequiredExperience.ToString("F1", CultureInfo.InvariantCulture);

		public ExperienceState(ExperienceModel model)
		{
			Level = model.Level;
			CurrentExperience = model.CurrentExperience;
			RequiredExperience = model.RequiredExperience;
		}
	}

	public class GetExperienceStateQuery : AbstractQuery<ExperienceState>
	{
		protected override ExperienceState OnDo()
		{
			return new ExperienceState(this.GetModel<ExperienceModel>());
		}
	}

	public class ExperienceSystem : AbstractSystem, IRunUpdateable
	{
		private readonly List<ActiveExperienceDrop> mDrops = new();

		public void Reset()
		{
			ClearDrops();
			var config = this.GetUtility<ExperienceProgressionCatalog>().Config;
			this.GetModel<ExperienceModel>().Reset(config == null ? 1 : config.GetRequiredExperience(1));
		}

		public IEnumerable<ExperienceDropSaveData> GetSaveData()
		{
			foreach (var drop in mDrops)
			{
				if (drop.Config == null || drop.Controller == null) continue;
				yield return new ExperienceDropSaveData
				{
					ConfigId = drop.Config.Id,
					Experience = drop.Controller.Experience,
					PositionX = drop.Controller.transform.position.x,
					PositionY = drop.Controller.transform.position.y,
					IsCaptured = drop.IsCaptured,
					AbsorbSpeed = drop.AbsorbSpeed
				};
			}
		}

		public void RestoreDrops(IEnumerable<ExperienceDropSaveData> entries)
		{
			ClearDrops();
			if (entries == null) return;
			foreach (var entry in entries)
			{
				var config = entry == null ? null : FindConfig(entry.ConfigId);
				var controller = config == null ? null : ExperienceFactory.Instance.Create(config, new Vector2(entry.PositionX, entry.PositionY));
				if (controller == null) continue;
				controller.Configure(entry.Experience);
				mDrops.Add(new ActiveExperienceDrop(config, controller) { IsCaptured = entry.IsCaptured, AbsorbSpeed = Mathf.Max(0f, entry.AbsorbSpeed) });
			}
			if (mDrops.Count > 0 && this.GetSystem<RunTimerSystem>().IsRunning()) this.GetSystem<GameLoopSystem>().RegisterUpdateable(this);
		}

		public void OnRunUpdate(float deltaTime)
		{
			var player = this.GetModel<PlayerModel>();
			if (!player.IsRegistered || player.IsDead) return;
			var playerStats = this.GetModel<PlayerStatModel>();
			var radius = playerStats.ExperienceAbsorbRadius;
			for (var index = mDrops.Count - 1; index >= 0; index--)
			{
				var drop = mDrops[index];
				if (drop.Controller == null)
				{
					mDrops.RemoveAt(index);
					continue;
				}
				var distance = Vector2.Distance(drop.Controller.transform.position, player.Position);
				if (!drop.IsCaptured)
				{
					if (distance > radius) continue;
					drop.IsCaptured = true;
					drop.AbsorbSpeed = 0f;
				}
				if (distance <= 0.1f)
				{
					Collect(drop);
					mDrops.RemoveAt(index);
					continue;
				}
				drop.AbsorbSpeed = Mathf.Min(
					playerStats.ExperienceAbsorbMaxSpeed,
					drop.AbsorbSpeed + playerStats.ExperienceAbsorbAcceleration * deltaTime);
				drop.Controller.MoveTowards(player.Position, drop.AbsorbSpeed * deltaTime);
			}
			if (mDrops.Count == 0) this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		private void OnEnemyDied(EnemyDiedEvent enemyDiedEvent)
		{
			var config = enemyDiedEvent.ExperienceDrop;
			if (config == null || config.Prefab == null) return;
			var controller = ExperienceFactory.Instance.Create(config, enemyDiedEvent.DeathPosition);
			if (controller == null) return;
			controller.Configure(config.BaseExperience);
			mDrops.Add(new ActiveExperienceDrop(config, controller));
			if (this.GetSystem<RunTimerSystem>().IsRunning()) this.GetSystem<GameLoopSystem>().RegisterUpdateable(this);
		}

		private void Collect(ActiveExperienceDrop drop)
		{
			var amount = drop.Controller.Experience;
			ExperienceFactory.Instance.Release(drop.Config, drop.Controller);
			AddExperience(amount);
		}

		private void ClearDrops()
		{
			foreach (var drop in mDrops) ExperienceFactory.Instance.Release(drop.Config, drop.Controller);
			mDrops.Clear();
			this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		private static ExperienceDropConfig FindConfig(int configId)
		{
			foreach (var config in Resources.LoadAll<ExperienceDropConfig>("Configs/Progression/Experience"))
				if (config != null && config.Id == configId) return config;
			return null;
		}

		private void AddExperience(float amount)
		{
			var model = this.GetModel<ExperienceModel>();
			var finalAmount = Mathf.Max(0f, amount) * this.GetSystem<StatSystem>().GetExperienceMultiplier();
			model.CurrentExperience += finalAmount;
			var progression = this.GetUtility<ExperienceProgressionCatalog>().Config;
			while (model.CurrentExperience >= model.RequiredExperience)
			{
				model.CurrentExperience -= model.RequiredExperience;
				model.Level++;
				model.RequiredExperience = progression == null ? 1 : progression.GetRequiredExperience(model.Level);
				this.SendEvent(new PlayerLevelUpEvent(model.Level));
			}
			this.SendEvent(new ExperienceCollectedEvent(finalAmount, model.CurrentExperience, model.RequiredExperience));
		}

		protected override void OnInit()
		{
			this.RegisterEvent<EnemyDiedEvent>(OnEnemyDied);
			Reset();
		}

		private sealed class ActiveExperienceDrop
		{
			public readonly ExperienceDropConfig Config;
			public readonly ExperienceDropController Controller;
			public bool IsCaptured;
			public float AbsorbSpeed;

			public ActiveExperienceDrop(ExperienceDropConfig config, ExperienceDropController controller)
			{
				Config = config;
				Controller = controller;
			}
		}
	}
}
