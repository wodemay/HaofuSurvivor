using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class RunSaveStorage : IUtility
	{
		private const string SaveKey = "HaoFuSurvivor.ActiveRun";

		public bool HasSave() => PlayerPrefs.HasKey(SaveKey);

		public void Save(RunSaveData data)
		{
			if (data == null) return;
			PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
			PlayerPrefs.Save();
		}

		public RunSaveData Load()
		{
			if (!HasSave()) return null;
			try
			{
				return JsonUtility.FromJson<RunSaveData>(PlayerPrefs.GetString(SaveKey));
			}
			catch
			{
				Clear();
				return null;
			}
		}

		public void Clear()
		{
			PlayerPrefs.DeleteKey(SaveKey);
			PlayerPrefs.Save();
		}
	}
}
