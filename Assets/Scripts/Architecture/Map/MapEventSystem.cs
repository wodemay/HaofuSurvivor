using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public readonly struct MapEventState
	{
		public readonly string StableId;
		public readonly int ConfigId;
		public readonly MapEventType Type;
		public readonly Vector2 Position;
		public readonly float TriggerRadius;
		public readonly float HoldElapsed;
		public readonly float HoldDuration;
		public readonly bool IsActive;
		public readonly bool IsCompleted;

		public MapEventState(string stableId, int configId, MapEventType type, Vector2 position, float triggerRadius,
			float holdElapsed, float holdDuration, bool isActive, bool isCompleted)
		{
			StableId = stableId;
			ConfigId = configId;
			Type = type;
			Position = position;
			TriggerRadius = triggerRadius;
			HoldElapsed = holdElapsed;
			HoldDuration = holdDuration;
			IsActive = isActive;
			IsCompleted = isCompleted;
		}
	}

	public class GetMapEventsQuery : AbstractQuery<IReadOnlyList<MapEventState>>
	{
		protected override IReadOnlyList<MapEventState> OnDo() => GameArchitecture.Interface.GetSystem<MapEventSystem>().GetStates();
	}

	public class MapEventSystem : AbstractSystem, IRunUpdateable
	{
		private readonly Dictionary<string, RuntimeMapEvent> mEvents = new();
		private readonly List<MapEventState> mStateBuffer = new();
		private readonly List<MapEventConfig> mConfigBuffer = new();
		private readonly List<string> mCompletedBuffer = new();
		private MapGridConfig mMapConfig;
		private float mSpawnElapsed;
		private int mSpawnIndex;

		public void Reset()
		{
			mEvents.Clear();
			mStateBuffer.Clear();
			mCompletedBuffer.Clear();
			mSpawnElapsed = 0f;
			mSpawnIndex = 0;
		}

		public float GetSpawnElapsed() => mSpawnElapsed;
		public int GetSpawnIndex() => mSpawnIndex;

		public IReadOnlyList<MapEventState> GetStates()
		{
			mStateBuffer.Clear();
			foreach (var runtime in mEvents.Values) mStateBuffer.Add(CreateState(runtime));
			return mStateBuffer;
		}

		public IEnumerable<MapEventSaveData> GetSaveData()
		{
			foreach (var runtime in mEvents.Values)
				yield return new MapEventSaveData
				{
					StableId = runtime.StableId,
					ConfigId = runtime.Config.Id,
					PositionX = runtime.Position.x,
					PositionY = runtime.Position.y,
					HoldElapsed = runtime.HoldElapsed,
					IsCompleted = runtime.IsCompleted
				};
		}

		public void Restore(IEnumerable<MapEventSaveData> entries, float spawnElapsed, int spawnIndex)
		{
			Reset();
			mSpawnElapsed = Mathf.Max(0f, spawnElapsed);
			mSpawnIndex = Mathf.Max(0, spawnIndex);
			if (entries == null) return;
			var catalog = this.GetUtility<MapEventCatalog>();
			foreach (var entry in entries)
			{
				var config = entry == null ? null : catalog.Get(entry.ConfigId);
				if (config == null || string.IsNullOrEmpty(entry.StableId) || entry.IsCompleted) continue;
				mEvents[entry.StableId] = new RuntimeMapEvent(entry.StableId, config,
					new Vector2(entry.PositionX, entry.PositionY), entry.HoldElapsed);
				CreateTrigger(mEvents[entry.StableId]);
			}
		}

		public void OnRunUpdate(float deltaTime)
		{
			var player = this.GetModel<PlayerModel>();
			if (!player.IsRegistered || player.IsDead) return;
			if (mMapConfig == null) mMapConfig = this.GetUtility<MapGridCatalog>().Config;
			if (mMapConfig == null || mMapConfig.Theme == null) return;
			if (this.GetSystem<EnemySystem>().HasActiveBoss())
			{
				RemoveIncompleteEvents();
				return;
			}
			mSpawnElapsed += deltaTime;
			var interval = Mathf.Max(1f, mMapConfig.Theme.MapEventSpawnIntervalSeconds);
			if (mSpawnElapsed >= interval)
			{
				mSpawnElapsed -= interval;
				TrySpawnEvent(player.Position);
			}
			mCompletedBuffer.Clear();
			foreach (var runtime in mEvents.Values)
			{
				var active = Vector2.Distance(player.Position, runtime.Position) <= runtime.Config.TriggerRadius;
				if (runtime.IsActive != active)
				{
					runtime.IsActive = active;
					if (!active) runtime.HoldElapsed = 0f;
					runtime.Entity?.SetProgress(runtime.HoldElapsed / Mathf.Max(0.1f, runtime.Config.HoldDurationSeconds));
					this.SendEvent(new MapEventChangedEvent(CreateState(runtime)));
				}
				if (!active) continue;
				runtime.HoldElapsed += deltaTime;
				runtime.Entity?.SetProgress(runtime.HoldElapsed / Mathf.Max(0.1f, runtime.Config.HoldDurationSeconds));
				if (runtime.HoldElapsed >= runtime.Config.HoldDurationSeconds) mCompletedBuffer.Add(runtime.StableId);
			}
			CompleteEvents(mCompletedBuffer);
		}

		public void SetPlayerPresence(string stableId, bool isInside)
		{
			if (!mEvents.TryGetValue(stableId, out var runtime)) return;
			runtime.IsActive = isInside;
			if (!isInside) runtime.HoldElapsed = 0f;
			runtime.Entity?.SetProgress(runtime.HoldElapsed / Mathf.Max(0.1f, runtime.Config.HoldDurationSeconds));
			this.SendEvent(new MapEventChangedEvent(CreateState(runtime)));
		}

		private bool TrySpawnEvent(Vector2 playerPosition)
		{
			mConfigBuffer.Clear();
			foreach (var config in mMapConfig.Theme.MapEvents)
				if (config != null && config.HoldDurationSeconds > 0f) mConfigBuffer.Add(config);
			if (mConfigBuffer.Count == 0) return false;
			var world = this.GetModel<WorldMapModel>();
			var random = new System.Random(CombineSeed(world.WorldSeed, mSpawnIndex++, mMapConfig.GeneratorVersion));
			var selectedConfig = mConfigBuffer[random.Next(0, mConfigBuffer.Count)];
			var minimumDistance = Mathf.Max(0f, mMapConfig.Theme.MapEventMinimumPlayerDistance);
			var maximumDistance = Mathf.Max(minimumDistance + 8f, Mathf.Max(1, mMapConfig.ChunkSize) * Mathf.Max(1, mMapConfig.LoadRadius));
			for (var attempt = 0; attempt < 64; attempt++)
			{
				var angle = (float)(random.NextDouble() * Math.PI * 2d);
				var distance = Mathf.Lerp(minimumDistance, maximumDistance, (float)random.NextDouble());
				var worldCell = Vector2Int.RoundToInt(playerPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance);
				if (Mathf.Abs(worldCell.x) <= 1 && Mathf.Abs(worldCell.y) <= 1) continue;
				if (this.GetSystem<MapSystem>().TryGetCellFlags(worldCell, out var flags) && (flags & MapCellFlags.BlocksMovement) != 0) continue;
				var position = new Vector2(worldCell.x + 0.5f, worldCell.y + 0.5f);
				if (Vector2.Distance(playerPosition, position) < minimumDistance || !IsSpaced(position, selectedConfig.MinimumSpacing)) continue;
				var stableId = $"e:{world.WorldSeed}:{mSpawnIndex - 1}";
				if (mEvents.ContainsKey(stableId)) return true;
				var runtime = new RuntimeMapEvent(stableId, selectedConfig, position, 0f);
				mEvents.Add(stableId, runtime);
				CreateTrigger(runtime);
				return true;
			}
			return false;
		}

		private void RemoveIncompleteEvents()
		{
			if (mEvents.Count == 0) return;
			foreach (var runtime in mEvents.Values) DestroyTrigger(runtime);
			mEvents.Clear();
		}

		private void CompleteEvents(List<string> stableIds)
		{
			foreach (var stableId in stableIds)
			{
				if (!mEvents.TryGetValue(stableId, out var runtime)) continue;
				runtime.IsCompleted = true;
				this.SendEvent(new MapEventChangedEvent(CreateState(runtime)));
				if (runtime.Config.TemporaryRewardDropTableId > 0)
					this.GetSystem<PickupSystem>().SpawnFromTable(runtime.Config.TemporaryRewardDropTableId, runtime.Position);
				DestroyTrigger(runtime);
				mEvents.Remove(stableId);
			}
		}

		private static void CreateTrigger(RuntimeMapEvent runtime)
		{
			var container = WorldRootLocator.Get(WorldRootSlot.MapDecoration);
			if (container == null) return;
			if (runtime.Config.EventPrefab == null)
			{
				Debug.LogError($"Map event config requires EventPrefab: {runtime.Config.name}");
				return;
			}
			var triggerObject = UnityEngine.Object.Instantiate(runtime.Config.EventPrefab, container);
			triggerObject.transform.SetParent(container, false);
			triggerObject.transform.position = runtime.Position;
			triggerObject.name = $"MapEvent_{runtime.StableId}";
			var entity = triggerObject.GetComponent<MapEventEntityView>();
			var trigger = triggerObject.GetComponent<MapEventTriggerView>();
			if (entity == null || trigger == null || triggerObject.GetComponent<CircleCollider2D>() == null)
			{
				Debug.LogError($"Map event prefab requires MapEventEntityView, MapEventTriggerView and CircleCollider2D: {runtime.Config.name}");
				UnityEngine.Object.Destroy(triggerObject);
				return;
			}
			entity.Configure(runtime.Config.TriggerRadius);
			runtime.Entity = entity;
			trigger.Configure(runtime.StableId, runtime.Config.TriggerRadius);
			runtime.Trigger = trigger;
		}

		private static void DestroyTrigger(RuntimeMapEvent runtime)
		{
			if (runtime.Trigger != null) UnityEngine.Object.Destroy(runtime.Trigger.gameObject);
			runtime.Trigger = null;
		}

		private bool IsSpaced(Vector2 position, int spacing)
		{
			var minimum = Mathf.Max(0, spacing);
			foreach (var runtime in mEvents.Values)
				if (Vector2.Distance(position, runtime.Position) < minimum) return false;
			return true;
		}

		private static MapEventState CreateState(RuntimeMapEvent runtime)
		{
			return new MapEventState(runtime.StableId, runtime.Config.Id, runtime.Config.Type, runtime.Position,
				runtime.Config.TriggerRadius, runtime.HoldElapsed, runtime.Config.HoldDurationSeconds, runtime.IsActive, runtime.IsCompleted);
		}

		private static int CombineSeed(int worldSeed, int sequence, int version)
		{
			unchecked
			{
				var hash = worldSeed;
				hash = hash * 397 ^ sequence;
				return hash * 397 ^ version ^ 104729;
			}
		}

		protected override void OnInit()
		{
			this.RegisterEvent<EnemySpawnedEvent>(eventData => { if (eventData.IsBoss) RemoveIncompleteEvents(); });
			this.RegisterEvent<RunEndedEvent>(_ => Reset());
		}

		internal sealed class RuntimeMapEvent
		{
			public readonly string StableId;
			public readonly MapEventConfig Config;
			public readonly Vector2 Position;
			public float HoldElapsed;
			public bool IsActive;
			public bool IsCompleted;
			public MapEventTriggerView Trigger;
			public MapEventEntityView Entity;

			public RuntimeMapEvent(string stableId, MapEventConfig config, Vector2 position, float holdElapsed)
			{
				StableId = stableId;
				Config = config;
				Position = position;
				HoldElapsed = Mathf.Clamp(holdElapsed, 0f, config.HoldDurationSeconds);
			}
		}
	}
}
