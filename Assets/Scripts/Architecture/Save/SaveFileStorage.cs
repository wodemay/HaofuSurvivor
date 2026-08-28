using System;
using System.IO;
using System.Text;
using QFramework;

namespace HaoFuSurvivor
{
	public enum SaveValidationResult
	{
		Valid,
		Corrupt,
		UnsupportedVersion
	}

	public enum SaveLoadResult
	{
		Loaded,
		RecoveredFromBackup,
		Missing,
		Corrupt,
		UnsupportedVersion
	}

	public class SaveFileStorage : IUtility
	{
		private const string RecoveryLogFile = "Logs/save-recovery.log";

		public bool TryLoad(string relativePath, string backupRelativePath, Func<string, SaveValidationResult> validate, out string json)
		{
			return TryLoad(relativePath, backupRelativePath, validate, out json, out _);
		}

		public bool TryLoad(string relativePath, string backupRelativePath, Func<string, SaveValidationResult> validate,
			out string json, out SaveLoadResult loadResult)
		{
			json = null;
			loadResult = SaveLoadResult.Missing;
			if (!TryGetPath(relativePath, out var path)) return false;
			var mainResult = TryRead(path, validate, out json, out var reason);
			if (mainResult == SaveValidationResult.Valid)
			{
				loadResult = SaveLoadResult.Loaded;
				return true;
			}
			if (mainResult == SaveValidationResult.UnsupportedVersion)
			{
				Record("load", relativePath, "version", reason, "blocked");
				loadResult = SaveLoadResult.UnsupportedVersion;
				return false;
			}
			if (mainResult == SaveValidationResult.Corrupt) Quarantine(path, reason);

			if (!TryGetPath(backupRelativePath, out var backupPath))
			{
				loadResult = mainResult == SaveValidationResult.Corrupt ? SaveLoadResult.Corrupt : SaveLoadResult.Missing;
				return false;
			}
			var backupResult = TryRead(backupPath, validate, out json, out reason);
			if (backupResult == SaveValidationResult.Valid)
			{
				if (!TryWrite(relativePath, backupRelativePath, json, validate, out var restoreError))
					Record("restore", relativePath, "backup_restore", restoreError, "loaded_backup");
				loadResult = SaveLoadResult.RecoveredFromBackup;
				return true;
			}
			if (backupResult == SaveValidationResult.UnsupportedVersion)
			{
				Record("load", backupRelativePath, "version", reason, "blocked");
				loadResult = SaveLoadResult.UnsupportedVersion;
			}
			else if (backupResult == SaveValidationResult.Corrupt)
			{
				Quarantine(backupPath, reason);
				loadResult = SaveLoadResult.Corrupt;
			}
			else if (mainResult == SaveValidationResult.Corrupt) loadResult = SaveLoadResult.Corrupt;
			return false;
		}

		public bool TryWrite(string relativePath, string backupRelativePath, string json, Func<string, SaveValidationResult> validate, out string error)
		{
			error = null;
			if (string.IsNullOrEmpty(json) || validate(json) != SaveValidationResult.Valid)
			{
				error = "serialized_data_invalid";
				return false;
			}
			if (!TryGetPath(relativePath, out var path) || !TryGetPath(backupRelativePath, out var backupPath))
			{
				error = "storage_path_unavailable";
				return false;
			}
			var temporaryPath = path + ".tmp";
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllText(temporaryPath, json, new UTF8Encoding(false));
				if (validate(File.ReadAllText(temporaryPath, Encoding.UTF8)) != SaveValidationResult.Valid)
					throw new InvalidDataException("temporary_validation_failed");
				if (File.Exists(path))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
					File.Copy(path, backupPath, true);
					File.Replace(temporaryPath, path, null);
				}
				else File.Move(temporaryPath, path);
				return true;
			}
			catch (Exception exception)
			{
				error = exception.Message;
				Record("write", relativePath, "write", exception.Message, "failed");
				try { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); } catch { }
				return false;
			}
		}

		private static SaveValidationResult TryRead(string path, Func<string, SaveValidationResult> validate, out string json, out string reason)
		{
			json = null;
			reason = null;
			if (!File.Exists(path)) return SaveValidationResult.Corrupt;
			try
			{
				json = File.ReadAllText(path, Encoding.UTF8);
				var result = validate(json);
				if (result != SaveValidationResult.Valid) reason = result == SaveValidationResult.UnsupportedVersion ? "unsupported_version" : "validation_failed";
				return result;
			}
			catch (Exception exception)
			{
				reason = exception.Message;
				return SaveValidationResult.Corrupt;
			}
		}

		private static void Quarantine(string path, string reason)
		{
			if (!File.Exists(path)) return;
			var corruptPath = path + ".corrupt-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
			try
			{
				var suffix = 0;
				while (File.Exists(corruptPath)) corruptPath = path + ".corrupt-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + (++suffix).ToString();
				File.Move(path, corruptPath);
				Record("quarantine", path, "validation", reason, corruptPath);
			}
			catch (Exception exception)
			{
				Record("quarantine", path, "rename", exception.Message, "failed");
			}
		}

		private static void Record(string fileType, string path, string stage, string reason, string result)
		{
			try
			{
				if (!GameArchitecture.Interface.GetUtility<GameStoragePath>().TryGetPath(RecoveryLogFile, out var logPath)) return;
				Directory.CreateDirectory(Path.GetDirectoryName(logPath));
				var line = $"[{DateTime.UtcNow:O}] file={fileType} path={path} stage={stage} reason={reason} result={result}{Environment.NewLine}";
				File.AppendAllText(logPath, line, new UTF8Encoding(false));
			}
			catch
			{
			}
		}

		private static bool TryGetPath(string relativePath, out string path) => GameArchitecture.Interface.GetUtility<GameStoragePath>().TryGetPath(relativePath, out path);
	}
}
