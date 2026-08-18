using System.Collections.Generic;
using QFramework;

namespace HaoFuSurvivor
{
	public class ProjectileSystem : AbstractSystem, IRunUpdateable, IRunFixedUpdateable
	{
		private readonly List<ProjectileController> mActiveProjectiles = new();

		public void Register(ProjectileController projectile)
		{
			if (projectile == null || mActiveProjectiles.Contains(projectile)) return;
			mActiveProjectiles.Add(projectile);
			if (this.GetSystem<RunTimerSystem>().IsRunning())
			{
				this.GetSystem<GameLoopSystem>().RegisterUpdateable(this);
				this.GetSystem<GameLoopSystem>().RegisterFixedUpdateable(this);
			}
		}

		public void Unregister(ProjectileController projectile)
		{
			mActiveProjectiles.Remove(projectile);
			if (mActiveProjectiles.Count != 0) return;
			this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
			this.GetSystem<GameLoopSystem>().UnregisterFixedUpdateable(this);
		}

		public void Reset()
		{
			ProjectileFactory.Instance.ReleaseAllActive();
			mActiveProjectiles.Clear();
			this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
			this.GetSystem<GameLoopSystem>().UnregisterFixedUpdateable(this);
		}

		public IEnumerable<ProjectileSaveData> GetSaveData()
		{
			var attacks = this.GetUtility<AttackCatalog>().Config;
			foreach (var projectile in mActiveProjectiles)
			{
				var parameters = ProjectileFactory.Instance.GetParameters(projectile);
				var attack = attacks?.Attacks.Find(item => item != null && item.ExecutorParameterConfig == parameters);
				if (projectile != null && attack != null) yield return projectile.GetSaveData(attack.Id);
			}
		}

		public void Restore(IEnumerable<ProjectileSaveData> entries)
		{
			Reset();
			if (entries == null) return;
			foreach (var entry in entries)
			{
				var parameters = entry == null ? null : this.GetUtility<AttackCatalog>().Get(entry.AttackId)?.ExecutorParameterConfig as ProjectileAttackParameterConfig;
				if (parameters != null) ProjectileFactory.Instance.SpawnRestored(parameters, entry);
			}
		}

		public void OnRunUpdate(float deltaTime)
		{
			for (var index = mActiveProjectiles.Count - 1; index >= 0; index--)
			{
				var projectile = mActiveProjectiles[index];
				if (projectile == null)
				{
					mActiveProjectiles.RemoveAt(index);
					continue;
				}
				projectile.Advance(deltaTime);
			}
			UnregisterWhenEmpty();
		}

		public void OnRunFixedUpdate(float deltaTime)
		{
			for (var index = mActiveProjectiles.Count - 1; index >= 0; index--)
			{
				var projectile = mActiveProjectiles[index];
				if (projectile == null)
				{
					mActiveProjectiles.RemoveAt(index);
					continue;
				}
				projectile.AdvanceFixed(deltaTime);
			}
			UnregisterWhenEmpty();
		}

		private void UnregisterWhenEmpty()
		{
			if (mActiveProjectiles.Count != 0) return;
			this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
			this.GetSystem<GameLoopSystem>().UnregisterFixedUpdateable(this);
		}

		protected override void OnInit()
		{
		}
	}
}
