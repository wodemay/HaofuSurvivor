using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class MapSystem : AbstractSystem, IRunUpdateable
	{
		private readonly Dictionary<Vector2Int, MapChunkView> mLoadedChunks = new();
		private readonly Dictionary<Vector2Int, MapChunkData> mGeneratedChunks = new();
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
			mGeneratedChunks.Clear();
			mPendingLoads.Clear();
			mQueuedLoads.Clear();
			mUnloadBuffer.Clear();
			mHasCenter = false;
			var model = this.GetModel<MapModel>();
			model.HasCurrentChunk = false;
			model.LoadedChunkCount = 0;
			this.GetModel<WorldMapModel>().Clear();
		}

		public bool TryRestoreWorld(int worldSeed, int themeId, int generatorVersion)
		{
			if (mConfig == null) mConfig = this.GetUtility<MapGridCatalog>().Config;
			if (mConfig == null || themeId != mConfig.ThemeId || generatorVersion != mConfig.GeneratorVersion) return false;

			this.GetModel<WorldMapModel>().Restore(worldSeed, themeId, generatorVersion);
			MapChunkFactory.Instance.ReleaseAll();
			mLoadedChunks.Clear();
			mGeneratedChunks.Clear();
			mPendingLoads.Clear();
			mQueuedLoads.Clear();
			mUnloadBuffer.Clear();
			mHasCenter = false;
			PrepareForRun();
			return true;
		}

		public void PrepareForRun()
		{
			if (mConfig == null) mConfig = this.GetUtility<MapGridCatalog>().Config;
			var player = this.GetModel<PlayerModel>();
			if (mConfig == null || !player.IsRegistered || player.IsDead) return;
			var world = this.GetModel<WorldMapModel>();
			if (!world.HasWorld) world.BeginNew(mConfig.ThemeId, mConfig.GeneratorVersion);

			var center = WorldToChunk(player.Position, mConfig.ChunkSize);
			var model = this.GetModel<MapModel>();
			model.CurrentChunk = center;
			model.HasCurrentChunk = true;
			QueueRequiredChunks(center);
			EnsureGenerationWindow(center);
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

				var view = MapChunkFactory.Instance.Spawn(mConfig, coordinate, GenerateChunkData(coordinate), WorldRootLocator.Get(WorldRootSlot.MapBackground));
				if (view != null) mLoadedChunks.Add(coordinate, view);
			}
		}

		private MapChunkData GenerateChunkData(Vector2Int coordinate)
		{
			if (!mGeneratedChunks.TryGetValue(coordinate, out var data))
			{
				EnsureGenerationWindow(coordinate);
				data = mGeneratedChunks[coordinate];
			}
			return data;
		}

		private void EnsureGenerationWindow(Vector2Int center)
		{
			var chunkSize = Mathf.Max(1, mConfig.ChunkSize);
			var origin = new Vector2Int((center.x - 1) * chunkSize, (center.y - 1) * chunkSize);
			var windowSize = chunkSize * 3;
			var working = new Dictionary<Vector2Int, MapChunkData>();
			var missing = new HashSet<Vector2Int>();
			for (var y = -1; y <= 1; y++)
			for (var x = -1; x <= 1; x++)
			{
				var coordinate = center + new Vector2Int(x, y);
				if (mGeneratedChunks.TryGetValue(coordinate, out var existing)) working.Add(coordinate, existing);
				else
				{
					working.Add(coordinate, CreateEmptyChunk(coordinate, chunkSize));
					missing.Add(coordinate);
				}
			}

			var blocked = new bool[windowSize * windowSize];
			foreach (var entry in working) MarkExistingObstacles(entry.Value, blocked, origin, windowSize);
			var candidates = new List<ObstacleCandidate>();
			foreach (var coordinate in missing) candidates.AddRange(BuildCandidates(coordinate, chunkSize));
			candidates.Sort((left, right) =>
			{
				var result = left.OrderKey.CompareTo(right.OrderKey);
				if (result != 0) return result;
				result = left.Coordinate.y.CompareTo(right.Coordinate.y);
				return result != 0 ? result : left.Coordinate.x.CompareTo(right.Coordinate.x);
			});

			foreach (var candidate in candidates)
			{
				if (!CanPlace(blocked, candidate, origin, windowSize, chunkSize)) continue;
				MarkCandidate(blocked, candidate, origin, windowSize, chunkSize, true);
				if (!IsWalkableWindow(blocked, origin, windowSize))
				{
					MarkCandidate(blocked, candidate, origin, windowSize, chunkSize, false);
					continue;
				}

				var data = working[candidate.Coordinate];
				data.Obstacles.Add(candidate.ToPlacement(data.Coordinate, data.ChunkSize, data.Obstacles.Count));
				foreach (var cell in candidate.Cells)
				{
					var localX = candidate.Anchor.x + cell.x;
					var localY = candidate.Anchor.y + cell.y;
					data.CellFlags[localY * data.ChunkSize + localX] |= MapCellFlags.BlocksMovement | MapCellFlags.BlocksProjectile;
				}
			}

			foreach (var entry in working)
				if (missing.Contains(entry.Key)) mGeneratedChunks[entry.Key] = entry.Value;
		}

		private static MapChunkData CreateEmptyChunk(Vector2Int coordinate, int chunkSize)
		{
			var data = new MapChunkData { Coordinate = coordinate, ChunkSize = chunkSize };
			for (var i = 0; i < chunkSize * chunkSize; i++) data.CellFlags.Add(MapCellFlags.Walkable);
			return data;
		}

		private List<ObstacleCandidate> BuildCandidates(Vector2Int coordinate, int chunkSize)
		{
			var candidates = new List<ObstacleCandidate>();
			var templates = mConfig.Theme == null ? null : mConfig.Theme.ObstacleTemplates;
			if (templates == null || templates.Count == 0) return candidates;

			var random = new System.Random(CombineSeed(this.GetModel<WorldMapModel>().WorldSeed, coordinate, mConfig.GeneratorVersion));
			var targetCount = Mathf.Min(8, Mathf.Max(0, chunkSize * chunkSize / 128));
			for (var attempt = 0; attempt < targetCount * 16 && candidates.Count < targetCount; attempt++)
			{
				var template = PickTemplate(templates, random);
				if (template == null || template.OccupiedCells == null || template.OccupiedCells.Count == 0) continue;
				var turns = template.AllowQuarterTurnRotation ? random.Next(0, 4) : 0;
				var mirrored = template.AllowMirror && random.NextDouble() < 0.5;
				var anchor = new Vector2Int(random.Next(0, chunkSize), random.Next(0, chunkSize));
				var cells = template.GetTransformedCells(turns, mirrored);
				var valid = true;
				foreach (var cell in cells)
				{
					var x = anchor.x + cell.x;
					var y = anchor.y + cell.y;
					if (x < 0 || y < 0 || x >= chunkSize || y >= chunkSize) valid = false;
				}
				if (valid) candidates.Add(new ObstacleCandidate(coordinate, template, anchor, turns, mirrored, cells, attempt));
			}
			return candidates;
		}

		private static MapObstacleTemplateConfig PickTemplate(List<MapObstacleTemplateConfig> templates, System.Random random)
		{
			var total = 0f;
			foreach (var template in templates) if (template != null) total += Mathf.Max(0f, template.Weight);
			if (total <= 0f) return null;
			var value = (float)random.NextDouble() * total;
			foreach (var template in templates)
			{
				if (template == null) continue;
				value -= Mathf.Max(0f, template.Weight);
				if (value <= 0f) return template;
			}
			return templates.Find(template => template != null);
		}

		private static bool CanPlace(bool[] blocked, ObstacleCandidate candidate, Vector2Int origin, int windowSize, int chunkSize)
		{
			var spacing = Mathf.Max(0, candidate.Template.MinimumSpacing);
			foreach (var cell in candidate.Cells)
			{
				var worldX = candidate.Coordinate.x * chunkSize + candidate.Anchor.x + cell.x;
				var worldY = candidate.Coordinate.y * chunkSize + candidate.Anchor.y + cell.y;
				if (Mathf.Abs(worldX) <= 1 && Mathf.Abs(worldY) <= 1) return false;
				for (var bufferY = -spacing; bufferY <= spacing; bufferY++)
				for (var bufferX = -spacing; bufferX <= spacing; bufferX++)
				{
					if (IsBlocked(blocked, origin, windowSize, new Vector2Int(worldX + bufferX, worldY + bufferY))) return false;
				}
			}
			return true;
		}

		private void MarkExistingObstacles(MapChunkData data, bool[] blocked, Vector2Int origin, int windowSize)
		{
			if (mConfig.Theme == null) return;
			foreach (var placement in data.Obstacles)
			{
				var template = mConfig.Theme.ObstacleTemplates.Find(item => item != null && item.Id == placement.TemplateId);
				if (template == null) continue;
				foreach (var cell in template.GetTransformedCells(placement.QuarterTurns, placement.IsMirrored))
					SetBlocked(blocked, origin, windowSize, new Vector2Int(placement.WorldCellX + cell.x, placement.WorldCellY + cell.y), true);
			}
		}

		private static void MarkCandidate(bool[] blocked, ObstacleCandidate candidate, Vector2Int origin, int windowSize, int chunkSize, bool value)
		{
			foreach (var cell in candidate.Cells)
				SetBlocked(blocked, origin, windowSize, new Vector2Int(candidate.Coordinate.x * chunkSize + candidate.Anchor.x + cell.x, candidate.Coordinate.y * chunkSize + candidate.Anchor.y + cell.y), value);
		}

		private static bool IsWalkableWindow(bool[] blocked, Vector2Int origin, int windowSize)
		{
			var visited = new bool[blocked.Length];
			var queue = new Queue<int>();
			for (var x = 0; x < windowSize; x++)
			{
				TryEnqueue(x, 0, blocked, visited, queue, windowSize);
				TryEnqueue(x, windowSize - 1, blocked, visited, queue, windowSize);
			}
			for (var y = 1; y < windowSize - 1; y++)
			{
				TryEnqueue(0, y, blocked, visited, queue, windowSize);
				TryEnqueue(windowSize - 1, y, blocked, visited, queue, windowSize);
			}
			while (queue.Count > 0)
			{
				var index = queue.Dequeue();
				var x = index % windowSize;
				var y = index / windowSize;
				TryEnqueue(x - 1, y, blocked, visited, queue, windowSize);
				TryEnqueue(x + 1, y, blocked, visited, queue, windowSize);
				TryEnqueue(x, y - 1, blocked, visited, queue, windowSize);
				TryEnqueue(x, y + 1, blocked, visited, queue, windowSize);
			}
			for (var index = 0; index < blocked.Length; index++)
				if (!blocked[index] && !visited[index]) return false;
			return IsSpawnReachable(blocked, visited, origin, windowSize);
		}

		private static bool IsSpawnReachable(bool[] blocked, bool[] visited, Vector2Int origin, int windowSize)
		{
			for (var y = -1; y <= 1; y++)
			for (var x = -1; x <= 1; x++)
			{
				var localX = x - origin.x;
				var localY = y - origin.y;
				if (localX < 0 || localY < 0 || localX >= windowSize || localY >= windowSize) continue;
				var index = localY * windowSize + localX;
				if (blocked[index] || !visited[index]) return false;
			}
			return true;
		}

		private static void TryEnqueue(int x, int y, bool[] blocked, bool[] visited, Queue<int> queue, int size)
		{
			if (x < 0 || y < 0 || x >= size || y >= size) return;
			var index = y * size + x;
			if (blocked[index] || visited[index]) return;
			visited[index] = true;
			queue.Enqueue(index);
		}

		private static bool IsBlocked(bool[] blocked, Vector2Int origin, int size, Vector2Int world)
		{
			var x = world.x - origin.x;
			var y = world.y - origin.y;
			return x >= 0 && y >= 0 && x < size && y < size && blocked[y * size + x];
		}

		private static void SetBlocked(bool[] blocked, Vector2Int origin, int size, Vector2Int world, bool value)
		{
			var x = world.x - origin.x;
			var y = world.y - origin.y;
			if (x < 0 || y < 0 || x >= size || y >= size) return;
			blocked[y * size + x] = value;
		}

		private sealed class ObstacleCandidate
		{
			public readonly Vector2Int Coordinate;
			public readonly MapObstacleTemplateConfig Template;
			public readonly Vector2Int Anchor;
			public readonly int QuarterTurns;
			public readonly bool IsMirrored;
			public readonly List<Vector2Int> Cells;
			public readonly int OrderKey;

			public ObstacleCandidate(Vector2Int coordinate, MapObstacleTemplateConfig template, Vector2Int anchor, int quarterTurns, bool isMirrored, List<Vector2Int> cells, int attempt)
			{
				Coordinate = coordinate;
				Template = template;
				Anchor = anchor;
				QuarterTurns = quarterTurns;
				IsMirrored = isMirrored;
				Cells = cells;
				OrderKey = CombineSeed(0, coordinate, attempt);
			}

			public MapObstaclePlacementData ToPlacement(Vector2Int coordinate, int chunkSize, int index)
			{
				return new MapObstaclePlacementData
				{
					TemplateId = Template.Id,
					WorldCellX = coordinate.x * chunkSize + Anchor.x,
					WorldCellY = coordinate.y * chunkSize + Anchor.y,
					QuarterTurns = QuarterTurns,
					IsMirrored = IsMirrored,
					StableId = $"{coordinate.x}:{coordinate.y}:{index}"
				};
			}
		}

		private static int CombineSeed(int worldSeed, Vector2Int coordinate, int version)
		{
			unchecked
			{
				var hash = worldSeed;
				hash = hash * 397 ^ coordinate.x;
				hash = hash * 397 ^ coordinate.y;
				return hash * 397 ^ version;
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
