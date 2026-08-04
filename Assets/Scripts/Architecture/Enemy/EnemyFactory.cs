using UnityEngine;
using System.Collections.Generic;
namespace HaoFuSurvivor
{
	public class EnemyFactory : MonoBehaviour
	{
		private const string EnemyRootConfigPath = "Configs/EnemyRoot";
		private const string CharacterRootName = "CharacterRoot";
		private const string EnemyContainerName = "EnemyContainer";
		private static EnemyFactory sInstance;
		private static Sprite sPlaceholderSprite;
		private readonly Dictionary<int, Queue<GameObject>> mPools = new();
		private readonly Dictionary<GameObject, int> mEnemyIds = new();
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
			var rootConfig = Resources.Load<EnemyRootConfig>(EnemyRootConfigPath);
			if (rootConfig != null && rootConfig.EnemyPrefab != null)
			{
				if (mPools.TryGetValue(config.Id, out var pool) && pool.Count > 0)
				{
					var pooledRoot = pool.Dequeue();
					pooledRoot.transform.SetParent(GetContainer(), false);
					pooledRoot.transform.SetPositionAndRotation(position, Quaternion.identity);
					pooledRoot.SetActive(true);
					ConfigureRoot(pooledRoot, config);
					return pooledRoot.transform;
				}

				var enemyRoot = Instantiate(rootConfig.EnemyPrefab, position, Quaternion.identity, GetContainer());
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
				return enemyRoot.transform;
			}

			var enemy = new GameObject("Enemy");
			enemy.transform.position = position;
			var renderer = enemy.AddComponent<SpriteRenderer>();
			renderer.sprite = sPlaceholderSprite ??= Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
			renderer.color = new Color(0.8f, 0.12f, 0.16f);
			enemy.transform.localScale = Vector3.one * 0.55f;
			return enemy.transform;
		}

		private static void ConfigureRoot(GameObject enemyRoot, EnemyConfig config)
		{
			var rigidbody = enemyRoot.GetComponent<Rigidbody2D>();
			if (rigidbody == null) rigidbody = enemyRoot.AddComponent<Rigidbody2D>();
			rigidbody.bodyType = RigidbodyType2D.Kinematic;
			rigidbody.gravityScale = 0f;

			foreach (var attackId in config.AttackIds)
			{
				var attack = GameArchitecture.Interface.GetUtility<EnemyAttackCatalog>().Get(attackId);
				if (attack == null || attack.AttackType != EnemyAttackType.Contact) continue;
				var contactAttack = enemyRoot.GetComponent<EnemyContactAttack>();
				if (contactAttack == null) contactAttack = enemyRoot.AddComponent<EnemyContactAttack>();
				contactAttack.Initialize(attackId);
				break;
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

			enemyRoot.SetActive(false);
			enemyRoot.transform.SetParent(GetContainer(), false);
			pool.Enqueue(enemyRoot);
		}

		private static Transform GetContainer()
		{
			var container = GameObject.Find(EnemyContainerName);
			if (container != null) return container.transform;
			return new GameObject(EnemyContainerName).transform;
		}
		private void Awake()
		{
			if (sInstance != null && sInstance != this) { Destroy(gameObject); return; }
			sInstance = this;
			DontDestroyOnLoad(gameObject);
		}
	}
}
