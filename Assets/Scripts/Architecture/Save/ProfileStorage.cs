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
		private const int CurrentSaveVersion = 1;

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
				if (data == null || !BigCoin.TryParse(data.ProfileCoin, out var coin)) return false;
				data.ProfileCoin = coin.ToString();
				if (data.SaveVersion == 0)
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
			if (!BigCoin.TryParse(data.ProfileCoin, out _))
			{
				error = "profile_coin_invalid";
				return false;
			}
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
				if (data == null || !BigCoin.TryParse(data.ProfileCoin, out _)) return SaveValidationResult.Corrupt;
				if (data.SaveVersion > CurrentSaveVersion) return SaveValidationResult.UnsupportedVersion;
				if (data.SaveVersion < 0) return SaveValidationResult.Corrupt;
				return SaveValidationResult.Valid;
			}
			catch
			{
				return SaveValidationResult.Corrupt;
			}
		}
	}
}
