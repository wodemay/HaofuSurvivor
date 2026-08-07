using System.Collections.Generic;
using QFramework;
using UnityEngine;
namespace HaoFuSurvivor
{
	public class EnemySystem : AbstractSystem
	{
		private readonly List<Transform> mEnemies = new();
        private readonly Dictionary<Transform, float> mMoveSpeeds = new();
		private float mSpawnElapsed;
		public void Reset()
		{
			foreach (var enemy in mEnemies)
			{
				this.GetSystem<EnemyHealthSystem>().Unregister(enemy.GetComponent<CombatEntity>());
				EnemyFactory.Instance.Release(enemy);
			}
			mEnemies.Clear(); mMoveSpeeds.Clear(); mSpawnElapsed = 0f; this.GetModel<EnemyModel>().AliveCount = 0;
		}

		public void Release(Transform enemy)
		{
			if (enemy == null || !mEnemies.Remove(enemy)) return;
			mMoveSpeeds.Remove(enemy);
			this.GetSystem<EnemyHealthSystem>().Unregister(enemy.GetComponent<CombatEntity>());
			EnemyFactory.Instance.Release(enemy);
			this.GetModel<EnemyModel>().AliveCount = mEnemies.Count;
		}
		public void Tick()
		{
			var deltaTime = this.GetModel<RunTimerModel>().FixedDeltaTime;
			if (deltaTime <= 0f || !this.GetSystem<RunTimerSystem>().IsRunning()) return;
			var player = this.GetModel<PlayerModel>();
			var catalog = this.GetUtility<EnemyCatalog>();
			var stageIndex = this.GetModel<RunTimerModel>().CurrentStageIndex;
			if (!player.IsRegistered || player.IsDead || catalog.Config == null) return;
			var ids = this.GetUtility<RunTimelineCatalog>().Config.Stages[Mathf.Max(0, stageIndex)].EnemyIds;
			if (ids.Count == 0) return;
			Spawn(catalog, ids, deltaTime); Move(player.Position, deltaTime);
		}
		private void Spawn(EnemyCatalog catalog, List<int> ids, float deltaTime)
		{
			var config = catalog.Config; mSpawnElapsed += deltaTime;
			var timer = this.GetModel<RunTimerModel>();
			var interval = Mathf.Max(config.MinSpawnInterval, config.BaseSpawnInterval / (1f + timer.ElapsedSeconds / config.SpawnRampSeconds) / timer.SpawnRateMultiplier);
			while (mSpawnElapsed >= interval && mEnemies.Count < config.MaxAliveEnemies)
			{
				mSpawnElapsed -= interval;
				var count = Mathf.Min(config.MaxEnemiesPerWave, 1 + Mathf.FloorToInt(timer.ElapsedSeconds / config.SecondsPerExtraEnemy));
				for (var i = 0; i < count && mEnemies.Count < config.MaxAliveEnemies; i++)
				{
					var enemyConfig = catalog.Get(ids[Random.Range(0, ids.Count)]);
					if (enemyConfig != null)
					{
						var enemy = EnemyFactory.Instance.Create(enemyConfig, GetSpawnPosition(config.ViewportPadding));
						if (enemy == null) continue;
						mEnemies.Add(enemy);
						mMoveSpeeds[enemy] = enemyConfig.MoveSpeed;
					}
				}
			}
		}
		private void Move(Vector2 playerPosition, float deltaTime)
		{
			var multiplier = this.GetModel<RunTimerModel>().EnemyMoveSpeedMultiplier;
			for (var i = mEnemies.Count - 1; i >= 0; i--)
			{
				var enemy = mEnemies[i]; if (enemy == null) { mEnemies.RemoveAt(i); mMoveSpeeds.Remove(enemy); continue; }
				var nextPosition = Vector2.MoveTowards(enemy.position, playerPosition, mMoveSpeeds[enemy] * multiplier * deltaTime);
				var rigidbody = enemy.GetComponent<Rigidbody2D>();
				if (rigidbody != null) rigidbody.MovePosition(nextPosition);
				else enemy.position = nextPosition;
			}
			this.GetModel<EnemyModel>().AliveCount = mEnemies.Count;
		}
		private static Vector3 GetSpawnPosition(float padding)
		{
			var camera = Camera.main; if (camera == null) return Vector3.zero;
			var horizontal = Random.value < .5f;
			var point = horizontal ? new Vector3(Random.value < .5f ? -padding : 1f + padding, Random.Range(-padding, 1f + padding)) : new Vector3(Random.Range(-padding, 1f + padding), Random.value < .5f ? -padding : 1f + padding);
			var position = camera.ViewportToWorldPoint(new Vector3(point.x, point.y, Mathf.Abs(camera.transform.position.z))); position.z = 0f; return position;
		}
		protected override void OnInit() { }
	}
}
