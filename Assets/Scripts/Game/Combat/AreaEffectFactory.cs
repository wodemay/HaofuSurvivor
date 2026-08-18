using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class AreaEffectFactory : MonoBehaviour
	{
		private static AreaEffectFactory sInstance;
		private readonly Dictionary<GameObject, Queue<GameObject>> mPools = new();
		private readonly Dictionary<GameObject, GameObject> mPrefabs = new();
		private readonly Dictionary<GameObject, WorldRootSlot> mRoots = new();

		public static AreaEffectFactory Instance
		{
			get
			{
				if (sInstance != null) return sInstance;
				var gameObject = new GameObject(nameof(AreaEffectFactory));
				sInstance = gameObject.AddComponent<AreaEffectFactory>();
				DontDestroyOnLoad(gameObject);
				return sInstance;
			}
		}

		public GameObject Spawn(GameObject prefab, Vector2 position, WorldRootSlot rootSlot)
		{
			var effect = Get(prefab, rootSlot);
			if (effect == null) return null;
			var container = GetContainer(rootSlot);
			if (container == null) return null;
			effect.transform.SetParent(container, false);
			effect.transform.position = position;
			mRoots[effect] = rootSlot;
			effect.SetActive(true);
			return effect;
		}

		public void Release(GameObject effect)
		{
			if (effect == null || !mPrefabs.TryGetValue(effect, out var prefab)) return;
			effect.SetActive(false);
			if (mRoots.TryGetValue(effect, out var rootSlot))
			{
				var container = GetContainer(rootSlot);
				if (container != null) effect.transform.SetParent(container, false);
			}
			mPools[prefab].Enqueue(effect);
		}

		private GameObject Get(GameObject prefab, WorldRootSlot rootSlot)
		{
			if (prefab == null) return null;
			var container = GetContainer(rootSlot);
			if (container == null) return null;
			PruneDestroyedEffects();
			if (!mPools.TryGetValue(prefab, out var pool))
			{
				pool = new Queue<GameObject>();
				mPools.Add(prefab, pool);
			}
			while (pool.Count > 0)
			{
				var effect = pool.Dequeue();
				if (effect != null) return effect;
			}
			var instance = Instantiate(prefab, container);
			mPrefabs.Add(instance, prefab);
			return instance;
		}

		private void PruneDestroyedEffects()
		{
			var destroyed = new List<GameObject>();
			foreach (var effect in mPrefabs.Keys)
				if (effect == null) destroyed.Add(effect);
			foreach (var effect in destroyed)
			{
				mPrefabs.Remove(effect);
				mRoots.Remove(effect);
			}
		}

		private static Transform GetContainer(WorldRootSlot rootSlot)
		{
			return WorldRootLocator.Get(rootSlot);
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
