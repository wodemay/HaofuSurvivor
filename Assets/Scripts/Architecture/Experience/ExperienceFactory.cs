using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class ExperienceFactory : MonoBehaviour
	{
		private static ExperienceFactory sInstance;
		private readonly Dictionary<int, Queue<ExperienceDropController>> mPools = new();
		private readonly HashSet<ExperienceDropController> mActive = new();

		public static ExperienceFactory Instance
		{
			get
			{
				if (sInstance != null) return sInstance;
				var factoryObject = new GameObject(nameof(ExperienceFactory));
				sInstance = factoryObject.AddComponent<ExperienceFactory>();
				DontDestroyOnLoad(factoryObject);
				return sInstance;
			}
		}

		public ExperienceDropController Create(ExperienceDropConfig config, Vector2 position)
		{
			if (config == null || config.Prefab == null) return null;
			var container = GetContainer();
			if (container == null) return null;
			PruneDestroyedEntries();
			if (!mPools.TryGetValue(config.Id, out var pool))
			{
				pool = new Queue<ExperienceDropController>();
				mPools.Add(config.Id, pool);
			}

			while (pool.Count > 0)
			{
				var pooled = pool.Dequeue();
				if (pooled == null) continue;
				pooled.transform.SetParent(container, false);
				pooled.transform.position = position;
				pooled.gameObject.SetActive(true);
				mActive.Add(pooled);
				return pooled;
			}

			var dropObject = Instantiate(config.Prefab, position, Quaternion.identity, container);
			var controller = dropObject.GetComponent<ExperienceDropController>();
			if (controller == null) controller = dropObject.AddComponent<ExperienceDropController>();
			mActive.Add(controller);
			return controller;
		}

		public void Release(ExperienceDropConfig config, ExperienceDropController controller)
		{
			if (config == null || controller == null) return;
			mActive.Remove(controller);
			if (!mPools.TryGetValue(config.Id, out var pool))
			{
				pool = new Queue<ExperienceDropController>();
				mPools.Add(config.Id, pool);
			}
			controller.gameObject.SetActive(false);
			var container = GetContainer();
			if (container != null) controller.transform.SetParent(container, false);
			pool.Enqueue(controller);
		}

		private static Transform GetContainer()
		{
			return WorldRootLocator.Get(WorldRootSlot.Pickup);
		}

		private void PruneDestroyedEntries()
		{
			mActive.RemoveWhere(drop => drop == null);
			foreach (var pool in mPools.Values)
			{
				var retained = new Queue<ExperienceDropController>();
				while (pool.Count > 0)
				{
					var drop = pool.Dequeue();
					if (drop != null) retained.Enqueue(drop);
				}
				foreach (var drop in retained) pool.Enqueue(drop);
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
