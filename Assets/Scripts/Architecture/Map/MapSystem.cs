using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class MapSystem : AbstractSystem, IRunUpdateable
	{
		private readonly Dictionary<Vector2Int, MapChunkView> mLoadedChunks = new();
		private readonly Queue<Vector2Int> mPendingLoads = new();
		private readonly HashSet<Vector2Int> mQueuedLoads = new();
		private readonly List<Vector2Int> mUnloadBuffer = new();
		private MapGridConfig mConfig;
		private bool mHasCenter;

		public void Reset()
		{
			this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
			MapChunkFactory.Instance.ReleaseAll();
			mLoadedChunks.Clear();
			mPendingLoads.Clear();
			mQueuedLoads.Clear();
			mUnloadBuffer.Clear();
			mHasCenter = false;
			var model = this.GetModel<MapModel>();
			model.HasCurrentChunk = false;
			model.LoadedChunkCount = 0;
		}

		public void PrepareForRun()
		{
			if (mConfig == null) mConfig = this.GetUtility<MapGridCatalog>().Config;
			var player = this.GetModel<PlayerModel>();
			if (mConfig == null || !player.IsRegistered || player.IsDead) return;

			var center = WorldToChunk(player.Position, mConfig.ChunkSize);
			var model = this.GetModel<MapModel>();
			model.CurrentChunk = center;
			model.HasCurrentChunk = true;
			QueueRequiredChunks(center);
			LoadPendingChunks(center, int.MaxValue);
			mHasCenter = true;
			model.LoadedChunkCount = mLoadedChunks.Count;
		}

		public void OnRunUpdate(float deltaTime)
		{
			if (mConfig == null) mConfig = this.GetUtility<MapGridCatalog>().Config;
			var player = this.GetModel<PlayerModel>();
			if (mConfig == null || !player.IsRegistered || player.IsDead) return;

			var center = WorldToChunk(player.Position, mConfig.ChunkSize);
			var model = this.GetModel<MapModel>();
			var centerChanged = !mHasCenter || center != model.CurrentChunk;
			model.CurrentChunk = center;
			model.HasCurrentChunk = true;
			if (centerChanged) QueueRequiredChunks(center);
			mHasCenter = true;
			UnloadFarChunks(center);
			LoadPendingChunks(center, Mathf.Max(1, mConfig.MaxChunkOperationsPerTick));
			model.LoadedChunkCount = mLoadedChunks.Count;
		}

		private void QueueRequiredChunks(Vector2Int center)
		{
			var radius = Mathf.Max(0, mConfig.LoadRadius);
			for (var y = -radius; y <= radius; y++)
			for (var x = -radius; x <= radius; x++)
			{
				var coordinate = center + new Vector2Int(x, y);
				if (!mLoadedChunks.ContainsKey(coordinate) && mQueuedLoads.Add(coordinate)) mPendingLoads.Enqueue(coordinate);
			}
		}

		private void UnloadFarChunks(Vector2Int center)
		{
			var radius = Mathf.Max(mConfig.LoadRadius + 1, mConfig.UnloadRadius);
			mUnloadBuffer.Clear();
			foreach (var entry in mLoadedChunks)
				if (!IsWithin(entry.Key, center, radius)) mUnloadBuffer.Add(entry.Key);

			foreach (var coordinate in mUnloadBuffer)
			{
				var view = mLoadedChunks[coordinate];
				mLoadedChunks.Remove(coordinate);
				MapChunkFactory.Instance.Release(view);
			}
		}

		private void LoadPendingChunks(Vector2Int center, int operationLimit)
		{
			for (var operation = 0; operation < operationLimit && mPendingLoads.Count > 0; operation++)
			{
				var coordinate = mPendingLoads.Dequeue();
				mQueuedLoads.Remove(coordinate);
				if (!IsWithin(coordinate, center, mConfig.LoadRadius) || mLoadedChunks.ContainsKey(coordinate)) continue;

				var view = MapChunkFactory.Instance.Spawn(mConfig, coordinate, WorldRootLocator.Get(WorldRootSlot.MapBackground));
				if (view != null) mLoadedChunks.Add(coordinate, view);
			}
		}

		private static bool IsWithin(Vector2Int value, Vector2Int center, int radius)
		{
			return Mathf.Abs(value.x - center.x) <= radius && Mathf.Abs(value.y - center.y) <= radius;
		}

		private static Vector2Int WorldToChunk(Vector2 position, int chunkSize)
		{
			var size = Mathf.Max(1, chunkSize);
			return new Vector2Int(Mathf.FloorToInt(position.x / size), Mathf.FloorToInt(position.y / size));
		}

		protected override void OnInit()
		{
			mConfig = this.GetUtility<MapGridCatalog>().Config;
			this.RegisterEvent<RunEndedEvent>(_ => Reset());
		}
	}
}
