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

		public List<Vector2Int> GetTransformedCells(int quarterTurns, bool mirrored)
		{
			var result = new List<Vector2Int>();
			foreach (var source in OccupiedCells)
			{
				var cell = mirrored ? new Vector2Int(-source.x, source.y) : source;
				for (var turn = 0; turn < ((quarterTurns % 4) + 4) % 4; turn++)
					cell = new Vector2Int(-cell.y, cell.x);
				result.Add(cell);
			}
			return result;
		}
	}
}
