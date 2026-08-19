using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using QFramework;

namespace HaoFuSurvivor.Editor
{
	public static class ProjectBuild
	{
		private const string DefaultBuildPath = "Build/StandaloneWindows64/HaofuSurvivor.exe";

		public static void ValidateProject()
		{
			var scenes = GetEnabledScenes();
			if (scenes.Length == 0) throw new BuildFailedException("No enabled scenes found in EditorBuildSettings.");
			foreach (var scene in scenes)
			{
				if (!File.Exists(scene)) throw new BuildFailedException($"Build scene does not exist: {scene}");
			}
			Debug.Log($"Project validation passed. Enabled scenes: {scenes.Length}.");
		}

		public static void BuildWindows()
		{
			var scenes = GetEnabledScenes();
			if (scenes.Length == 0) throw new BuildFailedException("No enabled scenes found in EditorBuildSettings.");
			BuildScript.BuildAssetBundles(BuildTarget.StandaloneWindows64);
			var buildPath = ResolveBuildPath();
			var directory = Path.GetDirectoryName(buildPath);
			if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

			var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
			{
				scenes = scenes,
				locationPathName = buildPath,
				target = BuildTarget.StandaloneWindows64,
				options = BuildOptions.None
			});

			if (report.summary.result != BuildResult.Succeeded)
				throw new BuildFailedException($"Windows build failed: {report.summary.result}");
			Debug.Log($"Windows build completed: {buildPath}");
		}

		private static string[] GetEnabledScenes()
		{
			return EditorBuildSettings.scenes
				.Where(scene => scene.enabled)
				.Select(scene => scene.path)
				.ToArray();
		}

		private static string ResolveBuildPath()
		{
			var args = Environment.GetCommandLineArgs();
			for (var index = 0; index < args.Length - 1; index++)
			{
				if (!string.Equals(args[index], "-buildPath", StringComparison.OrdinalIgnoreCase)) continue;
				var path = args[index + 1];
				if (!string.IsNullOrWhiteSpace(path)) return Path.GetFullPath(path);
			}
			return Path.GetFullPath(DefaultBuildPath);
		}
	}
}
