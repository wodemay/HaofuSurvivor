using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
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

	public class GetLevelUpOptionsQuery : AbstractQuery<IReadOnlyList<LevelUpOption>>
	{
		protected override IReadOnlyList<LevelUpOption> OnDo()
		{
			return this.GetModel<LevelUpModel>().CurrentOptions;
		}
	}

	public class LevelUpSystem : AbstractSystem
	{
		public void Reset()
		{
			this.GetModel<LevelUpModel>().Reset();
		}

		public bool CompleteOption(LevelUpOption option)
		{
			var model = this.GetModel<LevelUpModel>();
			if (model.PendingSelectionCount <= 0 || !model.ContainsOption(option.CandidateKey)) return false;

			var completed = option.Type switch
			{
				LevelUpOptionType.Weapon when option.IsNewWeapon => this.GetSystem<PlayerLoadoutSystem>().AcquireWeapon(option.ContentId),
				LevelUpOptionType.Weapon => this.GetSystem<PlayerLoadoutSystem>().UpgradeWeapon(option.RuntimeId),
				LevelUpOptionType.WeaponCombination => this.GetSystem<WeaponCombinationSystem>().Combine(option.ContentId),
				LevelUpOptionType.Dodge => this.GetSystem<DodgeSystem>().Upgrade(),
				LevelUpOptionType.Skill => this.GetSystem<CharacterExclusiveSkillUpgradeSystem>().Upgrade(option.ContentId),
				LevelUpOptionType.Stat => this.GetSystem<PlayerStatUpgradeSystem>().Upgrade(option.ContentId),
				LevelUpOptionType.CharacterPerk => this.GetSystem<CharacterExclusivePerkSystem>().Upgrade(option.ContentId),
				_ => false
			};
			if (!completed)
			{
				GenerateCurrentOptions();
				PresentNextSelection();
				return false;
			}

			model.CompleteCurrent();
			PresentNextSelection();
			return true;
		}

		private void OnPlayerLevelUp(PlayerLevelUpEvent levelUpEvent)
		{
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
			if (model.CurrentOptions.Count == 0) GenerateCurrentOptions();
			if (model.CurrentOptions.Count == 0)
			{
				model.CompleteCurrent();
				PresentNextSelection();
				return;
			}
			var runModel = this.GetModel<RunModel>();
			if (runModel.Phase == RunPhase.LevelUpSelection) return;
			if (runModel.Phase != RunPhase.Active) return;

			this.GetSystem<RunSystem>().BeginLevelUpSelection();
			this.SendEvent(new LevelUpSelectionRequestedEvent());
		}

		private void GenerateCurrentOptions()
		{
			var pool = new List<LevelUpOption>();
			var catalog = this.GetUtility<WeaponCatalog>();
			foreach (var runtime in this.GetModel<PlayerLoadoutModel>().Weapons)
			{
				var config = catalog.Get(runtime.WeaponId);
				if (config == null) continue;
				if (runtime.CanUpgrade && runtime.Level < config.MaxLevel)
				{
					var upgrade = config.LevelUpgrades.Find(item => item != null && item.Level == runtime.Level + 1);
					pool.Add(LevelUpOption.CreateWeapon(runtime, config, upgrade?.Description));
					continue;
				}
			}

			var combinations = this.GetSystem<WeaponCombinationSystem>().GetEligible();
			var combinationOptions = new List<LevelUpOption>();
			foreach (var combination in combinations)
			{
				if (Random.value > Mathf.Clamp01(combination.CandidateChance)) continue;
				var target = catalog.Get(combination.TargetWeaponId);
				if (target != null) combinationOptions.Add(LevelUpOption.CreateWeaponCombination(combination, target));
			}

			var loadout = this.GetSystem<PlayerLoadoutSystem>();
			var weaponCatalogConfig = catalog.Config;
			if (this.GetModel<PlayerLoadoutModel>().HasAvailableWeaponSlot && weaponCatalogConfig != null)
			{
				foreach (var config in weaponCatalogConfig.Weapons)
					if (config != null && loadout.CanAcquireWeapon(config.Id))
						pool.Add(LevelUpOption.CreateWeaponAcquisition(config));
			}

			var dodge = this.GetModel<DodgeModel>().Runtime;
			var dodgeConfig = dodge == null ? null : this.GetUtility<DodgeCatalog>().Get(dodge.DodgeId);
			if (dodge != null && dodgeConfig != null && this.GetSystem<DodgeSystem>().HasUpgrade())
			{
				var upgrade = dodgeConfig.LevelUpgrades.Find(item => item != null && item.Level == dodge.Level + 1);
				pool.Add(LevelUpOption.CreateDodge(dodge, dodgeConfig, upgrade?.Description));
			}

			var statCatalog = this.GetUtility<StatUpgradeCatalog>().Config;
			if (statCatalog != null)
			{
				var statUpgrades = this.GetSystem<PlayerStatUpgradeSystem>();
				foreach (var definition in statCatalog.Upgrades)
					if (definition != null && statUpgrades.HasUpgrade(definition.Id))
						pool.Add(LevelUpOption.CreateStat(definition, statUpgrades.GetLevel(definition.Id)));
			}

			var perks = this.GetSystem<CharacterExclusivePerkSystem>();
			foreach (var definition in perks.GetEligible())
				pool.Add(LevelUpOption.CreateCharacterPerk(definition, perks.GetLevel(definition.Id)));

			var selected = new List<LevelUpOption>();
			var exclusiveSkillUpgrade = this.GetSystem<CharacterExclusiveSkillUpgradeSystem>().GetEligible();
			if (exclusiveSkillUpgrade != null)
			{
				var runtime = this.GetModel<PlayerLoadoutModel>().GetSkillById(exclusiveSkillUpgrade.SkillId);
				if (runtime != null) selected.Add(LevelUpOption.CreateSkill(runtime, exclusiveSkillUpgrade));
			}
			while (combinationOptions.Count > 0 && selected.Count < 3)
			{
				var index = Random.Range(0, combinationOptions.Count);
				selected.Add(combinationOptions[index]);
				combinationOptions.RemoveAt(index);
			}
			while (pool.Count > 0 && selected.Count < 3)
			{
				selected.Add(PickWeightedOption(pool));
			}
			this.GetModel<LevelUpModel>().SetCurrentOptions(selected);
		}

		private static LevelUpOption PickWeightedOption(List<LevelUpOption> pool)
		{
			var totalWeight = 0f;
			foreach (var option in pool) totalWeight += Mathf.Max(0.01f, option.Weight);
			var roll = Random.value * totalWeight;
			for (var index = 0; index < pool.Count; index++)
			{
				roll -= Mathf.Max(0.01f, pool[index].Weight);
				if (roll > 0f) continue;
				var selected = pool[index];
				pool.RemoveAt(index);
				return selected;
			}
			var fallback = pool[^1];
			pool.RemoveAt(pool.Count - 1);
			return fallback;
		}

		protected override void OnInit()
		{
			this.RegisterEvent<PlayerLevelUpEvent>(OnPlayerLevelUp);
			Reset();
		}
	}
}
