using NavMeshPlus.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace HaoFuSurvivor
{
	public class MapChunkView : MonoBehaviour
	{
		private Tilemap mGroundTilemap;
		private Tilemap mDecorationTilemap;
		private Tilemap mMoveBlockerTilemap;
		private Tilemap mProjectileBlockerTilemap;
		private int mChunkSize;

		public void Configure(Vector2Int coordinate, MapGridConfig config, MapChunkData data)
		{
			mChunkSize = Mathf.Max(1, config.ChunkSize);
			mGroundTilemap = transform.Find("GroundTilemap")?.GetComponent<Tilemap>();
			mDecorationTilemap = transform.Find("DecorationTilemap")?.GetComponent<Tilemap>();
			mMoveBlockerTilemap = transform.Find("MoveBlockerTilemap")?.GetComponent<Tilemap>();
			mProjectileBlockerTilemap = transform.Find("ProjectileBlockerTilemap")?.GetComponent<Tilemap>();
			if (mGroundTilemap == null) return;
			ConfigureNavigationSources();
			SetRendererEnabled(mMoveBlockerTilemap, false);
			SetRendererEnabled(mProjectileBlockerTilemap, false);
			SetCollisionEnabled(mMoveBlockerTilemap, false);
			SetCollisionEnabled(mProjectileBlockerTilemap, false);

			transform.SetPositionAndRotation(
				new Vector3(coordinate.x * mChunkSize, coordinate.y * mChunkSize, 0f),
				Quaternion.identity);
			gameObject.name = $"MapChunk_{coordinate.x}_{coordinate.y}";
			Fill(config.GetGroundTile());
			FillObstacles(config, data);
			SetCollisionEnabled(mMoveBlockerTilemap, true);
			SetCollisionEnabled(mProjectileBlockerTilemap, true);
		}

		public void ResetView()
		{
			SetCollisionEnabled(mMoveBlockerTilemap, false);
			SetCollisionEnabled(mProjectileBlockerTilemap, false);
			if (mGroundTilemap != null) mGroundTilemap.ClearAllTiles();
			if (mDecorationTilemap != null) mDecorationTilemap.ClearAllTiles();
			if (mMoveBlockerTilemap != null) mMoveBlockerTilemap.ClearAllTiles();
			if (mProjectileBlockerTilemap != null) mProjectileBlockerTilemap.ClearAllTiles();
			mChunkSize = 0;
		}

		private void ConfigureNavigationSources()
		{
			ConfigureNavigationSource(mGroundTilemap, false, 0);
			ConfigureNavigationSource(mMoveBlockerTilemap, true, 1);
		}

		private static void ConfigureNavigationSource(Tilemap tilemap, bool overrideArea, int area)
		{
			if (tilemap == null) return;
			var modifier = tilemap.GetComponent<NavMeshModifier>();
			if (modifier == null) modifier = tilemap.gameObject.AddComponent<NavMeshModifier>();
			modifier.overrideArea = overrideArea;
			modifier.area = area;
			modifier.ignoreFromBuild = false;
		}

		private void FillObstacles(MapGridConfig config, MapChunkData data)
		{
			if (data == null || config.Theme == null) return;
			mDecorationTilemap?.ClearAllTiles();
			mMoveBlockerTilemap?.ClearAllTiles();
			mProjectileBlockerTilemap?.ClearAllTiles();
			var decorationPositions = new List<Vector3Int>();
			var decorationTiles = new List<TileBase>();
			var moveBlockerPositions = new List<Vector3Int>();
			var moveBlockerTiles = new List<TileBase>();
			var projectileBlockerPositions = new List<Vector3Int>();
			var projectileBlockerTiles = new List<TileBase>();
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
					if (mDecorationTilemap != null) { decorationPositions.Add(local); decorationTiles.Add(template.VisualTile); }
					if (template.BlocksMovement && mMoveBlockerTilemap != null) { moveBlockerPositions.Add(local); moveBlockerTiles.Add(template.VisualTile); }
					if (template.BlocksProjectile && mProjectileBlockerTilemap != null) { projectileBlockerPositions.Add(local); projectileBlockerTiles.Add(template.VisualTile); }
				}
			}
			if (mDecorationTilemap != null && decorationPositions.Count > 0) mDecorationTilemap.SetTiles(decorationPositions.ToArray(), decorationTiles.ToArray());
			if (mMoveBlockerTilemap != null && moveBlockerPositions.Count > 0) mMoveBlockerTilemap.SetTiles(moveBlockerPositions.ToArray(), moveBlockerTiles.ToArray());
			if (mProjectileBlockerTilemap != null && projectileBlockerPositions.Count > 0) mProjectileBlockerTilemap.SetTiles(projectileBlockerPositions.ToArray(), projectileBlockerTiles.ToArray());
		}

		private static void SetCollisionEnabled(Tilemap tilemap, bool enabled)
		{
			if (tilemap == null) return;
			var tilemapCollider = tilemap.GetComponent<TilemapCollider2D>();
			var compositeCollider = tilemap.GetComponent<CompositeCollider2D>();
			if (!enabled)
			{
				if (compositeCollider != null) compositeCollider.enabled = false;
				if (tilemapCollider != null) tilemapCollider.enabled = false;
				return;
			}
			if (tilemapCollider != null) tilemapCollider.enabled = true;
			if (compositeCollider != null) compositeCollider.enabled = true;
		}

		private static void SetRendererEnabled(Tilemap tilemap, bool enabled)
		{
			if (tilemap == null) return;
			var renderer = tilemap.GetComponent<TilemapRenderer>();
			if (renderer != null) renderer.enabled = enabled;
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
