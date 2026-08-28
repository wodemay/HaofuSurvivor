using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Map/Theme Config")]
	public class MapThemeConfig : ScriptableObject
	{
		[Min(1)] public int Id = 1;
		public TileBase GroundTile;
		public List<TileBase> DecorationTiles = new();
		public List<MapObstacleTemplateConfig> ObstacleTemplates = new();
		public List<BreakableObjectConfig> BreakableObjects = new();
		public List<MapEventConfig> MapEvents = new();
		[Min(1f)] public float MapEventSpawnIntervalSeconds = 60f;
		[Min(0f)] public float MapEventMinimumPlayerDistance = 12f;
	}
}
