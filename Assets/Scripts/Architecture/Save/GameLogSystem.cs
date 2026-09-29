using System;
using System.IO;
using System.Text;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class GameLogSystem : AbstractSystem
	{
		private readonly object mSync = new object();
		private StreamWriter mWriter;
		private const long MaxLogBytes = 10 * 1024 * 1024;
		private string mPath;
		private string mLastCondition;
		private string mLastStack;
		private LogType mLastType;
		private long mRepeated;
		private long mLastWriteTick;

		protected override void OnInit()
		{
			try
			{
				if (!GameArchitecture.Interface.GetUtility<GameStoragePath>().TryGetPath("Logs/game.log", out var path)) return;
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				mPath = path;
				if (File.Exists(path) && new FileInfo(path).Length >= MaxLogBytes)
					File.Move(path, path + ".archive-" + Guid.NewGuid().ToString("N"));
				OpenWriter();
			}
			catch (Exception exception)
			{
				mWriter = null;
				Debug.LogWarning($"Game log file could not be opened: {exception.Message}");
				return;
			}
			Application.logMessageReceivedThreaded += OnLogMessage;
			Application.quitting += CloseWriter;
		}

		private void OnLogMessage(string condition, string stackTrace, LogType type)
		{
			lock (mSync)
			{
				if (mWriter == null) return;
				try
				{
					var tick = System.Diagnostics.Stopwatch.GetTimestamp();
					if (condition == mLastCondition && stackTrace == mLastStack && type == mLastType)
					{
						mRepeated++;
						if (tick - mLastWriteTick < 5 * System.Diagnostics.Stopwatch.Frequency) return;
						WriteRepeated();
						mLastWriteTick = tick;
						RotateIfNeeded();
						return;
					}
					WriteRepeated();
					RotateIfNeeded();
					mLastCondition = condition;
					mLastStack = stackTrace;
					mLastType = type;
					mLastWriteTick = tick;
					mWriter.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{type}] {condition}");
					if (!string.IsNullOrEmpty(stackTrace)) mWriter.WriteLine(stackTrace);
				}
				catch
				{
				}
			}
		}

		private void OpenWriter()
		{
			mWriter = new StreamWriter(new FileStream(mPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), new UTF8Encoding(false)) { AutoFlush = true };
		}

		private void WriteRepeated()
		{
			if (mRepeated == 0) return;
			mWriter.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{mLastType}] Repeated {mRepeated} times: {mLastCondition}");
			mRepeated = 0;
		}

		private void RotateIfNeeded()
		{
			if (mWriter.BaseStream.Length < MaxLogBytes) return;
			mWriter.Dispose();
			mWriter = null;
			var previous = mPath + ".previous";
			if (File.Exists(previous)) File.Delete(previous);
			File.Move(mPath, previous);
			OpenWriter();
		}

		private void CloseWriter()
		{
			lock (mSync)
			{
				if (mWriter == null) return;
				WriteRepeated();
				mWriter.Flush();
				mWriter.Dispose();
				mWriter = null;
			}
		}

		protected override void OnDeinit()
		{
			Application.logMessageReceivedThreaded -= OnLogMessage;
			Application.quitting -= CloseWriter;
			CloseWriter();
		}
	}
}
