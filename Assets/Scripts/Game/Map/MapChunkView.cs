using UnityEngine;
using UnityEngine.Tilemaps;

namespace HaoFuSurvivor
{
	public class MapChunkView : MonoBehaviour
	{
		private Tilemap mGroundTilemap;
		private Tilemap mObstacleTilemap;
		private int mChunkSize;

		public void Configure(Vector2Int coordinate, MapGridConfig config, MapChunkData data)
		{
			mChunkSize = Mathf.Max(1, config.ChunkSize);
			mGroundTilemap = transform.Find("GroundTilemap")?.GetComponent<Tilemap>();
			mObstacleTilemap = transform.Find("ObstacleTilemap")?.GetComponent<Tilemap>();
			mGroundTilemap ??= GetComponentInChildren<Tilemap>();
			if (mGroundTilemap == null) return;

			transform.SetPositionAndRotation(
				new Vector3(coordinate.x * mChunkSize, coordinate.y * mChunkSize, 0f),
				Quaternion.identity);
			gameObject.name = $"MapChunk_{coordinate.x}_{coordinate.y}";
			Fill(config.GetGroundTile());
			FillObstacles(config, data);
		}

		public void ResetView()
		{
			if (mGroundTilemap != null) mGroundTilemap.ClearAllTiles();
			if (mObstacleTilemap != null) mObstacleTilemap.ClearAllTiles();
			mChunkSize = 0;
		}

		private void FillObstacles(MapGridConfig config, MapChunkData data)
		{
			if (mObstacleTilemap == null || data == null || config.Theme == null) return;
			mObstacleTilemap.ClearAllTiles();
			foreach (var placement in data.Obstacles)
			{
				var template = config.Theme.ObstacleTemplates.Find(item => item != null && item.Id == placement.TemplateId);
				if (template == null || template.VisualTile == null) continue;
				foreach (var cell in template.GetTransformedCells(placement.QuarterTurns, placement.IsMirrored))
				{
					var local = new Vector3Int(
						placement.WorldCellX - data.Coordinate.x * mChunkSize + cell.x,
						placement.WorldCellY - data.Coordinate.y * mChunkSize + cell.y,
						0);
					mObstacleTilemap.SetTile(local, template.VisualTile);
				}
			}
		}

		private void Fill(TileBase groundTile)
		{
			mGroundTilemap.ClearAllTiles();
			if (groundTile == null || mChunkSize <= 0) return;

			var tiles = new TileBase[mChunkSize * mChunkSize];
			for (var index = 0; index < tiles.Length; index++) tiles[index] = groundTile;
			mGroundTilemap.SetTilesBlock(
				new BoundsInt(0, 0, 0, mChunkSize, mChunkSize, 1),
				tiles);
		}
	}
}
