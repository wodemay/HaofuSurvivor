using UnityEngine;
using System.Collections.Generic;
namespace HaoFuSurvivor
{
	public class EnemyFactory : MonoBehaviour
	{
		private const string EnemyRootConfigPath = "Configs/Enemy/EnemyRoot";
		private const string CharacterRootName = "CharacterRoot";
		private static EnemyFactory sInstance;
		private static Sprite sPlaceholderSprite;
		private readonly Dictionary<int, Queue<GameObject>> mPools = new();
		private readonly Dictionary<GameObject, int> mEnemyIds = new();
		private readonly Dictionary<GameObject, EnemyConfig> mConfigs = new();
		public static EnemyFactory Instance
		{
			get
			{
				if (sInstance != null) return sInstance;
				var gameObject = new GameObject(nameof(EnemyFactory));
				sInstance = gameObject.AddComponent<EnemyFactory>();
				DontDestroyOnLoad(gameObject);
				return sInstance;
			}
		}
		public Transform Create(EnemyConfig config, Vector3 position)
		{
			PruneDestroyedRoots();
			var container = GetContainer();
			if (container == null) return null;
			var rootConfig = Resources.Load<EnemyRootConfig>(EnemyRootConfigPath);
			if (rootConfig != null && rootConfig.EnemyPrefab != null)
			{
				if (mPools.TryGetValue(config.Id, out var pool))
				{
					while (pool.Count > 0)
					{
						var pooledRoot = pool.Dequeue();
						if (pooledRoot == null) continue;
						pooledRoot.transform.SetParent(container, false);
						pooledRoot.transform.SetPositionAndRotation(position, Quaternion.identity);
						pooledRoot.SetActive(true);
						ConfigureRoot(pooledRoot, config);
						mConfigs[pooledRoot] = config;
						return pooledRoot.transform;
					}
				}

				var enemyRoot = Instantiate(rootConfig.EnemyPrefab, position, Quaternion.identity, container);
				var characterRoot = enemyRoot.transform.Find(CharacterRootName);
				if (characterRoot == null)
				{
					Destroy(enemyRoot);
					Debug.LogError("Enemy root requires a CharacterRoot child.");
					return null;
				}

				if (config.Prefab != null) Instantiate(config.Prefab, characterRoot);
				ConfigureRoot(enemyRoot, config);
				mEnemyIds.Add(enemyRoot, config.Id);
				mConfigs.Add(enemyRoot, config);
				return enemyRoot.transform;
			}

			var enemy = new GameObject("Enemy");
			enemy.transform.SetParent(container, false);
			enemy.transform.position = position;
			var renderer = enemy.AddComponent<SpriteRenderer>();
			renderer.sprite = sPlaceholderSprite ??= Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
			renderer.color = new Color(0.8f, 0.12f, 0.16f);
			enemy.transform.localScale = Vector3.one * 0.55f;
			return enemy.transform;
		}

		public EnemyConfig GetConfig(Transform enemyTransform)
		{
			if (enemyTransform == null) return null;
			return mConfigs.TryGetValue(enemyTransform.gameObject, out var config) ? config : null;
		}

		private static void ConfigureRoot(GameObject enemyRoot, EnemyConfig config)
		{
			var combatEntity = enemyRoot.GetComponent<CombatEntity>();
			if (combatEntity == null) combatEntity = enemyRoot.AddComponent<CombatEntity>();
			combatEntity.Initialize(CombatFaction.Enemy);
			GameArchitecture.Interface.SendCommand(new RegisterEnemyHealthCommand(combatEntity, config.BaseHealth));

			var rigidbody = enemyRoot.GetComponent<Rigidbody2D>();
			if (rigidbody == null) rigidbody = enemyRoot.AddComponent<Rigidbody2D>();
			rigidbody.bodyType = RigidbodyType2D.Kinematic;
			rigidbody.useFullKinematicContacts = true;
			rigidbody.gravityScale = 0f;

			foreach (var attackId in config.AttackIds)
			{
				var attack = GameArchitecture.Interface.GetUtility<AttackCatalog>().Get(attackId);
				var executor = attack == null ? null : GameArchitecture.Interface.GetUtility<AttackExecutorRegistry>().Get(attack.ExecutorId);
				executor?.ConfigureOwner(enemyRoot, attack, CombatFaction.Enemy);
			}
		}

		public void ReleaseAllActive()
		{
			PruneDestroyedRoots();
			foreach (var enemyRoot in new List<GameObject>(mEnemyIds.Keys))
			{
				if (enemyRoot != null && enemyRoot.activeInHierarchy) Release(enemyRoot.transform);
			}
		}

		public void Release(Transform enemyTransform)
		{
			if (enemyTransform == null) return;
			var enemyRoot = enemyTransform.gameObject;
			if (!mEnemyIds.TryGetValue(enemyRoot, out var enemyId))
			{
				Destroy(enemyRoot);
				return;
			}

			if (!mPools.TryGetValue(enemyId, out var pool))
			{
				pool = new Queue<GameObject>();
				mPools.Add(enemyId, pool);
			}

			GameArchitecture.Interface.GetSystem<AttackSystem>().UnregisterOwner(enemyRoot, 0);
			enemyRoot.SetActive(false);
			var container = GetContainer();
			if (container != null) enemyRoot.transform.SetParent(container, false);
			pool.Enqueue(enemyRoot);
		}

		private static Transform GetContainer()
		{
			return WorldRootLocator.Get(WorldRootSlot.Enemy);
		}

		private void PruneDestroyedRoots()
		{
			var destroyedRoots = new List<GameObject>();
			foreach (var root in mEnemyIds.Keys)
			{
				if (root == null) destroyedRoots.Add(root);
			}
			foreach (var root in destroyedRoots)
			{
				mEnemyIds.Remove(root);
				mConfigs.Remove(root);
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
