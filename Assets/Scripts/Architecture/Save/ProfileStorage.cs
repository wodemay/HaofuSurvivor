using System;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public enum ProfileLoadStatus
	{
		Loaded,
		CreatedDefault,
		CorruptRecovered,
		UnsupportedVersion
	}

	public class ProfileStorage : IUtility
	{
		private const string SaveFile = "SaveData/profile.json";
		private const string BackupFile = "SaveData/profile.json.bak";
		private const int CurrentSaveVersion = 3;

		public bool HasProfile()
		{
			return GameArchitecture.Interface.GetUtility<GameStoragePath>().TryGetPath(SaveFile, out var path) &&
				System.IO.File.Exists(path) ||
				GameArchitecture.Interface.GetUtility<GameStoragePath>().TryGetPath(BackupFile, out var backupPath) &&
				System.IO.File.Exists(backupPath);
		}

		public bool TryLoad(out ProfileData data, out ProfileLoadStatus status)
		{
			data = null;
			status = ProfileLoadStatus.CorruptRecovered;
			var storage = GameArchitecture.Interface.GetUtility<SaveFileStorage>();
			if (!storage.TryLoad(SaveFile, BackupFile, Validate, out var json, out var result))
			{
				if (result == SaveLoadResult.UnsupportedVersion) status = ProfileLoadStatus.UnsupportedVersion;
				return false;
			}

			try
			{
				data = JsonUtility.FromJson<ProfileData>(json);
				if (!IsValidData(data, out var coin)) return false;
				data.ProfileCoin = coin.ToString();
				data.SettledRunIds ??= new System.Collections.Generic.List<string>();
				if (data.SaveVersion < CurrentSaveVersion)
				{
					data.SaveVersion = CurrentSaveVersion;
					Save(data);
				}
				status = ProfileLoadStatus.Loaded;
				return true;
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogError($"Profile parse failed: {exception.Message}");
				return false;
			}
		}

		public bool Save(ProfileData data, out string error)
		{
			error = null;
			if (data == null) return false;
			data.SaveVersion = CurrentSaveVersion;
			if (!IsValidData(data, out var coin))
			{
				error = "profile_coin_invalid";
				return false;
			}
			data.ProfileCoin = coin.ToString();
			data.SettledRunIds ??= new System.Collections.Generic.List<string>();
			var json = JsonUtility.ToJson(data);
			return GameArchitecture.Interface.GetUtility<SaveFileStorage>().TryWrite(SaveFile, BackupFile, json, Validate, out error);
		}

		public bool Save(ProfileData data) => Save(data, out _);

		private static SaveValidationResult Validate(string json)
		{
			if (string.IsNullOrEmpty(json)) return SaveValidationResult.Corrupt;
			try
			{
				var data = JsonUtility.FromJson<ProfileData>(json);
				if (data != null && data.SaveVersion > CurrentSaveVersion) return SaveValidationResult.UnsupportedVersion;
				if (!IsValidData(data, out _)) return SaveValidationResult.Corrupt;
				if (data.SaveVersion < 0) return SaveValidationResult.Corrupt;
				return SaveValidationResult.Valid;
			}
			catch
			{
				return SaveValidationResult.Corrupt;
			}
		}

		private static bool IsValidData(ProfileData data, out BigCoin coin)
		{
			coin = null;
			if (data == null || !BigCoin.TryParse(data.ProfileCoin, out coin)) return false;
			if (data.SaveVersion < 0) return false;
			var seenRunIds = new System.Collections.Generic.HashSet<string>();
			if (data.SettledRunIds != null)
				foreach (var runId in data.SettledRunIds)
					if (string.IsNullOrWhiteSpace(runId) || runId.Length > 64 || !seenRunIds.Add(runId)) return false;

			var catalog = GameArchitecture.Interface.GetUtility<MetaUpgradeCatalog>();
			var seenUpgrades = new System.Collections.Generic.HashSet<int>();
			if (data.MetaUpgrades != null)
				foreach (var entry in data.MetaUpgrades)
				{
					var definition = entry == null ? null : catalog.Get(entry.UpgradeId);
					if (definition == null || entry.Level < 0 || entry.Level > definition.MaxLevel || !seenUpgrades.Add(entry.UpgradeId)) return false;
				}
			return true;
		}
	}
}
