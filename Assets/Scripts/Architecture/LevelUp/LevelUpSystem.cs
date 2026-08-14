using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public readonly struct LevelUpWeaponOption
	{
		public readonly int RuntimeId;
		public readonly int WeaponId;
		public readonly int CurrentLevel;
		public readonly string DisplayName;
		public readonly string Description;
		public readonly Sprite Icon;
		public readonly bool IsEvolution;
		public readonly bool IsDodge;
		public readonly string LevelText;

		public LevelUpWeaponOption(WeaponRuntimeData runtime, WeaponConfig config, string description, bool isEvolution = false)
		{
			RuntimeId = runtime.RuntimeId;
			WeaponId = runtime.WeaponId;
			CurrentLevel = runtime.Level;
			DisplayName = string.IsNullOrWhiteSpace(config.DisplayName) ? $"Weapon {runtime.WeaponId}" : config.DisplayName;
			Description = string.IsNullOrWhiteSpace(description) ? config.Description : description;
			Icon = config.Icon;
			IsEvolution = isEvolution;
			IsDodge = false;
			LevelText = isEvolution ? $"Level{runtime.Level}->Evolve" : $"Level{runtime.Level}->Level{runtime.Level + 1}";
		}

		public LevelUpWeaponOption(DodgeRuntimeData runtime, DodgeConfig config, string description)
		{
			RuntimeId = 0;
			WeaponId = config.Id;
			CurrentLevel = runtime.Level;
			DisplayName = string.IsNullOrWhiteSpace(config.DisplayName) ? $"Dodge {runtime.DodgeId}" : config.DisplayName;
			Description = string.IsNullOrWhiteSpace(description) ? config.Description : description;
			Icon = config.Icon;
			IsEvolution = false;
			IsDodge = true;
			LevelText = $"Level{runtime.Level}->Level{runtime.Level + 1}";
		}
	}

	public readonly struct LevelUpState
	{
		public readonly int CurrentLevel;
		public readonly int PendingSelectionCount;

		public LevelUpState(LevelUpModel model)
		{
			CurrentLevel = model.CurrentLevel;
			PendingSelectionCount = model.PendingSelectionCount;
		}
	}

	public class GetLevelUpStateQuery : AbstractQuery<LevelUpState>
	{
		protected override LevelUpState OnDo()
		{
			return new LevelUpState(this.GetModel<LevelUpModel>());
		}
	}

	public class GetLevelUpWeaponOptionsQuery : AbstractQuery<IReadOnlyList<LevelUpWeaponOption>>
	{
		protected override IReadOnlyList<LevelUpWeaponOption> OnDo()
		{
			var options = new List<LevelUpWeaponOption>();
			var catalog = GameArchitecture.Interface.GetUtility<WeaponCatalog>();
			foreach (var runtime in this.GetModel<PlayerLoadoutModel>().Weapons)
			{
				var config = catalog.Get(runtime.WeaponId);
				if (config == null) continue;
				if (runtime.CanUpgrade && runtime.Level < config.MaxLevel)
				{
					var upgrade = config.LevelUpgrades.Find(item => item != null && item.Level == runtime.Level + 1);
					options.Add(new LevelUpWeaponOption(runtime, config, upgrade?.Description));
					continue;
				}
				var evolution = GameArchitecture.Interface.GetUtility<WeaponEvolutionCatalog>().Get(runtime.WeaponId, runtime.Level);
				var target = evolution == null ? null : catalog.Get(evolution.TargetWeaponId);
				if (target != null && !target.CanUpgrade && target.MaxLevel == 1)
				{
					options.Add(new LevelUpWeaponOption(runtime, target, target.Description, true));
				}
			}
			var dodge = this.GetModel<DodgeModel>().Runtime;
			var dodgeConfig = dodge == null ? null : GameArchitecture.Interface.GetUtility<DodgeCatalog>().Get(dodge.DodgeId);
			if (dodge != null && dodgeConfig != null && this.GetSystem<DodgeSystem>().HasUpgrade())
			{
				var upgrade = dodgeConfig.LevelUpgrades.Find(item => item != null && item.Level == dodge.Level + 1);
				options.Add(new LevelUpWeaponOption(dodge, dodgeConfig, upgrade?.Description));
			}
			return options;
		}
	}

	public class LevelUpSystem : AbstractSystem
	{
		public void Reset()
		{
			this.GetModel<LevelUpModel>().Reset();
		}

		public bool CompleteWeaponUpgrade(int weaponRuntimeId, bool isEvolution, bool isDodge = false)
		{
			var model = this.GetModel<LevelUpModel>();
			if (model.PendingSelectionCount <= 0) return false;
			var loadout = this.GetSystem<PlayerLoadoutSystem>();
			var completed = isDodge ? this.GetSystem<DodgeSystem>().Upgrade() : isEvolution ? loadout.TryEvolveWeapon(weaponRuntimeId) : loadout.UpgradeWeapon(weaponRuntimeId);
			if (!completed) return false;

			model.CompleteCurrent();
			PresentNextSelection();
			return true;
		}

		private void OnPlayerLevelUp(PlayerLevelUpEvent levelUpEvent)
		{
			if (!HasSelectionOptions()) return;
			this.GetModel<LevelUpModel>().Enqueue(levelUpEvent.Level);
			PresentNextSelection();
		}

		private void PresentNextSelection()
		{
			var model = this.GetModel<LevelUpModel>();
			if (model.PendingSelectionCount <= 0)
			{
				this.GetSystem<RunSystem>().EndLevelUpSelection();
				return;
			}
			if (!HasSelectionOptions())
			{
				model.Reset();
				this.GetSystem<RunSystem>().EndLevelUpSelection();
				return;
			}
			if (this.GetModel<RunModel>().Phase != RunPhase.Active) return;

			this.GetSystem<RunSystem>().BeginLevelUpSelection();
			this.SendEvent(new LevelUpSelectionRequestedEvent());
		}

		private bool HasSelectionOptions()
		{
			var loadout = this.GetSystem<PlayerLoadoutSystem>();
			foreach (var runtime in this.GetModel<PlayerLoadoutModel>().Weapons)
			{
				if (loadout.HasEvolution(runtime.RuntimeId)) return true;
			}
			return loadout.HasUpgradeableWeapon() || this.GetSystem<DodgeSystem>().HasUpgrade();
		}

		protected override void OnInit()
		{
			this.RegisterEvent<PlayerLevelUpEvent>(OnPlayerLevelUp);
			Reset();
		}
	}
}
