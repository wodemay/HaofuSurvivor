using System;
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
		private const int CurrentSaveVersion = 1;

		public bool HasSave()
		{
			return TryGetSavePath(SaveFile, out var path) && File.Exists(path) ||
				TryGetSavePath(BackupFile, out var backupPath) && File.Exists(backupPath);
		}

		public void Save(RunSaveData data)
		{
			if (data == null) return;
			data.SaveVersion = CurrentSaveVersion;
			var json = JsonUtility.ToJson(data);
			if (!GameArchitecture.Interface.GetUtility<SaveFileStorage>().TryWrite(SaveFile, BackupFile, json, Validate, out var error))
				Debug.LogError($"Run save failed: {error}");
		}

		public RunSaveData Load()
		{
			if (!GameArchitecture.Interface.GetUtility<SaveFileStorage>().TryLoad(SaveFile, BackupFile, Validate, out var json)) return null;
			try
			{
				var data = JsonUtility.FromJson<RunSaveData>(json);
				if (data.SaveVersion == 0)
				{
					data.SaveVersion = CurrentSaveVersion;
					Save(data);
				}
				return data;
			}
			catch (Exception exception)
			{
				Debug.LogError($"Run save parse failed: {exception.Message}");
				return null;
			}
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

		private static SaveValidationResult Validate(string json)
		{
			if (string.IsNullOrEmpty(json)) return SaveValidationResult.Corrupt;
			try
			{
				var data = JsonUtility.FromJson<RunSaveData>(json);
				if (data == null || data.CharacterId <= 0) return SaveValidationResult.Corrupt;
				if (data.SaveVersion > CurrentSaveVersion) return SaveValidationResult.UnsupportedVersion;
				if (data.SaveVersion < 0) return SaveValidationResult.Corrupt;
				return SaveValidationResult.Valid;
			}
			catch
			{
				return SaveValidationResult.Corrupt;
			}
		}

		private static bool TryGetSavePath(string relativePath, out string path) => GameArchitecture.Interface.GetUtility<GameStoragePath>().TryGetPath(relativePath, out path);

	}
}
