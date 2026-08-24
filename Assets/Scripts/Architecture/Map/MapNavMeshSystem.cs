using NavMeshPlus.Components;
using NavMeshPlus.Extensions;
using QFramework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace HaoFuSurvivor
{
	public class MapNavMeshSystem : AbstractSystem, IRunUpdateable
	{
		private const string NavigationRootName = "RuntimeNavigationRoot";
		private const float SpawnSampleDistance = 0.75f;
		private const float RebuildInterval = 0.15f;
		private NavMeshSurface mSurface;
		private readonly Dictionary<Transform, PathRuntime> mPaths = new();
		private AsyncOperation mBuildOperation;
		private bool mIsDirty;
		private bool mIsReady;
		private float mRebuildElapsed;
		private int mVersion;
		private int mDirtyVersion;

		public bool IsReady => mIsReady;
		public int Version => mVersion;

		public void Reset()
		{
			CancelPendingBuild();
			mIsDirty = true;
			mIsReady = false;
			mRebuildElapsed = 0f;
			mVersion = 0;
			mDirtyVersion = 0;
			mPaths.Clear();
			if (mSurface == null) return;
			mSurface.RemoveData();
			Object.Destroy(mSurface.gameObject);
			mSurface = null;
		}

		public void PrepareForMapMutation()
		{
			CancelPendingBuild();
		}

		public void MarkDirty()
		{
			mIsDirty = true;
			mDirtyVersion++;
			mRebuildElapsed = 0f;
			mPaths.Clear();
		}

		public void Invalidate(Transform enemy)
		{
			if (enemy != null) mPaths.Remove(enemy);
		}

		public Vector2 GetDirection(Transform enemy, Vector2 position, Vector2 targetPosition)
		{
			if (enemy == null || !mIsReady) return Vector2.zero;
			if (!mPaths.TryGetValue(enemy, out var runtime))
			{
				runtime = new PathRuntime();
				mPaths.Add(enemy, runtime);
			}
			var targetMoved = (runtime.Target - targetPosition).sqrMagnitude > 0.5625f;
			var needsPath = runtime.Version != mVersion || targetMoved || runtime.Corners != null && runtime.Corners.Length > 1 && runtime.CornerIndex >= runtime.Corners.Length;
			if (needsPath)
			{
				runtime.Path.ClearCorners();
				if (!TryCalculatePath(position, targetPosition, runtime.Path))
				{
					runtime.Corners = null;
					runtime.Target = targetPosition;
					runtime.Version = mVersion;
					return Vector2.zero;
				}
				runtime.Corners = runtime.Path.corners;
				runtime.CornerIndex = 1;
				runtime.Target = targetPosition;
				runtime.Version = mVersion;
			}
			while (runtime.Corners != null && runtime.CornerIndex < runtime.Corners.Length - 1 && ((Vector2)runtime.Corners[runtime.CornerIndex] - position).sqrMagnitude < 0.09f)
				runtime.CornerIndex++;
			if (runtime.Corners == null || runtime.CornerIndex >= runtime.Corners.Length) return Vector2.zero;
			return ((Vector2)runtime.Corners[runtime.CornerIndex] - position).normalized;
		}

		public void OnRunUpdate(float deltaTime)
		{
			if (mBuildOperation != null)
			{
				if (!mBuildOperation.isDone) return;
				mBuildOperation = null;
				if (mBuildToken == mDirtyVersion)
				{
					mIsDirty = false;
					mIsReady = mSurface != null && mSurface.navMeshDataInstance.valid;
					if (mIsReady) mVersion++;
				}
			}
			var map = this.GetModel<MapModel>();
			if (!map.HasCurrentChunk || !mIsDirty) return;
			if (this.GetSystem<MapSystem>().HasPendingChunkOperations) return;
			mRebuildElapsed += deltaTime;
			if (mSurface != null && mRebuildElapsed < RebuildInterval) return;
			RebuildAsync();
		}

		public bool IsWithinLoadedArea(Vector2 position)
		{
			var map = this.GetModel<MapModel>();
			var config = this.GetUtility<MapGridCatalog>().Config;
			if (!map.HasCurrentChunk || config == null) return false;
			var chunkSize = Mathf.Max(1, config.ChunkSize);
			var chunk = new Vector2Int(Mathf.FloorToInt(position.x / chunkSize), Mathf.FloorToInt(position.y / chunkSize));
			var radius = Mathf.Max(0, config.LoadRadius);
			return Mathf.Abs(chunk.x - map.CurrentChunk.x) <= radius && Mathf.Abs(chunk.y - map.CurrentChunk.y) <= radius;
		}

		public bool IsWithinWindow(Vector2 position) => IsWithinLoadedArea(position);

		public bool IsWalkable(Vector2 position, float bodyRadius)
		{
			return TrySampleWalkable(position, Mathf.Max(SpawnSampleDistance, bodyRadius), out _) && !IsOverlappingMoveBlocker(position, bodyRadius);
		}

		public bool TryFindSpawnPosition(Vector2 playerPosition, float viewportPadding, float bodyRadius, out Vector3 position)
		{
			position = Vector3.zero;
			if (!mIsReady) return false;
			var camera = Camera.main;
			for (var attempt = 0; attempt < 24; attempt++)
			{
				Vector2 candidate;
				if (camera != null)
				{
					var horizontal = Random.value < 0.5f;
					var viewport = horizontal
						? new Vector2(Random.value < 0.5f ? -viewportPadding : 1f + viewportPadding, Random.Range(-viewportPadding, 1f + viewportPadding))
						: new Vector2(Random.Range(-viewportPadding, 1f + viewportPadding), Random.value < 0.5f ? -viewportPadding : 1f + viewportPadding);
					candidate = camera.ViewportToWorldPoint(new Vector3(viewport.x, viewport.y, Mathf.Abs(camera.transform.position.z)));
				}
				else
				{
					var angle = Random.value * Mathf.PI * 2f;
					candidate = playerPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(8f, 12f);
				}
				if (!TrySampleWalkable(candidate, Mathf.Max(SpawnSampleDistance, bodyRadius), out var sample)) continue;
				if (IsOverlappingMoveBlocker(sample.position, bodyRadius)) continue;
				position = sample.position;
				return true;
			}
			return false;
		}

		public bool TryCalculatePath(Vector2 start, Vector2 target, NavMeshPath path)
		{
			if (!mIsReady || path == null) return false;
			if (!TrySampleWalkable(start, SpawnSampleDistance, out var startSample)) return false;
			if (!TrySampleWalkable(target, SpawnSampleDistance, out var targetSample)) return false;
			return NavMesh.CalculatePath(startSample.position, targetSample.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete && path.corners.Length > 1;
		}

		private int mBuildToken;

		private void RebuildAsync()
		{
			if (!EnsureSurface()) return;
			mBuildToken = mDirtyVersion;
			mBuildOperation = mSurface.navMeshData == null
				? mSurface.BuildNavMeshAsync()
				: mSurface.UpdateNavMesh(mSurface.navMeshData);
			mIsReady = mSurface.navMeshDataInstance.valid;
			mRebuildElapsed = 0f;
		}

		private void CancelPendingBuild()
		{
			if (mBuildOperation == null) return;
			if (!mBuildOperation.isDone && mSurface != null && mSurface.navMeshData != null)
				NavMeshBuilder.Cancel(mSurface.navMeshData);
			mBuildOperation = null;
		}

		private bool EnsureSurface()
		{
			if (mSurface != null) return true;
			var container = WorldRootLocator.Get(WorldRootSlot.MapBackground);
			if (container == null) return false;
			var root = new GameObject(NavigationRootName);
			root.transform.SetParent(container, false);
			root.transform.localRotation = Quaternion.Euler(270f, 0f, 0f);
			mSurface = root.AddComponent<NavMeshSurface>();
			mSurface.collectObjects = CollectObjects.All;
			mSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
			mSurface.defaultArea = 0;
			mSurface.ignoreNavMeshAgent = true;
			mSurface.ignoreNavMeshObstacle = true;
			mSurface.hideEditorLogs = true;
			root.AddComponent<CollectSources2d>();
			return true;
		}

		private static bool TrySampleWalkable(Vector2 position, float maxDistance, out NavMeshHit sample)
		{
			return NavMesh.SamplePosition(position, out sample, Mathf.Max(0.01f, maxDistance), NavMesh.AllAreas);
		}

		private static bool IsOverlappingMoveBlocker(Vector2 position, float radius)
		{
			foreach (var collider in Physics2D.OverlapCircleAll(position, Mathf.Max(0.01f, radius)))
				if (MapColliderUtility.IsMoveBlocker(collider)) return true;
			return false;
		}

		protected override void OnInit()
		{
			this.RegisterEvent<RunEndedEvent>(_ => Reset());
		}

		private sealed class PathRuntime
		{
			public readonly NavMeshPath Path = new();
			public Vector3[] Corners;
			public Vector2 Target;
			public int CornerIndex;
			public int Version = -1;
		}
	}
}
