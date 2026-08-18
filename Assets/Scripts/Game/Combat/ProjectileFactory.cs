using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class ProjectileFactory : MonoBehaviour
	{
		private static ProjectileFactory sInstance;
		private readonly Dictionary<ProjectileAttackParameterConfig, Queue<ProjectileController>> mPools = new();
		private readonly Dictionary<ProjectileController, ProjectileAttackParameterConfig> mParameters = new();

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
			CombatFaction ownerFaction, float damage, float moveSpeed, int pierce)
		{
			var projectile = Get(parameters);
			if (projectile == null) return;
			projectile.gameObject.SetActive(true);
			projectile.ConfigureParameters(parameters);
			projectile.Launch(position, direction, ownerFaction, damage, moveSpeed, parameters.Lifetime, pierce);
			GameArchitecture.Interface.GetSystem<ProjectileSystem>().Register(projectile);
		}

		public void SpawnRestored(ProjectileAttackParameterConfig parameters, ProjectileSaveData data)
		{
			var projectile = Get(parameters);
			if (projectile == null || data == null) return;
			projectile.gameObject.SetActive(true);
			projectile.ConfigureParameters(parameters);
			projectile.Restore(data);
			GameArchitecture.Interface.GetSystem<ProjectileSystem>().Register(projectile);
		}

		public ProjectileAttackParameterConfig GetParameters(ProjectileController projectile)
		{
			return projectile != null && mParameters.TryGetValue(projectile, out var parameters) ? parameters : null;
		}

		public void Release(ProjectileController projectile)
		{
			if (projectile == null || !mParameters.TryGetValue(projectile, out var parameters)) return;
			GameArchitecture.Interface.GetSystem<ProjectileSystem>().Unregister(projectile);
			projectile.ResetState();
			projectile.gameObject.SetActive(false);
			var container = GetContainer();
			if (container != null) projectile.transform.SetParent(container, false);
			mPools[parameters].Enqueue(projectile);
		}

		public void ReleaseAllActive()
		{
			PruneDestroyedProjectiles();
			foreach (var projectile in new List<ProjectileController>(mParameters.Keys))
			{
				if (projectile != null && projectile.gameObject.activeInHierarchy) Release(projectile);
			}
		}

		private ProjectileController Get(ProjectileAttackParameterConfig parameters)
		{
			if (parameters.ProjectilePrefab == null) return null;
			var container = GetContainer();
			if (container == null) return null;
			PruneDestroyedProjectiles();
			if (!mPools.TryGetValue(parameters, out var pool))
			{
				pool = new Queue<ProjectileController>(Mathf.Max(0, parameters.PoolCapacity));
				mPools.Add(parameters, pool);
			}
			while (pool.Count > 0)
			{
				var pooledProjectile = pool.Dequeue();
				if (pooledProjectile != null) return pooledProjectile;
			}

			var projectileObject = Instantiate(parameters.ProjectilePrefab, container);
			var projectile = parameters is ExplosiveProjectileAttackParameterConfig
				? projectileObject.GetComponent<ExplosiveProjectileController>() ?? projectileObject.AddComponent<ExplosiveProjectileController>()
				: projectileObject.GetComponent<ProjectileController>() ?? projectileObject.AddComponent<ProjectileController>();
			mParameters.Add(projectile, parameters);
			return projectile;
		}

		private static Transform GetContainer()
		{
			return WorldRootLocator.Get(WorldRootSlot.Projectile);
		}

		private void PruneDestroyedProjectiles()
		{
			var destroyedProjectiles = new List<ProjectileController>();
			foreach (var projectile in mParameters.Keys)
			{
				if (projectile == null) destroyedProjectiles.Add(projectile);
			}
			foreach (var projectile in destroyedProjectiles) mParameters.Remove(projectile);
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
