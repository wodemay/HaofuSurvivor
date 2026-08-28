using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class PickupFactory : MonoBehaviour
	{
		private static PickupFactory sInstance;
		private readonly Dictionary<GameObject, Queue<PickupController>> mPools = new();
		private readonly Dictionary<PickupController, GameObject> mPrefabs = new();

		public static PickupFactory Instance
		{
			get
			{
				if (sInstance != null) return sInstance;
				var obj = new GameObject(nameof(PickupFactory));
				sInstance = obj.AddComponent<PickupFactory>();
				DontDestroyOnLoad(obj);
				return sInstance;
			}
		}

		public PickupController Spawn(int tableId, DropEntry entry, Vector2 position)
		{
			if (entry?.Prefab == null) return null;
			var container = WorldRootLocator.Get(WorldRootSlot.Pickup);
			if (container == null) return null;
			if (!mPools.TryGetValue(entry.Prefab, out var pool))
			{
				pool = new Queue<PickupController>();
				mPools.Add(entry.Prefab, pool);
			}
			while (pool.Count > 0)
			{
				var pooled = pool.Dequeue();
				if (pooled == null) continue;
				pooled.transform.SetParent(container, false);
				pooled.transform.position = position;
				pooled.gameObject.SetActive(true);
				pooled.Configure(tableId, entry);
				return pooled;
			}
			var instance = Instantiate(entry.Prefab, position, Quaternion.identity, container);
			var controller = instance.GetComponent<PickupController>() ?? instance.AddComponent<PickupController>();
			mPrefabs[controller] = entry.Prefab;
			controller.Configure(tableId, entry);
			return controller;
		}

		public void Release(PickupController pickup)
		{
			if (pickup == null || !mPrefabs.TryGetValue(pickup, out var prefab)) return;
			pickup.gameObject.SetActive(false);
			var container = WorldRootLocator.Get(WorldRootSlot.Pickup);
			if (container != null) pickup.transform.SetParent(container, false);
			if (!mPools.TryGetValue(prefab, out var pool))
			{
				pool = new Queue<PickupController>();
				mPools.Add(prefab, pool);
			}
			pool.Enqueue(pickup);
		}

		public void ReleaseAll()
		{
			foreach (var pickup in new List<PickupController>(mPrefabs.Keys))
				if (pickup != null && pickup.gameObject.activeInHierarchy) Release(pickup);
		}

		private void Awake()
		{
			if (sInstance != null && sInstance != this) { Destroy(gameObject); return; }
			sInstance = this;
			DontDestroyOnLoad(gameObject);
		}
	}
}
