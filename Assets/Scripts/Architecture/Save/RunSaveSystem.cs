using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public readonly struct SavedRunState
	{
		public readonly bool HasSave;

		public SavedRunState(bool hasSave)
		{
			HasSave = hasSave;
		}
	}

	public class HasSavedRunQuery : AbstractQuery<SavedRunState>
	{
		protected override SavedRunState OnDo()
		{
			return new SavedRunState(GameArchitecture.Interface.GetUtility<RunSaveStorage>().HasSave());
		}
	}

	public class RunSaveSystem : AbstractSystem, IRunUpdateable
	{
		private const float AutoSaveIntervalSeconds = 30f;
		private float mAutoSaveElapsed;

		public void ResetAutoSaveTimer()
		{
			mAutoSaveElapsed = 0f;
		}

		public void OnRunUpdate(float deltaTime)
		{
			mAutoSaveElapsed += deltaTime;
			if (mAutoSaveElapsed < AutoSaveIntervalSeconds) return;
			mAutoSaveElapsed = 0f;
			SaveCurrentRun();
		}
		public void SaveCurrentRun()
		{
			var run = this.GetModel<RunModel>();
			if (run.Phase != RunPhase.Active && run.Phase != RunPhase.Paused && run.Phase != RunPhase.LevelUpSelection) return;

			var player = this.GetModel<PlayerModel>();
			var experience = this.GetModel<ExperienceModel>();
			var timer = this.GetModel<RunTimerModel>();
			var world = this.GetModel<WorldMapModel>();
			var data = new RunSaveData
			{
				HasMapSnapshot = world.HasWorld,
				WorldSeed = world.WorldSeed,
				MapThemeId = world.ThemeId,
				MapGeneratorVersion = world.GeneratorVersion,
				SavedPhase = (int)run.Phase,
				RandomStateJson = JsonUtility.ToJson(Random.state),
				CharacterId = player.CharacterId,
				ElapsedSeconds = timer.ElapsedSeconds,
				CurrentStageIndex = timer.CurrentStageIndex,
				CurrentHealth = player.CurrentHealth,
				PositionX = player.Position.x,
				PositionY = player.Position.y,
				Level = experience.Level,
				CurrentExperience = experience.CurrentExperience,
				RequiredExperience = experience.RequiredExperience,
				DodgeId = this.GetModel<PlayerLoadoutModel>().DodgeId,
				DodgeLevel = this.GetModel<DodgeModel>().Runtime?.Level ?? 0,
				HasSkillSnapshot = true,
				DamageInvulnerabilityRemaining = player.DamageInvulnerabilityRemaining,
				DodgeInvulnerabilityRemaining = player.DodgeInvulnerabilityRemaining,
				DodgeCooldownRemaining = this.GetModel<DodgeModel>().Runtime?.CooldownRemaining ?? 0f,
				DodgeDurationRemaining = this.GetModel<DodgeModel>().Runtime?.DurationRemaining ?? 0f,
				DodgeDirectionX = this.GetModel<DodgeModel>().Runtime?.Direction.x ?? 0f,
				DodgeDirectionY = this.GetModel<DodgeModel>().Runtime?.Direction.y ?? 0f,
				DodgeIsActive = this.GetModel<DodgeModel>().Runtime?.IsActive ?? false,
				EnemySpawnElapsed = this.GetSystem<EnemySystem>().GetSpawnElapsed(),
				CharacterPerkRuntime = this.GetSystem<CharacterExclusivePerkSystem>().GetRuntimeSaveData()
			};
			data.StatUpgrades.AddRange(this.GetModel<PlayerStatUpgradeModel>().GetSaveData());
			data.CharacterPerks.AddRange(this.GetModel<CharacterExclusivePerkModel>().GetSaveData());
			data.PendingLevelSelections.AddRange(this.GetModel<LevelUpModel>().GetPendingLevels());
			foreach (var option in this.GetModel<LevelUpModel>().CurrentOptions) data.CurrentLevelOptions.Add(option.GetSaveData());
			data.Enemies.AddRange(this.GetSystem<EnemySystem>().GetSaveData());
			data.ExperienceDrops.AddRange(this.GetSystem<ExperienceSystem>().GetSaveData());
			data.Projectiles.AddRange(this.GetSystem<ProjectileSystem>().GetSaveData());
			data.GroundFlames.AddRange(this.GetSystem<ExplosiveAreaSystem>().GetGroundFlameSaveData());
			data.TimedEffects.AddRange(this.GetSystem<ExplosiveAreaSystem>().GetTimedEffectSaveData());
			data.Barrages.AddRange(this.GetSystem<BarrageProjectileSystem>().GetSaveData());
			data.AttackCooldowns.AddRange(this.GetSystem<AttackSystem>().GetPlayerCooldownSaveData(player.RuntimeRoot));

			foreach (var skill in this.GetModel<PlayerLoadoutModel>().Skills)
				data.Skills.Add(new SkillSaveData { RuntimeId = skill.RuntimeId, SkillId = skill.SkillId, Level = skill.Level });
			foreach (var weapon in this.GetModel<PlayerLoadoutModel>().Weapons)
			{
				var weaponData = new WeaponSaveData
				{
					RuntimeId = weapon.RuntimeId,
					WeaponId = weapon.WeaponId,
					Level = weapon.Level,
					CanUpgrade = weapon.CanUpgrade,
					AttackIds = new List<int>(weapon.AttackIds)
				};
				foreach (var modifier in weapon.GetModifierSnapshots())
					weaponData.Modifiers.Add(new WeaponModifierSaveData { AttackId = modifier.AttackId, Key = modifier.Key, Value = modifier.Value });
				data.Weapons.Add(weaponData);
			}

			this.GetUtility<RunSaveStorage>().Save(data);
		}

		public RunSaveData Load() => this.GetUtility<RunSaveStorage>().Load();

		public void Clear() => this.GetUtility<RunSaveStorage>().Clear();

		public bool Restore(RunSaveData data)
		{
			if (data == null) return false;
			var player = this.GetModel<PlayerModel>();
			player.Position = new Vector2(data.PositionX, data.PositionY);
			if (player.RuntimeRoot != null) player.RuntimeRoot.transform.position = player.Position;
			if (data.HasMapSnapshot && !this.GetSystem<MapSystem>().TryRestoreWorld(data.WorldSeed, data.MapThemeId, data.MapGeneratorVersion)) return false;
			var timer = this.GetModel<RunTimerModel>();
			var experience = this.GetModel<ExperienceModel>();
			player.CurrentHealth = Mathf.Max(0f, data.CurrentHealth);
			player.DamageInvulnerabilityRemaining = Mathf.Max(0f, data.DamageInvulnerabilityRemaining);
			player.DodgeInvulnerabilityRemaining = Mathf.Max(0f, data.DodgeInvulnerabilityRemaining);
			this.GetSystem<RunTimerSystem>().Restore(data.ElapsedSeconds, data.CurrentStageIndex);
			experience.Level = Mathf.Max(1, data.Level);
			experience.CurrentExperience = Mathf.Max(0f, data.CurrentExperience);
			experience.RequiredExperience = Mathf.Max(1f, data.RequiredExperience);
			this.GetSystem<PlayerStatUpgradeSystem>().Restore(data.StatUpgrades);
			this.GetSystem<CharacterExclusivePerkSystem>().Restore(data.CharacterPerks, data.CharacterPerkRuntime);

			var loadout = this.GetSystem<PlayerLoadoutSystem>();
			loadout.Reset();
			this.GetModel<PlayerLoadoutModel>().BindOwner(player.RuntimeRoot);
			loadout.SetDodge(data.DodgeId);
			var dodge = this.GetModel<DodgeModel>().Runtime;
			if (dodge != null) dodge.Level = Mathf.Max(1, data.DodgeLevel);
			this.GetSystem<DodgeSystem>().RestoreRuntime(data.DodgeCooldownRemaining, data.DodgeDurationRemaining,
				new Vector2(data.DodgeDirectionX, data.DodgeDirectionY), data.DodgeIsActive);
			var restoredWeaponCount = 0;
			foreach (var weapon in data.Weapons ?? new List<WeaponSaveData>())
			{
				var runtime = loadout.RestoreWeapon(player.RuntimeRoot, weapon);
				if (runtime == null)
				{
					Debug.LogWarning($"Saved weapon {weapon.WeaponId} was skipped during restore.");
					continue;
				}
				restoredWeaponCount++;
			}
			RestoreSkills(loadout, data, player.RuntimeRoot);
			loadout.RestoreRetiredWeaponsFromCombinations();
			RestoreLevelUp(data);
			this.GetSystem<EnemySystem>().Restore(data.Enemies, data.EnemySpawnElapsed);
			this.GetSystem<ExperienceSystem>().RestoreDrops(data.ExperienceDrops);
			this.GetSystem<ProjectileSystem>().Restore(data.Projectiles);
			this.GetSystem<ExplosiveAreaSystem>().RestoreGroundFlames(data.GroundFlames);
			this.GetSystem<ExplosiveAreaSystem>().RestoreTimedEffects(data.TimedEffects);
			this.GetSystem<BarrageProjectileSystem>().Restore(data.Barrages, player.RuntimeRoot);
			this.GetSystem<AttackSystem>().RestorePlayerCooldowns(player.RuntimeRoot, data.AttackCooldowns);
			RestoreRandomState(data.RandomStateJson);
			this.SendEvent(new PlayerHealthRestoredEvent());
			RestorePhase(data);
			return data.Weapons == null || data.Weapons.Count == 0 || restoredWeaponCount > 0;
		}

		private void RestoreLevelUp(RunSaveData data)
		{
			var options = new List<LevelUpOption>();
			foreach (var option in data.CurrentLevelOptions ?? new List<LevelUpOptionSaveData>())
				if (option != null && !string.IsNullOrEmpty(option.CandidateKey)) options.Add(LevelUpOption.FromSaveData(option));
			this.GetModel<LevelUpModel>().Restore(data.PendingLevelSelections, options);
		}

		private void RestorePhase(RunSaveData data)
		{
			var phase = (RunPhase)data.SavedPhase;
			if (phase == RunPhase.LevelUpSelection && this.GetModel<LevelUpModel>().PendingSelectionCount > 0)
			{
				this.GetSystem<RunSystem>().BeginLevelUpSelection();
				this.SendEvent(new LevelUpSelectionRequestedEvent());
				return;
			}
			if (phase == RunPhase.Paused) this.GetSystem<RunSystem>().Pause();
		}

		private static void RestoreRandomState(string json)
		{
			if (string.IsNullOrEmpty(json)) return;
			try { Random.state = JsonUtility.FromJson<Random.State>(json); }
			catch { }
		}

		private void RestoreSkills(PlayerLoadoutSystem loadout, RunSaveData data, GameObject owner)
		{
			if (data.HasSkillSnapshot)
			{
				foreach (var skill in data.Skills ?? new List<SkillSaveData>()) RestoreSkill(loadout, owner, skill);
				return;
			}

			var character = this.GetUtility<CharacterCatalog>().Get(data.CharacterId);
			var skillGroup = character == null ? null : this.GetUtility<SkillGroupCatalog>().Get(character.SkillGroupId);
			foreach (var skillId in skillGroup?.StartingSkillIds ?? new List<int>())
				RestoreSkill(loadout, owner, new SkillSaveData { SkillId = skillId, Level = 1 });
		}

		private void RestoreSkill(PlayerLoadoutSystem loadout, GameObject owner, SkillSaveData data)
		{
			if (data == null || data.SkillId == 0 || !loadout.EquipSkill(owner, data.SkillId, data.RuntimeId))
			{
				if (data != null && data.SkillId != 0) Debug.LogWarning($"Saved skill {data.SkillId} was skipped during restore.");
				return;
			}
			var skill = data.RuntimeId < 0
				? this.GetModel<PlayerLoadoutModel>().GetSkill(data.RuntimeId)
				: this.GetModel<PlayerLoadoutModel>().GetSkillById(data.SkillId);
			if (skill != null) loadout.RestoreSkill(skill, data.Level);
		}

		protected override void OnInit()
		{
		}
	}
}
