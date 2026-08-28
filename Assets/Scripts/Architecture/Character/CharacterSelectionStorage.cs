using QFramework;
using System;
using System.IO;
using System.Text;
using UnityEngine;
namespace HaoFuSurvivor
{
	public class CharacterSelectionStorage : IUtility
	{
		private const string SelectedCharacterFile = "SaveData/selected-character.json";
		[Serializable] private class SelectedCharacterData { public int Id; }

		public int LoadSelectedCharacterId()
		{
			if (!TryGetPath(out var path)) return -1;
			if (!File.Exists(path)) return -1;
			try { return JsonUtility.FromJson<SelectedCharacterData>(File.ReadAllText(path)).Id; }
			catch { return -1; }
		}

		public void SaveSelectedCharacterId(int characterId)
		{
			try
			{
				if (!TryGetPath(out var path)) return;
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllText(path, JsonUtility.ToJson(new SelectedCharacterData { Id = characterId }), new UTF8Encoding(false));
			}
			catch (Exception exception)
			{
				Debug.LogWarning($"Selected character save failed: {exception.Message}");
			}
		}

		private bool TryGetPath(out string path) => GameArchitecture.Interface.GetUtility<GameStoragePath>().TryGetPath(SelectedCharacterFile, out path);

	}
}
