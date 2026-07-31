using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class PlayerSpawnSystem : AbstractSystem
	{
		public bool SpawnSelectedCharacter()
		{
			if (this.GetModel<PlayerModel>().IsRegistered) return true;

			var selectedCharacterId = this.GetModel<CharacterSelectionModel>().SelectedCharacterId;
			var character = this.GetUtility<CharacterCatalog>().Get(selectedCharacterId);
			if (character == null || character.PlayerPrefab == null)
			{
				Debug.LogError($"Character '{selectedCharacterId}' has no PlayerPrefab assigned.");
				return false;
			}

			var playerObject = Object.Instantiate(character.PlayerPrefab, Vector3.zero, Quaternion.identity);
			if (playerObject.GetComponent<PlayerController>() != null)
			{
				BindCamera(playerObject.transform);
				return true;
			}

			Object.Destroy(playerObject);
			Debug.LogError($"Character '{selectedCharacterId}' PlayerPrefab requires PlayerController.");
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
