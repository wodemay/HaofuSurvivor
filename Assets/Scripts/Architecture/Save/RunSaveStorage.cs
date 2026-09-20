using System;
using System.Collections.Generic;
using QFramework;
using System.IO;
using System.Text;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class RunSaveStorage : IUtility
	{
		private const string SaveFile = "SaveData/active-run.json";
		private const string BackupFile = "SaveData/active-run.json.bak";
		private const int CurrentSaveVersion = 2;

		public bool HasSave()
		{
			return TryGetSavePath(SaveFile, out var path) && File.Exists(path) ||
				TryGetSavePath(BackupFile, out var backupPath) && File.Exists(backupPath);
		}

		public bool Save(RunSaveData data, out string error)
		{
			error = null;
			if (data == null) return false;
			data.SaveVersion = CurrentSaveVersion;
			var json = JsonUtility.ToJson(data);
			if (!GameArchitecture.Interface.GetUtility<SaveFileStorage>().TryWrite(SaveFile, BackupFile, json, Validate, out var writeError))
			{
				error = writeError;
				Debug.LogError($"Run save failed: {error}");
				return false;
			}
			return true;
		}

		public bool Save(RunSaveData data) => Save(data, out _);

		public RunSaveData Load(out SaveLoadResult loadResult)
		{
			if (!GameArchitecture.Interface.GetUtility<SaveFileStorage>().TryLoad(SaveFile, BackupFile, Validate, out var json, out loadResult)) return null;
			try
			{
				var data = JsonUtility.FromJson<RunSaveData>(json);
				if (data == null) return null;
				if (data.SaveVersion < CurrentSaveVersion)
				{
					data.SaveVersion = CurrentSaveVersion;
					if (string.IsNullOrEmpty(data.RunId))
					{
						using var hash = System.Security.Cryptography.SHA256.Create();
						var identity = data.HasMapSnapshot
							? $"{data.WorldSeed}:{data.MapThemeId}:{data.MapGeneratorVersion}:{data.CharacterId}"
							: json;
						data.RunId = BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(identity))).Replace("-", "").Substring(0, 32);
					}
					if (!Save(data)) return null;
				}
				return data;
			}
			catch (Exception exception)
			{
				Debug.LogError($"Run save parse failed: {exception.Message}");
				return null;
			}
		}

		public RunSaveData Load()
		{
			return Load(out _);
		}

		public void Clear()
		{
			try
			{
				if (!TryGetSavePath(SaveFile, out var path)) return;
				if (File.Exists(path)) File.Delete(path);
				if (TryGetSavePath(BackupFile, out var backupPath) && File.Exists(backupPath)) File.Delete(backupPath);
				var temporaryPath = path + ".tmp";
				if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
			}
			catch (Exception exception)
			{
				Debug.LogWarning($"Run save cleanup failed: {exception.Message}");
			}
		}

		public static SaveValidationResult ValidateJson(string json)
		{
			if (string.IsNullOrEmpty(json)) return SaveValidationResult.Corrupt;
			try
			{
				var data = JsonUtility.FromJson<RunSaveData>(json);
				if (data.SaveVersion > CurrentSaveVersion) return SaveValidationResult.UnsupportedVersion;
				return ValidateData(data);
			}
			catch
			{
				return SaveValidationResult.Corrupt;
			}
		}

		private static SaveValidationResult Validate(string json) => ValidateJson(json);

		private static SaveValidationResult ValidateData(RunSaveData data)
		{
			if (data == null || data.SaveVersion < 0 || data.CharacterId <= 0) return SaveValidationResult.Corrupt;
			if (!GameArchitecture.Interface.GetUtility<CharacterCatalog>().Contains(data.CharacterId)) return SaveValidationResult.Corrupt;
			if (data.SaveVersion >= 2 && (!IsValidId(data.RunId) || data.RunId.Length > 64)) return SaveValidationResult.Corrupt;
			if (!IsFiniteNonNegative(data.ElapsedSeconds) || !IsFiniteNonNegative(data.CurrentHealth) ||
				!IsFinite(data.PositionX) || !IsFinite(data.PositionY) || !IsFiniteNonNegative(data.CurrentExperience) ||
				!IsFinitePositive(data.RequiredExperience) || !IsFiniteNonNegative(data.DamageInvulnerabilityRemaining) ||
				!IsFiniteNonNegative(data.DodgeInvulnerabilityRemaining) || !IsFiniteNonNegative(data.DodgeCooldownRemaining) ||
				!IsFiniteNonNegative(data.DodgeDurationRemaining) || !IsFinite(data.DodgeDirectionX) || !IsFinite(data.DodgeDirectionY) ||
				!IsFiniteNonNegative(data.EnemySpawnElapsed) || !IsFiniteNonNegative(data.MapEventSpawnElapsed)) return SaveValidationResult.Corrupt;
			if (data.Level < 1 || data.NormalKillCount < 0 || data.BossKillCount < 0 || data.EndlessRound < 0 || data.CurrentStageIndex < -1 ||
				data.MapEventSpawnIndex < 0 || data.SavedPhase < (int)RunPhase.Active || data.SavedPhase > (int)RunPhase.Victory) return SaveValidationResult.Corrupt;
			if (data.HasMapSnapshot && (data.MapThemeId <= 0 || data.MapGeneratorVersion < 0)) return SaveValidationResult.Corrupt;
			if (!BigCoin.TryParse(string.IsNullOrEmpty(data.RunCoin) ? "0" : data.RunCoin, out _)) return SaveValidationResult.Corrupt;

			var timeline = GameArchitecture.Interface.GetUtility<RunTimelineCatalog>().Config;
			if (timeline != null && data.CurrentStageIndex >= timeline.Stages.Count) return SaveValidationResult.Corrupt;
			if (!ValidateDodge(data) || !ValidateStats(data) || !ValidatePerks(data) || !ValidateLevelUp(data) ||
				!ValidateLoadout(data) || !ValidateRuntimeLists(data)) return SaveValidationResult.Corrupt;
			return SaveValidationResult.Valid;
		}

		private static bool ValidateDodge(RunSaveData data)
		{
			if (data.DodgeId == 0) return data.DodgeLevel == 0;
			var config = GameArchitecture.Interface.GetUtility<DodgeCatalog>().Get(data.DodgeId);
			return config != null && data.DodgeLevel >= 1 && data.DodgeLevel <= Mathf.Max(1, config.MaxLevel);
		}

		private static bool ValidateStats(RunSaveData data)
		{
			var seen = new HashSet<int>();
			var catalog = GameArchitecture.Interface.GetUtility<StatUpgradeCatalog>();
			if (data.StatUpgrades == null) return true;
			foreach (var entry in data.StatUpgrades)
			{
				var definition = entry == null ? null : catalog.Get(entry.UpgradeId);
				if (definition == null || entry.Level < 0 || entry.Level > definition.MaxLevel || !seen.Add(entry.UpgradeId)) return false;
			}
			return true;
		}

		private static bool ValidatePerks(RunSaveData data)
		{
			var seen = new HashSet<int>();
			var catalog = GameArchitecture.Interface.GetUtility<CharacterExclusivePerkCatalog>();
			if (data.CharacterPerks != null)
				foreach (var entry in data.CharacterPerks)
				{
					var definition = entry == null ? null : catalog.Get(entry.PerkId);
					if (definition == null || entry.Level < 0 || entry.Level > definition.MaxLevel || !seen.Add(entry.PerkId)) return false;
				}
			var runtime = data.CharacterPerkRuntime;
			return runtime != null && IsFiniteNonNegative(runtime.DodgeProjectileBonusRemaining) && IsFiniteNonNegative(runtime.SkillCooldownBonusRemaining);
		}

		private static bool ValidateLevelUp(RunSaveData data)
		{
			if (data.PendingLevelSelections != null)
				foreach (var level in data.PendingLevelSelections)
					if (level < 1) return false;
			var seen = new HashSet<string>();
			if (data.CurrentLevelOptions == null) return true;
			foreach (var option in data.CurrentLevelOptions)
			{
				if (option == null || string.IsNullOrEmpty(option.CandidateKey) || !seen.Add(option.CandidateKey) ||
					!Enum.IsDefined(typeof(LevelUpOptionType), option.Type) || option.CurrentLevel < 0 || !IsFiniteNonNegative(option.Weight)) return false;
			}
			return true;
		}

		private static bool ValidateLoadout(RunSaveData data)
		{
			var weaponIds = new HashSet<int>();
			var weaponRuntimeIds = new HashSet<int>();
			var skillIds = new HashSet<int>();
			var skillRuntimeIds = new HashSet<int>();
			var weapons = data.Weapons ?? new List<WeaponSaveData>();
			if (weapons.Count > PlayerLoadoutModel.MaxWeaponSlots) return false;
			foreach (var weapon in weapons)
			{
				var config = weapon == null ? null : GameArchitecture.Interface.GetUtility<WeaponCatalog>().Get(weapon.WeaponId);
				if (config == null || weapon.RuntimeId <= 0 || !weaponRuntimeIds.Add(weapon.RuntimeId) || !weaponIds.Add(weapon.WeaponId) ||
					weapon.Level < 1 || weapon.Level > Mathf.Max(1, config.MaxLevel) || (weapon.CanUpgrade && !config.CanUpgrade)) return false;
				if (!ValidateAttackIds(weapon.AttackIds) || !ValidateModifiers(weapon.Modifiers)) return false;
			}
			foreach (var skill in data.Skills ?? new List<SkillSaveData>())
			{
				var config = skill == null ? null : GameArchitecture.Interface.GetUtility<SkillCatalog>().Get(skill.SkillId);
				if (config == null || skill.RuntimeId >= 0 || !skillRuntimeIds.Add(skill.RuntimeId) || !skillIds.Add(skill.SkillId) ||
					skill.Level < 1 || skill.Level > Mathf.Max(1, config.MaxLevel)) return false;
			}
			return true;
		}

		private static bool ValidateModifiers(List<WeaponModifierSaveData> modifiers)
		{
			var seen = new HashSet<string>();
			if (modifiers == null) return true;
			foreach (var modifier in modifiers)
				if (modifier == null || GameArchitecture.Interface.GetUtility<AttackCatalog>().Get(modifier.AttackId) == null ||
					string.IsNullOrEmpty(modifier.Key) || !IsFinite(modifier.Value) || !seen.Add(modifier.AttackId + ":" + modifier.Key)) return false;
			return true;
		}

		private static bool ValidateAttackIds(List<int> attackIds)
		{
			var seen = new HashSet<int>();
			if (attackIds == null) return false;
			foreach (var attackId in attackIds)
				if (attackId <= 0 || GameArchitecture.Interface.GetUtility<AttackCatalog>().Get(attackId) == null || !seen.Add(attackId)) return false;
			return true;
		}

		private static bool ValidateRuntimeLists(RunSaveData data)
		{
			var attackCooldowns = new HashSet<string>();
			foreach (var cooldown in data.AttackCooldowns ?? new List<AttackCooldownSaveData>())
				if (cooldown == null || cooldown.RuntimeId == 0 || GameArchitecture.Interface.GetUtility<AttackCatalog>().Get(cooldown.AttackId) == null ||
					!IsFiniteNonNegative(cooldown.CooldownRemaining) || !attackCooldowns.Add(cooldown.RuntimeId + ":" + cooldown.AttackId)) return false;
			foreach (var enemy in data.Enemies ?? new List<EnemySaveData>())
				if (enemy == null || GameArchitecture.Interface.GetUtility<EnemyCatalog>().Get(enemy.ConfigId) == null || !IsFinite(enemy.PositionX) || !IsFinite(enemy.PositionY) ||
					!IsFinitePositive(enemy.CurrentHealth) || !IsFiniteNonNegative(enemy.MoveSpeed) || !ValidateEnemyCooldowns(enemy.AttackCooldowns)) return false;
			foreach (var drop in data.ExperienceDrops ?? new List<ExperienceDropSaveData>())
				if (drop == null || !HasExperienceConfig(drop.ConfigId) || drop.Experience <= 0f || !IsFinite(drop.Experience) || !IsFinite(drop.PositionX) || !IsFinite(drop.PositionY) || !IsFiniteNonNegative(drop.AbsorbSpeed)) return false;
			foreach (var projectile in data.Projectiles ?? new List<ProjectileSaveData>())
				if (projectile == null || GameArchitecture.Interface.GetUtility<AttackCatalog>().Get(projectile.AttackId) == null || !Enum.IsDefined(typeof(CombatFaction), projectile.OwnerFaction) ||
					!IsFinite(projectile.PositionX) || !IsFinite(projectile.PositionY) || !IsFinite(projectile.DirectionX) || !IsFinite(projectile.DirectionY) ||
					new Vector2(projectile.DirectionX, projectile.DirectionY).sqrMagnitude <= 0.0001f || !IsFinitePositive(projectile.Damage) || !IsFinitePositive(projectile.MoveSpeed) ||
					!IsFiniteNonNegative(projectile.RemainingLifetime) || projectile.RemainingPierce < 0) return false;
			foreach (var flame in data.GroundFlames ?? new List<GroundFlameSaveData>())
				if (flame == null || GameArchitecture.Interface.GetUtility<AttackCatalog>().Get(flame.AttackId) == null || !Enum.IsDefined(typeof(CombatFaction), flame.OwnerFaction) ||
					!IsFinite(flame.PositionX) || !IsFinite(flame.PositionY) || !IsFinitePositive(flame.Damage) || !IsFiniteNonNegative(flame.RemainingDuration) || !IsFiniteNonNegative(flame.RemainingUntilTick)) return false;
			foreach (var effect in data.TimedEffects ?? new List<TimedEffectSaveData>())
				if (effect == null || GameArchitecture.Interface.GetUtility<AttackCatalog>().Get(effect.AttackId) == null || !IsFinite(effect.PositionX) || !IsFinite(effect.PositionY) || !IsFiniteNonNegative(effect.RemainingDuration)) return false;
			foreach (var barrage in data.Barrages ?? new List<BarrageSaveData>())
				if (barrage == null || GameArchitecture.Interface.GetUtility<AttackCatalog>().Get(barrage.AttackId) == null || !Enum.IsDefined(typeof(CombatFaction), barrage.OwnerFaction) || !IsFinitePositive(barrage.Damage) || !IsFiniteNonNegative(barrage.DurationRemaining) || !IsFiniteNonNegative(barrage.TimeUntilNextProjectile) || !IsFinite(barrage.OrbitAngle)) return false;
			var breakableIds = new HashSet<string>();
			foreach (var breakable in data.Breakables ?? new List<BreakableSaveData>())
				if (breakable == null || !IsValidId(breakable.StableId) || !breakableIds.Add(breakable.StableId)) return false;
			var eventIds = new HashSet<string>();
			foreach (var mapEvent in data.MapEvents ?? new List<MapEventSaveData>())
				if (mapEvent == null || !IsValidId(mapEvent.StableId) || !eventIds.Add(mapEvent.StableId) || GameArchitecture.Interface.GetUtility<MapEventCatalog>().Get(mapEvent.ConfigId) == null ||
					!IsFinite(mapEvent.PositionX) || !IsFinite(mapEvent.PositionY) || mapEvent.Progress < 0 || !IsFiniteNonNegative(mapEvent.HoldElapsed)) return false;
			foreach (var pickup in data.Pickups ?? new List<PickupSaveData>())
			{
				var table = pickup == null ? null : GameArchitecture.Interface.GetUtility<DropTableCatalog>().Get(pickup.TableId);
				if (table == null || table.Get(pickup.EntryId) == null || !IsFinite(pickup.PositionX) || !IsFinite(pickup.PositionY) || !IsFiniteNonNegative(pickup.AbsorbSpeed)) return false;
			}
			return true;
		}

		private static bool ValidateEnemyCooldowns(List<AttackCooldownSaveData> cooldowns)
		{
			var seen = new HashSet<int>();
			if (cooldowns == null) return true;
			foreach (var cooldown in cooldowns)
				if (cooldown == null || GameArchitecture.Interface.GetUtility<AttackCatalog>().Get(cooldown.AttackId) == null || !IsFiniteNonNegative(cooldown.CooldownRemaining) || !seen.Add(cooldown.AttackId)) return false;
			return true;
		}

		private static bool HasExperienceConfig(int configId)
		{
			foreach (var config in Resources.LoadAll<ExperienceDropConfig>("Configs/Progression/Experience"))
				if (config != null && config.Id == configId) return true;
			return false;
		}

		private static bool IsValidId(string value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 128 && value.IndexOfAny(new[] { '\\', '/', '\r', '\n', '\t' }) < 0;
		private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
		private static bool IsFiniteNonNegative(float value) => IsFinite(value) && value >= 0f;
		private static bool IsFinitePositive(float value) => IsFinite(value) && value > 0f;

		private static bool TryGetSavePath(string relativePath, out string path) => GameArchitecture.Interface.GetUtility<GameStoragePath>().TryGetPath(relativePath, out path);

	}
}
