using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Map/Breakable Object Config")]
	public class BreakableObjectConfig : ScriptableObject
	{
		[Min(1)] public int Id = 1;
		public GameObject Prefab;
		[Min(1)] public int HitPoints = 3;
		[Min(0f)] public float Weight = 1f;
		[Min(0)] public int SpawnCountPerChunk = 2;
		[Min(0)] public int MinimumSpacing = 3;
		[Min(0)] public int DropTableId = 1;
	}

	public class BreakableObjectCatalog : QFramework.IUtility
	{
		public IReadOnlyList<BreakableObjectConfig> Configs { get; }

		public BreakableObjectCatalog()
		{
			Configs = Resources.LoadAll<BreakableObjectConfig>("Configs/Map/Breakables");
		}

		public BreakableObjectConfig Get(int id)
		{
			foreach (var config in Configs)
				if (config != null && config.Id == id) return config;
			return null;
		}
	}
}
