using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class BreakableObjectFactory : MonoBehaviour
	{
		private static BreakableObjectFactory sInstance;
		private readonly Dictionary<GameObject, Queue<BreakableObjectView>> mPools = new();
		private readonly Dictionary<BreakableObjectView, GameObject> mPrefabs = new();

		public static BreakableObjectFactory Instance
		{
			get
			{
				if (sInstance != null) return sInstance;
				var factoryObject = new GameObject(nameof(BreakableObjectFactory));
				sInstance = factoryObject.AddComponent<BreakableObjectFactory>();
				DontDestroyOnLoad(factoryObject);
				return sInstance;
			}
		}

		public BreakableObjectView Spawn(BreakableObjectConfig config, string stableId, Vector2 position, Transform container)
		{
			if (config == null || config.Prefab == null || container == null) return null;
			PruneDestroyed();
			if (!mPools.TryGetValue(config.Prefab, out var pool))
			{
				pool = new Queue<BreakableObjectView>();
				mPools.Add(config.Prefab, pool);
			}
			while (pool.Count > 0)
			{
				var pooled = pool.Dequeue();
				if (pooled == null) continue;
				pooled.transform.SetParent(container, false);
				pooled.transform.position = position;
				pooled.gameObject.SetActive(true);
				pooled.Configure(stableId, config);
				return pooled;
			}
			var instance = Instantiate(config.Prefab, position, Quaternion.identity, container);
			var view = instance.GetComponent<BreakableObjectView>() ?? instance.AddComponent<BreakableObjectView>();
			mPrefabs[view] = config.Prefab;
			view.Configure(stableId, config);
			return view;
		}

		public void Release(BreakableObjectView view)
		{
			if (view == null || !mPrefabs.TryGetValue(view, out var prefab)) return;
			view.gameObject.SetActive(false);
			var container = WorldRootLocator.Get(WorldRootSlot.MapDecoration);
			if (container != null) view.transform.SetParent(container, false);
			if (!mPools.TryGetValue(prefab, out var pool))
			{
				pool = new Queue<BreakableObjectView>();
				mPools.Add(prefab, pool);
			}
			pool.Enqueue(view);
		}

		public void ReleaseAll()
		{
			PruneDestroyed();
			foreach (var view in new List<BreakableObjectView>(mPrefabs.Keys))
				if (view != null && view.gameObject.activeInHierarchy) Release(view);
		}

		private void PruneDestroyed()
		{
			var destroyed = new List<BreakableObjectView>();
			foreach (var view in mPrefabs.Keys)
				if (view == null) destroyed.Add(view);
			foreach (var view in destroyed) mPrefabs.Remove(view);
			foreach (var pool in mPools.Values)
			{
				var retained = new Queue<BreakableObjectView>();
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
			if (sInstance != null && sInstance != this) { Destroy(gameObject); return; }
			sInstance = this;
			DontDestroyOnLoad(gameObject);
		}
	}
}
