using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class ProjectileFactory : MonoBehaviour
	{
		private const string ContainerName = "ProjectileContainer";
		private static ProjectileFactory sInstance;
		private readonly Dictionary<GameObject, Queue<ProjectileController>> mPools = new();
		private readonly Dictionary<ProjectileController, GameObject> mPrefabs = new();

		public static ProjectileFactory Instance
		{
			get
			{
				if (sInstance != null) return sInstance;
				var gameObject = new GameObject(nameof(ProjectileFactory));
				sInstance = gameObject.AddComponent<ProjectileFactory>();
				DontDestroyOnLoad(gameObject);
				return sInstance;
			}
		}

		public void Spawn(ProjectileAttackParameterConfig parameters, Vector2 position, Vector2 direction,
			CombatFaction ownerFaction, float damage)
		{
			var projectile = Get(parameters);
			if (projectile == null) return;
			projectile.gameObject.SetActive(true);
			projectile.Launch(position, direction, ownerFaction, damage, parameters.MoveSpeed, parameters.Lifetime);
		}

		public void Release(ProjectileController projectile)
		{
			if (projectile == null || !mPrefabs.TryGetValue(projectile, out var prefab)) return;
			projectile.ResetState();
			projectile.gameObject.SetActive(false);
			projectile.transform.SetParent(GetContainer(), false);
			mPools[prefab].Enqueue(projectile);
		}

		private ProjectileController Get(ProjectileAttackParameterConfig parameters)
		{
			if (parameters.ProjectilePrefab == null) return null;
			if (!mPools.TryGetValue(parameters.ProjectilePrefab, out var pool))
			{
				pool = new Queue<ProjectileController>(Mathf.Max(0, parameters.PoolCapacity));
				mPools.Add(parameters.ProjectilePrefab, pool);
			}
			if (pool.Count > 0) return pool.Dequeue();

			var projectileObject = Instantiate(parameters.ProjectilePrefab, GetContainer());
			var projectile = projectileObject.GetComponent<ProjectileController>();
			if (projectile == null) projectile = projectileObject.AddComponent<ProjectileController>();
			mPrefabs.Add(projectile, parameters.ProjectilePrefab);
			return projectile;
		}

		private static Transform GetContainer()
		{
			var container = GameObject.Find(ContainerName);
			return container != null ? container.transform : new GameObject(ContainerName).transform;
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
