using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	public enum PickupType
	{
		Coin,
		Health,
		ExperienceAbsorb
	}

	[System.Serializable]
	public class DropEntry
	{
		[Min(1)] public int Id = 1;
		public PickupType Type;
		public GameObject Prefab;
		[Min(0f)] public float Weight = 1f;
		public string Amount = "1";
	}

	[CreateAssetMenu(menuName = "ProjectSurvivor/Progression/Drop Table Config")]
	public class DropTableConfig : ScriptableObject
	{
		[Min(1)] public int Id = 1;
		public List<DropEntry> Entries = new();

		public DropEntry Get(int id)
		{
			return Entries.Find(entry => entry != null && entry.Id == id);
		}
	}

	public class DropTableCatalog : QFramework.IUtility
	{
		private readonly IReadOnlyList<DropTableConfig> mConfigs;

		public DropTableCatalog()
		{
			mConfigs = Resources.LoadAll<DropTableConfig>("Configs/Progression/Drops");
		}

		public DropTableConfig Get(int id)
		{
			foreach (var config in mConfigs)
				if (config != null && config.Id == id) return config;
			return null;
		}
	}
}
