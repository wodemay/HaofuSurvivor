using System.Collections.Generic;
using QFramework;
using UnityEngine;
namespace HaoFuSurvivor
{
	public class EnemySystem : AbstractSystem, IRunFixedUpdateable
	{
		private readonly List<Transform> mEnemies = new();
        private readonly Dictionary<Transform, float> mMoveSpeeds = new();
		private readonly Dictionary<Transform, float> mBodyRadii = new();
		private readonly List<Transform> mMoveEnemies = new();
		private readonly List<Vector2> mNextPositions = new();
		private const float DefaultBodyRadius = 0.5f;
		private const int SeparationIterations = 2;
		private float mSpawnElapsed;
		public void Reset()
		{
			foreach (var enemy in mEnemies)
			{
				if (enemy == null) continue;
				this.GetSystem<EnemyHealthSystem>().Unregister(enemy.GetComponent<CombatEntity>());
				EnemyFactory.Instance.Release(enemy);
			}
			EnemyFactory.Instance.ReleaseAllActive();
			mEnemies.Clear(); mMoveSpeeds.Clear(); mBodyRadii.Clear(); mMoveEnemies.Clear(); mNextPositions.Clear(); mSpawnElapsed = 0f; this.GetModel<EnemyModel>().AliveCount = 0;
		}

		public void Release(Transform enemy)
		{
			if (enemy == null || !mEnemies.Remove(enemy)) return;
			mMoveSpeeds.Remove(enemy);
			mBodyRadii.Remove(enemy);
			this.GetSystem<EnemyHealthSystem>().Unregister(enemy.GetComponent<CombatEntity>());
			EnemyFactory.Instance.Release(enemy);
			this.GetModel<EnemyModel>().AliveCount = mEnemies.Count;
		}

		public IEnumerable<EnemySaveData> GetSaveData()
		{
			foreach (var enemy in mEnemies)
			{
				if (enemy == null) continue;
				var config = EnemyFactory.Instance.GetConfig(enemy);
				var entity = enemy.GetComponent<CombatEntity>();
				if (config == null || entity == null) continue;
				var data = new EnemySaveData
				{
					ConfigId = config.Id,
					PositionX = enemy.position.x,
					PositionY = enemy.position.y,
					CurrentHealth = this.GetSystem<EnemyHealthSystem>().GetCurrentHealth(entity),
					MoveSpeed = mMoveSpeeds.TryGetValue(enemy, out var moveSpeed) ? moveSpeed : config.MoveSpeed
				};
				data.AttackCooldowns.AddRange(this.GetSystem<AttackSystem>().GetCooldownSaveData(enemy.gameObject));
				yield return data;
			}
		}

		public void Restore(IEnumerable<EnemySaveData> entries, float spawnElapsed)
		{
			Reset();
			var catalog = this.GetUtility<EnemyCatalog>();
			if (entries != null)
				foreach (var entry in entries)
				{
					var config = entry == null ? null : catalog.Get(entry.ConfigId);
					var enemy = config == null ? null : EnemyFactory.Instance.Create(config, new Vector2(entry.PositionX, entry.PositionY));
					if (enemy == null) continue;
					mEnemies.Add(enemy);
					mMoveSpeeds[enemy] = Mathf.Max(0f, entry.MoveSpeed);
					mBodyRadii[enemy] = GetBodyRadius(enemy);
					this.GetSystem<EnemyHealthSystem>().RestoreCurrentHealth(enemy.GetComponent<CombatEntity>(), entry.CurrentHealth);
					this.GetSystem<AttackSystem>().RestoreCooldowns(enemy.gameObject, entry.AttackCooldowns);
				}
			mSpawnElapsed = Mathf.Max(0f, spawnElapsed);
			this.GetModel<EnemyModel>().AliveCount = mEnemies.Count;
		}

		public float GetSpawnElapsed() => mSpawnElapsed;
		public void OnRunFixedUpdate(float deltaTime)
		{
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
						mBodyRadii[enemy] = GetBodyRadius(enemy);
					}
				}
			}
		}
		private void Move(Vector2 playerPosition, float deltaTime)
		{
			var multiplier = this.GetModel<RunTimerModel>().EnemyMoveSpeedMultiplier;
			mMoveEnemies.Clear();
			mNextPositions.Clear();
			for (var i = mEnemies.Count - 1; i >= 0; i--)
			{
				var enemy = mEnemies[i];
				if (enemy == null)
				{
					mEnemies.RemoveAt(i);
					mMoveSpeeds.Remove(enemy);
					mBodyRadii.Remove(enemy);
					continue;
				}

				mMoveEnemies.Add(enemy);
				var moveSpeed = mMoveSpeeds.TryGetValue(enemy, out var speed) ? speed : 0f;
				mNextPositions.Add(Vector2.MoveTowards(enemy.position, playerPosition, moveSpeed * multiplier * deltaTime));
			}

			for (var iteration = 0; iteration < SeparationIterations; iteration++)
			{
				for (var i = 0; i < mMoveEnemies.Count; i++)
				{
					for (var j = i + 1; j < mMoveEnemies.Count; j++)
					{
						var first = mMoveEnemies[i];
						var second = mMoveEnemies[j];
						var firstRadius = mBodyRadii.TryGetValue(first, out var firstValue) ? firstValue : DefaultBodyRadius;
						var secondRadius = mBodyRadii.TryGetValue(second, out var secondValue) ? secondValue : DefaultBodyRadius;
						var offset = mNextPositions[j] - mNextPositions[i];
						var distance = offset.magnitude;
						var minimumDistance = firstRadius + secondRadius;
						if (distance >= minimumDistance) continue;

						var direction = distance > 0.0001f ? offset / distance : GetOverlapDirection(i, j);
						var correction = (minimumDistance - distance) * 0.5f;
						mNextPositions[i] -= direction * correction;
						mNextPositions[j] += direction * correction;
					}
				}
			}

			for (var i = 0; i < mMoveEnemies.Count; i++)
			{
				var enemy = mMoveEnemies[i];
				var rigidbody = enemy.GetComponent<Rigidbody2D>();
				if (rigidbody != null) rigidbody.MovePosition(mNextPositions[i]);
				else enemy.position = mNextPositions[i];
			}
			this.GetModel<EnemyModel>().AliveCount = mEnemies.Count;
		}

		private static float GetBodyRadius(Transform enemy)
		{
			var bodyCollider = enemy == null ? null : enemy.Find("BodyCollider")?.GetComponent<Collider2D>();
			if (bodyCollider == null) return DefaultBodyRadius;
			var bounds = bodyCollider.bounds;
			return Mathf.Max(0.05f, Mathf.Max(bounds.extents.x, bounds.extents.y));
		}

		private static Vector2 GetOverlapDirection(int firstIndex, int secondIndex)
		{
			var angle = (firstIndex * 97 + secondIndex * 53) * Mathf.Deg2Rad;
			return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
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
