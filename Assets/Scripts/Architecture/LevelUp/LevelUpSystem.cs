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

		public LevelUpWeaponOption(WeaponRuntimeData runtime, WeaponConfig config)
		{
			RuntimeId = runtime.RuntimeId;
			WeaponId = runtime.WeaponId;
			CurrentLevel = runtime.Level;
			DisplayName = string.IsNullOrWhiteSpace(config.DisplayName) ? $"Weapon {runtime.WeaponId}" : config.DisplayName;
			Description = config.Description;
			Icon = config.Icon;
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
				if (config == null || !runtime.CanUpgrade || runtime.Level >= config.MaxLevel) continue;
				options.Add(new LevelUpWeaponOption(runtime, config));
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

		public bool CompleteWeaponUpgrade(int weaponRuntimeId)
		{
			var model = this.GetModel<LevelUpModel>();
			if (model.PendingSelectionCount <= 0) return false;
			if (!this.GetSystem<PlayerLoadoutSystem>().UpgradeWeapon(weaponRuntimeId)) return false;

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
			return this.GetSystem<PlayerLoadoutSystem>().HasUpgradeableWeapon();
		}

		protected override void OnInit()
		{
			this.RegisterEvent<PlayerLevelUpEvent>(OnPlayerLevelUp);
			Reset();
		}
	}
}
