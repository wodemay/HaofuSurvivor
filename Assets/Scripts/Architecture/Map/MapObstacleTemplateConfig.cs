using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Map/Obstacle Template Config")]
	public class MapObstacleTemplateConfig : ScriptableObject
	{
		[Min(1)] public int Id = 1;
		public TileBase VisualTile;
		public List<Vector2Int> OccupiedCells = new();
		[Min(0f)] public float Weight = 1f;
		[Min(0)] public int MinimumSpacing;
		public bool AllowQuarterTurnRotation = true;
		public bool AllowMirror;
		public bool BlocksMovement = true;
		public bool BlocksProjectile = true;
		[System.NonSerialized] private Dictionary<int, List<Vector2Int>> mTransformedCellCache;

		public List<Vector2Int> GetTransformedCells(int quarterTurns, bool mirrored)
		{
			mTransformedCellCache ??= new Dictionary<int, List<Vector2Int>>();
			var normalizedTurns = ((quarterTurns % 4) + 4) % 4;
			var key = normalizedTurns | (mirrored ? 4 : 0);
			if (mTransformedCellCache.TryGetValue(key, out var cached)) return cached;
			var result = new List<Vector2Int>();
			foreach (var source in OccupiedCells)
			{
				var cell = mirrored ? new Vector2Int(-source.x, source.y) : source;
				for (var turn = 0; turn < normalizedTurns; turn++)
					cell = new Vector2Int(-cell.y, cell.x);
				result.Add(cell);
			}
			mTransformedCellCache.Add(key, result);
			return result;
		}
	}
}
