using System;
using System.IO;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class GameStoragePath : IUtility
	{
		public string RootDirectory => ResolveRootDirectory();
		private bool mWarnedUnsupportedRoot;

		public string GetPath(string relativePath)
		{
			if (!IsSafeRelativePath(relativePath))
				throw new ArgumentException("Storage path must be relative.", nameof(relativePath));
			if (!IsAsciiPath(RootDirectory))
				throw new InvalidOperationException("Game storage root must use an ASCII-only path.");
			return Path.Combine(RootDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
		}

		public bool TryGetPath(string relativePath, out string path)
		{
			path = null;
			if (!IsSafeRelativePath(relativePath)) return false;
			var root = RootDirectory;
			if (!IsAsciiPath(root))
			{
				if (!mWarnedUnsupportedRoot)
				{
					mWarnedUnsupportedRoot = true;
					Debug.LogWarning("Game storage is disabled because the game root contains non-ASCII characters. Install the game in an ASCII-only path.");
				}
				return false;
			}
			path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
			return true;
		}

		public string EnsureDirectory(string relativeDirectory)
		{
			var path = GetPath(relativeDirectory);
			Directory.CreateDirectory(path);
			return path;
		}

		private static string ResolveRootDirectory()
		{
			var dataDirectory = new DirectoryInfo(Path.GetFullPath(Application.dataPath));
			if (Application.platform == RuntimePlatform.OSXPlayer && dataDirectory.Parent != null &&
				string.Equals(dataDirectory.Parent.Extension, ".app", StringComparison.OrdinalIgnoreCase))
				return dataDirectory.Parent.Parent?.FullName ?? dataDirectory.Parent.FullName;
			return dataDirectory.Parent?.FullName ?? dataDirectory.FullName;
		}

		private static bool IsSafeRelativePath(string relativePath)
		{
			if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath)) return false;
			var normalized = relativePath.Replace('\\', '/');
			foreach (var segment in normalized.Split('/'))
			{
				if (string.IsNullOrEmpty(segment) || segment == "." || segment == "..") return false;
				foreach (var character in segment)
					if (character < 0x20 || character > 0x7E) return false;
			}
			return true;
		}

		private static bool IsAsciiPath(string path)
		{
			if (string.IsNullOrEmpty(path)) return false;
			foreach (var character in path)
				if (character < 0x20 || character > 0x7E) return false;
			return true;
		}
	}
}
