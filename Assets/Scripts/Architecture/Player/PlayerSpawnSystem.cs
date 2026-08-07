using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class PlayerSpawnSystem : AbstractSystem
	{
		private const string PlayerRootConfigPath = "Configs/PlayerRoot";
		private const string CharacterRootName = "CharacterRoot";
		private const string HealthBarAnchorName = "HealthBarAnchor";

		public bool SpawnSelectedCharacter()
		{
			if (this.GetModel<PlayerModel>().IsRegistered) return true;

			var selectedCharacterId = this.GetModel<CharacterSelectionModel>().SelectedCharacterId;
			var character = this.GetUtility<CharacterCatalog>().Get(selectedCharacterId);
			var rootConfig = Resources.Load<PlayerRootConfig>(PlayerRootConfigPath);
			if (character == null || character.PlayerPrefab == null || rootConfig == null || rootConfig.PlayerPrefab == null)
			{
				Debug.LogError("Player spawn requires a selected character and Resources/Configs/PlayerRoot configuration.");
				return false;
			}

			var playerObject = Object.Instantiate(rootConfig.PlayerPrefab, Vector3.zero, Quaternion.identity);
			var characterRoot = playerObject.transform.Find(CharacterRootName);
			var healthBarAnchor = playerObject.transform.Find(HealthBarAnchorName);
			if (characterRoot == null || healthBarAnchor == null)
			{
				Object.Destroy(playerObject);
				Debug.LogError("Player root requires CharacterRoot and HealthBarAnchor children.");
				return false;
			}

			EnsurePlayerComponents(playerObject);
			Object.Instantiate(character.PlayerPrefab, characterRoot);
			if (!this.GetSystem<PlayerLoadoutSystem>().EquipInitialSkillGroup(playerObject, character.SkillGroupId))
			{
				this.GetSystem<PlayerSystem>().Unregister();
				Object.Destroy(playerObject);
				Debug.LogError($"Player spawn failed because skill group {character.SkillGroupId} could not be equipped.");
				return false;
			}
			if (rootConfig.HealthBarPrefab != null)
			{
				var healthBar = Object.Instantiate(rootConfig.HealthBarPrefab, healthBarAnchor);
				if (healthBar.GetComponent<PlayerHealthBarView>() == null) healthBar.AddComponent<PlayerHealthBarView>();
			}

			BindCamera(playerObject.transform);
			return true;
		}

		private static void EnsurePlayerComponents(GameObject playerObject)
		{
			var combatEntity = playerObject.GetComponent<CombatEntity>();
			if (combatEntity == null) combatEntity = playerObject.AddComponent<CombatEntity>();
			combatEntity.Initialize(CombatFaction.Player);

			var rigidbody = playerObject.GetComponent<Rigidbody2D>();
			if (rigidbody == null) rigidbody = playerObject.AddComponent<Rigidbody2D>();
			rigidbody.bodyType = RigidbodyType2D.Kinematic;
			rigidbody.gravityScale = 0f;
			if (playerObject.GetComponent<PlayerController>() == null) playerObject.AddComponent<PlayerController>();
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
