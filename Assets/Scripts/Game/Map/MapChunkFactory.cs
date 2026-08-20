using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class MapChunkFactory : MonoBehaviour
	{
		private static MapChunkFactory sInstance;
		private readonly Dictionary<GameObject, Queue<MapChunkView>> mPools = new();
		private readonly Dictionary<MapChunkView, GameObject> mPrefabs = new();

		public static MapChunkFactory Instance
		{
			get
			{
				if (sInstance != null) return sInstance;
				var factoryObject = new GameObject(nameof(MapChunkFactory));
				sInstance = factoryObject.AddComponent<MapChunkFactory>();
				DontDestroyOnLoad(factoryObject);
				return sInstance;
			}
		}

		public MapChunkView Spawn(MapGridConfig config, Vector2Int coordinate, Transform container)
		{
			if (config == null || config.ChunkPrefab == null || container == null) return null;
			PruneDestroyedEntries();
			if (!mPools.TryGetValue(config.ChunkPrefab, out var pool))
			{
				pool = new Queue<MapChunkView>();
				mPools.Add(config.ChunkPrefab, pool);
			}

			while (pool.Count > 0)
			{
				var pooled = pool.Dequeue();
				if (pooled == null) continue;
				pooled.transform.SetParent(container, false);
				pooled.gameObject.SetActive(true);
				pooled.Configure(coordinate, config);
				return pooled;
			}

			var instance = Instantiate(config.ChunkPrefab, container);
			var view = instance.GetComponent<MapChunkView>();
			if (view == null) view = instance.AddComponent<MapChunkView>();
			mPrefabs[view] = config.ChunkPrefab;
			view.Configure(coordinate, config);
			return view;
		}

		public void Release(MapChunkView view)
		{
			if (view == null || !mPrefabs.TryGetValue(view, out var prefab)) return;
			view.ResetView();
			view.gameObject.SetActive(false);
			if (!mPools.TryGetValue(prefab, out var pool))
			{
				pool = new Queue<MapChunkView>();
				mPools.Add(prefab, pool);
			}
			pool.Enqueue(view);
		}

		public void ReleaseAll()
		{
			PruneDestroyedEntries();
			foreach (var view in new List<MapChunkView>(mPrefabs.Keys))
				if (view != null && view.gameObject.activeInHierarchy) Release(view);
		}

		private void PruneDestroyedEntries()
		{
			var destroyed = new List<MapChunkView>();
			foreach (var entry in mPrefabs)
				if (entry.Key == null) destroyed.Add(entry.Key);
			foreach (var view in destroyed) mPrefabs.Remove(view);

			foreach (var pool in mPools.Values)
			{
				var retained = new Queue<MapChunkView>();
				while (pool.Count > 0)
				{
					var view = pool.Dequeue();
					if (view != null) retained.Enqueue(view);
				}
				foreach (var view in retained) pool.Enqueue(view);
			}
		}

		private void Awake()
		{
			if (sInstance != null && sInstance != this)
			{
				Destroy(gameObject);
				return;
			}
			sInstance = this;
			DontDestroyOnLoad(gameObject);
		}
	}
}
