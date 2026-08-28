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
		private readonly List<Vector2Int> mLoadOrderBuffer = new();
		private readonly HashSet<Vector2Int> mQueuedLoads = new();
		private readonly List<Vector2Int> mUnloadBuffer = new();
		private readonly HashSet<string> mDestroyedBreakables = new();
		private bool[] mWalkabilityVisited = System.Array.Empty<bool>();
		private int[] mWalkabilityQueue = System.Array.Empty<int>();
		private MapGridConfig mConfig;
		private bool mHasCenter;

		public bool HasPendingChunkOperations => mPendingLoads.Count > 0;

		public void Reset()
		{
			this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
			this.GetSystem<MapNavMeshSystem>().PrepareForMapMutation();
			this.GetSystem<BreakableObjectSystem>().Reset();
			MapChunkFactory.Instance.ReleaseAll();
			mLoadedChunks.Clear();
			mGeneratedChunks.Clear();
			mPendingLoads.Clear();
			mLoadOrderBuffer.Clear();
			mQueuedLoads.Clear();
			mUnloadBuffer.Clear();
			mDestroyedBreakables.Clear();
			mHasCenter = false;
			var model = this.GetModel<MapModel>();
			model.HasCurrentChunk = false;
			model.LoadedChunkCount = 0;
			this.GetModel<WorldMapModel>().Clear();
			this.GetSystem<MapNavMeshSystem>().MarkDirty();
		}

		public bool TryRestoreWorld(int worldSeed, int themeId, int generatorVersion)
		{
			if (mConfig == null) mConfig = this.GetUtility<MapGridCatalog>().Config;
			if (mConfig == null || themeId != mConfig.ThemeId || generatorVersion != mConfig.GeneratorVersion) return false;

			this.GetSystem<MapNavMeshSystem>().PrepareForMapMutation();
			this.GetSystem<BreakableObjectSystem>().Reset();
			this.GetModel<WorldMapModel>().Restore(worldSeed, themeId, generatorVersion);
			MapChunkFactory.Instance.ReleaseAll();
			mLoadedChunks.Clear();
			mGeneratedChunks.Clear();
			mPendingLoads.Clear();
			mLoadOrderBuffer.Clear();
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
			LoadPendingChunks(center, int.MaxValue, true);
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
			LoadPendingChunks(center, Mathf.Max(1, mConfig.MaxChunkOperationsPerTick), false);
			model.LoadedChunkCount = mLoadedChunks.Count;
		}

		private void QueueRequiredChunks(Vector2Int center)
		{
			var radius = Mathf.Max(0, mConfig.LoadRadius);
			mLoadOrderBuffer.Clear();
			for (var y = -radius; y <= radius; y++)
			for (var x = -radius; x <= radius; x++)
			{
				var coordinate = center + new Vector2Int(x, y);
				if (!mLoadedChunks.ContainsKey(coordinate) && !mQueuedLoads.Contains(coordinate)) mLoadOrderBuffer.Add(coordinate);
			}
			mLoadOrderBuffer.Sort((left, right) =>
			{
				var leftDistance = (left - center).sqrMagnitude;
				var rightDistance = (right - center).sqrMagnitude;
				if (leftDistance != rightDistance) return leftDistance.CompareTo(rightDistance);
				return left.y != right.y ? left.y.CompareTo(right.y) : left.x.CompareTo(right.x);
			});
			foreach (var coordinate in mLoadOrderBuffer)
				if (mQueuedLoads.Add(coordinate)) mPendingLoads.Enqueue(coordinate);
		}

		private void UnloadFarChunks(Vector2Int center)
		{
			var radius = Mathf.Max(mConfig.LoadRadius + 1, mConfig.UnloadRadius);
			mUnloadBuffer.Clear();
			foreach (var entry in mLoadedChunks)
				if (!IsWithin(entry.Key, center, radius)) mUnloadBuffer.Add(entry.Key);
			if (mUnloadBuffer.Count > 0) this.GetSystem<MapNavMeshSystem>().PrepareForMapMutation();

			foreach (var coordinate in mUnloadBuffer)
			{
				var view = mLoadedChunks[coordinate];
				this.GetSystem<BreakableObjectSystem>().UnloadChunk(mGeneratedChunks[coordinate]);
				mLoadedChunks.Remove(coordinate);
				MapChunkFactory.Instance.Release(view);
			}
			if (mUnloadBuffer.Count > 0) this.GetSystem<MapNavMeshSystem>().MarkDirty();
		}

		private void LoadPendingChunks(Vector2Int center, int operationLimit, bool initialOnly)
		{
			var changed = false;
			if (mPendingLoads.Count > 0) this.GetSystem<MapNavMeshSystem>().PrepareForMapMutation();
			for (var operation = 0; operation < operationLimit && mPendingLoads.Count > 0; operation++)
			{
				var coordinate = mPendingLoads.Peek();
				if (initialOnly && (coordinate - center).sqrMagnitude > mConfig.InitialLoadRadius * mConfig.InitialLoadRadius) break;
				mPendingLoads.Dequeue();
				mQueuedLoads.Remove(coordinate);
				if (!IsWithin(coordinate, center, mConfig.LoadRadius) || mLoadedChunks.ContainsKey(coordinate)) continue;

				var view = MapChunkFactory.Instance.Spawn(mConfig, coordinate, GenerateChunkData(coordinate), WorldRootLocator.Get(WorldRootSlot.MapBackground));
				if (view == null) continue;
				mLoadedChunks.Add(coordinate, view);
				this.GetSystem<BreakableObjectSystem>().LoadChunk(mGeneratedChunks[coordinate]);
				changed = true;
			}
			if (changed) this.GetSystem<MapNavMeshSystem>().MarkDirty();
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

			var occupied = new bool[windowSize * windowSize];
			var movementBlocked = new bool[windowSize * windowSize];
			foreach (var entry in working) MarkExistingObstacles(entry.Value, occupied, movementBlocked, origin, windowSize);
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
				if (!CanPlace(occupied, candidate, origin, windowSize, chunkSize)) continue;
				MarkCandidate(occupied, movementBlocked, candidate, origin, windowSize, chunkSize, true);
				if (!IsWalkableWindow(movementBlocked, windowSize))
				{
					MarkCandidate(occupied, movementBlocked, candidate, origin, windowSize, chunkSize, false);
					continue;
				}

				var data = working[candidate.Coordinate];
				data.Obstacles.Add(candidate.ToPlacement(data.Coordinate, data.ChunkSize, data.Obstacles.Count));
				foreach (var cell in candidate.Cells)
				{
					var localX = candidate.Anchor.x + cell.x;
					var localY = candidate.Anchor.y + cell.y;
					var flags = MapCellFlags.None;
					if (candidate.Template.BlocksMovement) flags |= MapCellFlags.BlocksMovement;
					if (candidate.Template.BlocksProjectile) flags |= MapCellFlags.BlocksProjectile;
					var cellIndex = localY * data.ChunkSize + localX;
					data.CellFlags[cellIndex] |= flags;
					if (candidate.Template.BlocksMovement) data.CellFlags[cellIndex] &= ~MapCellFlags.Walkable;
				}
			}

			var breakableCandidates = new List<BreakableCandidate>();
			foreach (var coordinate in missing) breakableCandidates.AddRange(BuildBreakableCandidates(coordinate, chunkSize));
			breakableCandidates.Sort((left, right) =>
			{
				var result = left.OrderKey.CompareTo(right.OrderKey);
				if (result != 0) return result;
				result = left.Coordinate.y.CompareTo(right.Coordinate.y);
				return result != 0 ? result : left.Coordinate.x.CompareTo(right.Coordinate.x);
			});
			foreach (var candidate in breakableCandidates)
			{
				if (!CanPlaceBreakable(occupied, candidate, origin, windowSize, chunkSize)) continue;
				MarkBreakable(occupied, candidate, origin, windowSize, chunkSize, true);
				var data = working[candidate.Coordinate];
				data.Breakables.Add(new MapBreakableEntityData
				{
					StableId = $"b:{candidate.Coordinate.x}:{candidate.Coordinate.y}:{data.Breakables.Count}",
					ConfigId = candidate.Config.Id,
					WorldCellX = candidate.Coordinate.x * chunkSize + candidate.Anchor.x,
					WorldCellY = candidate.Coordinate.y * chunkSize + candidate.Anchor.y,
					IsDestroyed = mDestroyedBreakables.Contains($"b:{candidate.Coordinate.x}:{candidate.Coordinate.y}:{data.Breakables.Count}")
				});
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

		private void MarkExistingObstacles(MapChunkData data, bool[] occupied, bool[] movementBlocked, Vector2Int origin, int windowSize)
		{
			if (mConfig.Theme == null) return;
			foreach (var placement in data.Obstacles)
			{
				var template = mConfig.Theme.ObstacleTemplates.Find(item => item != null && item.Id == placement.TemplateId);
				if (template == null) continue;
				foreach (var cell in template.GetTransformedCells(placement.QuarterTurns, placement.IsMirrored))
				{
					var world = new Vector2Int(placement.WorldCellX + cell.x, placement.WorldCellY + cell.y);
					SetBlocked(occupied, origin, windowSize, world, true);
					if (template.BlocksMovement) SetBlocked(movementBlocked, origin, windowSize, world, true);
				}
			}
		}

		private List<BreakableCandidate> BuildBreakableCandidates(Vector2Int coordinate, int chunkSize)
		{
			var candidates = new List<BreakableCandidate>();
			var configs = mConfig.Theme == null ? null : mConfig.Theme.BreakableObjects;
			if (configs == null || configs.Count == 0) return candidates;
			var random = new System.Random(CombineSeed(this.GetModel<WorldMapModel>().WorldSeed ^ 7919, coordinate, mConfig.GeneratorVersion));
			var targetCount = 0;
			foreach (var config in configs) if (config != null) targetCount += Mathf.Max(0, config.SpawnCountPerChunk);
			for (var attempt = 0; attempt < targetCount * 16 && candidates.Count < targetCount; attempt++)
			{
				var config = PickBreakable(configs, random);
				if (config == null || config.Prefab == null) continue;
				var anchor = new Vector2Int(random.Next(0, chunkSize), random.Next(0, chunkSize));
				candidates.Add(new BreakableCandidate(coordinate, config, anchor, attempt));
			}
			return candidates;
		}

		private static BreakableObjectConfig PickBreakable(List<BreakableObjectConfig> configs, System.Random random)
		{
			var total = 0f;
			foreach (var config in configs) if (config != null) total += Mathf.Max(0f, config.Weight);
			if (total <= 0f) return null;
			var value = (float)random.NextDouble() * total;
			foreach (var config in configs)
			{
				if (config == null) continue;
				value -= Mathf.Max(0f, config.Weight);
				if (value <= 0f) return config;
			}
			return configs.Find(config => config != null);
		}

		private static bool CanPlaceBreakable(bool[] occupied, BreakableCandidate candidate, Vector2Int origin, int windowSize, int chunkSize)
		{
			var world = new Vector2Int(candidate.Coordinate.x * chunkSize + candidate.Anchor.x, candidate.Coordinate.y * chunkSize + candidate.Anchor.y);
			if (Mathf.Abs(world.x) <= 1 && Mathf.Abs(world.y) <= 1) return false;
			var spacing = Mathf.Max(0, candidate.Config.MinimumSpacing);
			for (var y = -spacing; y <= spacing; y++)
			for (var x = -spacing; x <= spacing; x++)
				if (IsBlocked(occupied, origin, windowSize, world + new Vector2Int(x, y))) return false;
			return true;
		}

		private static void MarkBreakable(bool[] occupied, BreakableCandidate candidate, Vector2Int origin, int windowSize, int chunkSize, bool value)
		{
			var world = new Vector2Int(candidate.Coordinate.x * chunkSize + candidate.Anchor.x, candidate.Coordinate.y * chunkSize + candidate.Anchor.y);
			SetBlocked(occupied, origin, windowSize, world, value);
		}

		public bool TryGetCellFlags(Vector2Int worldCell, out MapCellFlags flags)
		{
			flags = MapCellFlags.None;
			if (mConfig == null) mConfig = this.GetUtility<MapGridCatalog>().Config;
			if (mConfig == null) return false;
			var chunkSize = Mathf.Max(1, mConfig.ChunkSize);
			var chunk = new Vector2Int(FloorDivide(worldCell.x, chunkSize), FloorDivide(worldCell.y, chunkSize));
			var data = GenerateChunkData(chunk);
			var localX = PositiveModulo(worldCell.x, chunkSize);
			var localY = PositiveModulo(worldCell.y, chunkSize);
			var index = localY * chunkSize + localX;
			if (index < 0 || index >= data.CellFlags.Count) return false;
			flags = data.CellFlags[index];
			return true;
		}

		public void MarkBreakableDestroyed(string stableId)
		{
			if (string.IsNullOrEmpty(stableId)) return;
			mDestroyedBreakables.Add(stableId);
			foreach (var data in mGeneratedChunks.Values)
				foreach (var entry in data.Breakables)
					if (entry != null && entry.StableId == stableId)
					{
						entry.IsDestroyed = true;
						return;
					}
		}

		public IEnumerable<BreakableSaveData> GetBreakableSaveData()
		{
			foreach (var stableId in mDestroyedBreakables)
				yield return new BreakableSaveData { StableId = stableId, IsDestroyed = true };
		}

		public void RestoreBreakables(IEnumerable<BreakableSaveData> entries)
		{
			mDestroyedBreakables.Clear();
			if (entries != null)
				foreach (var entry in entries)
					if (entry != null && entry.IsDestroyed && !string.IsNullOrEmpty(entry.StableId)) mDestroyedBreakables.Add(entry.StableId);
			foreach (var data in mGeneratedChunks.Values)
				foreach (var entry in data.Breakables)
					if (entry != null && mDestroyedBreakables.Contains(entry.StableId)) entry.IsDestroyed = true;
			this.GetSystem<BreakableObjectSystem>().Reset();
			foreach (var coordinate in mLoadedChunks.Keys)
				if (mGeneratedChunks.TryGetValue(coordinate, out var loadedData)) this.GetSystem<BreakableObjectSystem>().LoadChunk(loadedData);
		}

		private static void MarkCandidate(bool[] occupied, bool[] movementBlocked, ObstacleCandidate candidate, Vector2Int origin, int windowSize, int chunkSize, bool value)
		{
			foreach (var cell in candidate.Cells)
			{
				var world = new Vector2Int(candidate.Coordinate.x * chunkSize + candidate.Anchor.x + cell.x, candidate.Coordinate.y * chunkSize + candidate.Anchor.y + cell.y);
				SetBlocked(occupied, origin, windowSize, world, value);
				if (candidate.Template.BlocksMovement) SetBlocked(movementBlocked, origin, windowSize, world, value);
			}
		}

		private bool IsWalkableWindow(bool[] blocked, int windowSize)
		{
			if (mWalkabilityVisited.Length < blocked.Length)
			{
				mWalkabilityVisited = new bool[blocked.Length];
				mWalkabilityQueue = new int[blocked.Length];
			}
			System.Array.Clear(mWalkabilityVisited, 0, blocked.Length);
			var head = 0;
			var tail = 0;
			for (var index = 0; index < blocked.Length; index++)
				if (!blocked[index]) { mWalkabilityVisited[index] = true; mWalkabilityQueue[tail++] = index; break; }
			if (tail == 0) return false;
			while (head < tail)
			{
				var index = mWalkabilityQueue[head++];
				var x = index % windowSize;
				var y = index / windowSize;
				TryEnqueue(x - 1, y, blocked, mWalkabilityVisited, mWalkabilityQueue, ref tail, windowSize);
				TryEnqueue(x + 1, y, blocked, mWalkabilityVisited, mWalkabilityQueue, ref tail, windowSize);
				TryEnqueue(x, y - 1, blocked, mWalkabilityVisited, mWalkabilityQueue, ref tail, windowSize);
				TryEnqueue(x, y + 1, blocked, mWalkabilityVisited, mWalkabilityQueue, ref tail, windowSize);
			}
			for (var index = 0; index < blocked.Length; index++)
				if (!blocked[index] && !mWalkabilityVisited[index]) return false;
			return true;
		}

		private static void TryEnqueue(int x, int y, bool[] blocked, bool[] visited, int[] queue, ref int tail, int size)
		{
			if (x < 0 || y < 0 || x >= size || y >= size) return;
			var index = y * size + x;
			if (blocked[index] || visited[index]) return;
			visited[index] = true;
			queue[tail++] = index;
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

		private sealed class BreakableCandidate
		{
			public readonly Vector2Int Coordinate;
			public readonly BreakableObjectConfig Config;
			public readonly Vector2Int Anchor;
			public readonly int OrderKey;

			public BreakableCandidate(Vector2Int coordinate, BreakableObjectConfig config, Vector2Int anchor, int attempt)
			{
				Coordinate = coordinate;
				Config = config;
				Anchor = anchor;
				OrderKey = CombineSeed(17389, coordinate, attempt);
			}
		}

		private static int FloorDivide(int value, int divisor)
		{
			return Mathf.FloorToInt((float)value / Mathf.Max(1, divisor));
		}

		private static int PositiveModulo(int value, int divisor)
		{
			var size = Mathf.Max(1, divisor);
			var result = value % size;
			return result < 0 ? result + size : result;
		}

		protected override void OnInit()
		{
			mConfig = this.GetUtility<MapGridCatalog>().Config;
			this.RegisterEvent<RunEndedEvent>(_ => Reset());
		}
	}
}
