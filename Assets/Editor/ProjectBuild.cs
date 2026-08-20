using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using QFramework;

namespace HaoFuSurvivor.Editor
{
	public static class ProjectBuild
	{
		private const string DefaultBuildPath = "Build/StandaloneWindows64/HaofuSurvivor.exe";
		private const string DefaultMacBuildPath = "Build/StandaloneOSX/HaofuSurvivor.app";

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
			BuildStandalone(BuildTarget.StandaloneWindows64, DefaultBuildPath, "Windows");
		}

		public static void BuildMac()
		{
			BuildStandalone(BuildTarget.StandaloneOSX, DefaultMacBuildPath, "Mac");
		}

		private static void BuildStandalone(BuildTarget target, string defaultBuildPath, string platformName)
		{
			var scenes = GetEnabledScenes();
			if (scenes.Length == 0) throw new BuildFailedException("No enabled scenes found in EditorBuildSettings.");
			BuildScript.BuildAssetBundles(target);
			var buildPath = ResolveBuildPath(defaultBuildPath);
			var directory = Path.GetDirectoryName(buildPath);
			if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

			var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
			{
				scenes = scenes,
				locationPathName = buildPath,
				target = target,
				options = BuildOptions.None
			});

			if (report.summary.result != BuildResult.Succeeded)
				throw new BuildFailedException($"{platformName} build failed: {report.summary.result}");
			Debug.Log($"{platformName} build completed: {buildPath}");
		}

		private static string[] GetEnabledScenes()
		{
			return EditorBuildSettings.scenes
				.Where(scene => scene.enabled)
				.Select(scene => scene.path)
				.ToArray();
		}

		private static string ResolveBuildPath(string fallbackPath)
		{
			var args = Environment.GetCommandLineArgs();
			for (var index = 0; index < args.Length - 1; index++)
			{
				if (!string.Equals(args[index], "-buildPath", StringComparison.OrdinalIgnoreCase)) continue;
				var path = args[index + 1];
				if (!string.IsNullOrWhiteSpace(path)) return Path.GetFullPath(path);
			}
			return Path.GetFullPath(fallbackPath);
		}
	}
}
