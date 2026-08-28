using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	public enum MapEventType
	{
		RegionKill
	}

	[CreateAssetMenu(menuName = "ProjectSurvivor/Map/Event Config")]
	public class MapEventConfig : ScriptableObject
	{
		[Min(1)] public int Id = 1;
		public MapEventType Type = MapEventType.RegionKill;
		[Min(0.5f)] public float TriggerRadius = 5f;
		[Min(0.1f)] public float HoldDurationSeconds = 5f;
		[Min(0)] public int TemporaryRewardDropTableId = 1;
		public GameObject EventPrefab;
		[Min(0)] public int MinimumSpacing = 8;
		public GameObject GuidePrefab;
		public Sprite GuideIcon;
		[Min(0.1f)] public float GuideMinScale = 0.75f;
		[Min(0.1f)] public float GuideMaxScale = 1.25f;
		[Min(0)] public float GuideScaleFalloff = 0.01f;
	}

	public class MapEventCatalog : QFramework.IUtility
	{
		public IReadOnlyList<MapEventConfig> Configs { get; }

		public MapEventCatalog()
		{
			Configs = Resources.LoadAll<MapEventConfig>("Configs/Map/Events");
		}

		public MapEventConfig Get(int id)
		{
			foreach (var config in Configs)
				if (config != null && config.Id == id) return config;
			return null;
		}
	}
}
