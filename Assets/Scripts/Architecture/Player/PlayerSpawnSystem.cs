using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class PlayerSpawnSystem : AbstractSystem
	{
		private const string PlayerRootConfigPath = "Configs/Player/PlayerRoot";
		private const string CharacterRootName = "CharacterRoot";
		private const string HealthBarAnchorName = "HealthBarAnchor";
		private GameObject mCurrentPlayer;

		public bool SpawnSelectedCharacter()
		{
			if (this.GetModel<PlayerModel>().IsRegistered) return true;

			var selectedCharacterId = this.GetModel<CharacterSelectionModel>().SelectedCharacterId;
			var character = this.GetUtility<CharacterCatalog>().Get(selectedCharacterId);
			var rootConfig = Resources.Load<PlayerRootConfig>(PlayerRootConfigPath);
			if (character == null || character.PlayerPrefab == null || rootConfig == null || rootConfig.PlayerPrefab == null)
			{
				Debug.LogError("Player spawn requires a selected character and Resources/Configs/Player/PlayerRoot configuration.");
				return false;
			}

			var playerLayerRoot = WorldRootLocator.Get(WorldRootSlot.Player);
			if (playerLayerRoot == null) return false;
			var playerObject = Object.Instantiate(rootConfig.PlayerPrefab, Vector3.zero, Quaternion.identity, playerLayerRoot);
			var characterRoot = playerObject.transform.Find(CharacterRootName);
			var healthBarAnchor = playerObject.transform.Find(HealthBarAnchorName);
			if (characterRoot == null || healthBarAnchor == null)
			{
				Object.Destroy(playerObject);
				Debug.LogError("Player root requires CharacterRoot and HealthBarAnchor children.");
				return false;
			}

			EnsurePlayerComponents(playerObject);
			var characterObject = Object.Instantiate(character.PlayerPrefab, characterRoot);
			var legacyController = characterObject.GetComponent<PlayerController>();
			if (legacyController != null) Object.Destroy(legacyController);
			this.GetSystem<PlayerSystem>().Register(playerObject, playerObject.transform.position, character);
			var loadoutResult = this.GetSystem<PlayerLoadoutSystem>().EquipInitialSkillGroup(playerObject, character.SkillGroupId);
			if (!loadoutResult.CoreSucceeded)
			{
				this.GetSystem<PlayerSystem>().Unregister(playerObject);
				Object.Destroy(playerObject);
				Debug.LogError($"Player spawn failed because skill group {character.SkillGroupId} could not be equipped.");
				return false;
			}
			if (!loadoutResult.DodgeEquipped && character.SkillGroupId != 0)
				Debug.LogWarning($"Character {character.Id} started without dodge.");
			if (rootConfig.HealthBarPrefab != null)
			{
				var healthBar = Object.Instantiate(rootConfig.HealthBarPrefab, healthBarAnchor);
				if (healthBar.GetComponent<PlayerHealthBarView>() == null) healthBar.AddComponent<PlayerHealthBarView>();
			}

			BindCamera(playerObject.transform);
			mCurrentPlayer = playerObject;
			return true;
		}

		public void DespawnCurrentCharacter()
		{
			if (mCurrentPlayer != null)
			{
				this.GetSystem<PlayerSystem>().Unregister(mCurrentPlayer);
				mCurrentPlayer.SetActive(false);
				Object.Destroy(mCurrentPlayer);
			}
			mCurrentPlayer = null;
		}

		private static void EnsurePlayerComponents(GameObject playerObject)
		{
			var combatEntity = playerObject.GetComponent<CombatEntity>();
			if (combatEntity == null) combatEntity = playerObject.AddComponent<CombatEntity>();
			combatEntity.Initialize(CombatFaction.Player);

			var rigidbody = playerObject.GetComponent<Rigidbody2D>();
			if (rigidbody == null) rigidbody = playerObject.AddComponent<Rigidbody2D>();
			rigidbody.bodyType = RigidbodyType2D.Kinematic;
			rigidbody.useFullKinematicContacts = true;
			rigidbody.gravityScale = 0f;
			if (HasPhysicalCollider(playerObject)) return;
			Debug.LogError("PlayerRoot requires a non-trigger Collider2D for map collision.");
		}

		private static bool HasPhysicalCollider(GameObject playerObject)
		{
			foreach (var collider in playerObject.GetComponentsInChildren<Collider2D>(true))
				if (collider != null && !collider.isTrigger) return true;
			return false;
		}

		private static void BindCamera(Transform playerTransform)
		{
			var mainCamera = Camera.main;
			if (mainCamera == null)
			{
				Debug.LogError("Player camera binding requires a MainCamera-tagged Camera.");
				return;
			}

			var cameraFollow = mainCamera.GetComponent<CameraFollow>();
			if (cameraFollow == null) cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
			cameraFollow.Bind(playerTransform);
		}

		protected override void OnInit()
		{
		}
	}
}
