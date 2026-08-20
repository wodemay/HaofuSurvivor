using UnityEngine;
using UnityEngine.Tilemaps;

namespace HaoFuSurvivor
{
	public class MapChunkView : MonoBehaviour
	{
		private Tilemap mGroundTilemap;
		private int mChunkSize;

		public void Configure(Vector2Int coordinate, MapGridConfig config)
		{
			mChunkSize = Mathf.Max(1, config.ChunkSize);
			mGroundTilemap = GetComponentInChildren<Tilemap>();
			if (mGroundTilemap == null) return;

			transform.SetPositionAndRotation(
				new Vector3(coordinate.x * mChunkSize, coordinate.y * mChunkSize, 0f),
				Quaternion.identity);
			gameObject.name = $"MapChunk_{coordinate.x}_{coordinate.y}";
			Fill(config.GroundTile);
		}

		public void ResetView()
		{
			if (mGroundTilemap != null) mGroundTilemap.ClearAllTiles();
			mChunkSize = 0;
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
