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

		protected override void OnInit()
		{
			try
			{
				if (!GameArchitecture.Interface.GetUtility<GameStoragePath>().TryGetPath("Logs/game.log", out var path)) return;
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				mWriter = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), new UTF8Encoding(false))
				{
					AutoFlush = true
				};
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
					mWriter.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{type}] {condition}");
					if (!string.IsNullOrEmpty(stackTrace)) mWriter.WriteLine(stackTrace);
				}
				catch
				{
				}
			}
		}

		private void CloseWriter()
		{
			lock (mSync)
			{
				if (mWriter == null) return;
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
