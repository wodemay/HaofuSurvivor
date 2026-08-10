using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public readonly struct ExperienceState
	{
		public readonly int Level;
		public readonly int CurrentExperience;
		public readonly int RequiredExperience;

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

	public class ExperienceSystem : AbstractSystem
	{
		private readonly List<ActiveExperienceDrop> mDrops = new();

		public void Reset()
		{
			foreach (var drop in mDrops) ExperienceFactory.Instance.Release(drop.Config, drop.Controller);
			mDrops.Clear();
			var config = this.GetUtility<ExperienceProgressionCatalog>().Config;
			this.GetModel<ExperienceModel>().Reset(config == null ? 1 : config.GetRequiredExperience(1));
		}

		public void Tick()
		{
			if (!this.GetSystem<RunTimerSystem>().IsRunning()) return;
			var player = this.GetModel<PlayerModel>();
			if (!player.IsRegistered || player.IsDead) return;
			var deltaTime = this.GetModel<RunTimerModel>().DeltaTime;
			if (deltaTime <= 0f) return;
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
		}

		private void OnEnemyDied(EnemyDiedEvent enemyDiedEvent)
		{
			var config = enemyDiedEvent.ExperienceDrop;
			if (config == null || config.Prefab == null) return;
			var controller = ExperienceFactory.Instance.Create(config, enemyDiedEvent.DeathPosition);
			if (controller == null) return;
			controller.Configure(config.BaseExperience);
			mDrops.Add(new ActiveExperienceDrop(config, controller));
		}

		private void Collect(ActiveExperienceDrop drop)
		{
			var amount = drop.Controller.Experience;
			ExperienceFactory.Instance.Release(drop.Config, drop.Controller);
			AddExperience(amount);
		}

		private void AddExperience(int amount)
		{
			var model = this.GetModel<ExperienceModel>();
			model.CurrentExperience += Mathf.Max(0, amount);
			var progression = this.GetUtility<ExperienceProgressionCatalog>().Config;
			while (model.CurrentExperience >= model.RequiredExperience)
			{
				model.CurrentExperience -= model.RequiredExperience;
				model.Level++;
				model.RequiredExperience = progression == null ? 1 : progression.GetRequiredExperience(model.Level);
				this.SendEvent(new PlayerLevelUpEvent(model.Level));
			}
			this.SendEvent(new ExperienceCollectedEvent(amount, model.CurrentExperience, model.RequiredExperience));
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
