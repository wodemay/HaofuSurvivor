using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	[RequireComponent(typeof(Rigidbody2D))]
	public class PlayerController : MonoBehaviour, IController
	{
		private Rigidbody2D mRigidbody;

		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		private void Awake()
		{
			mRigidbody = GetComponent<Rigidbody2D>();
			var selectedCharacterId = this.GetModel<CharacterSelectionModel>().SelectedCharacterId;
			var character = this.GetUtility<CharacterCatalog>().Get(selectedCharacterId);
			this.SendCommand(new RegisterPlayerCommand(transform.position, character));
		}

		private void Update()
		{
			if (!this.SendQuery(new GetRunTimeStateQuery()).IsRunning)
			{
				this.SendCommand(new SetMovementInputCommand(Vector2.zero));
				return;
			}

			var movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
			this.SendCommand(new SetMovementInputCommand(movement));
		}

		private void FixedUpdate()
		{
			if (!this.SendQuery(new GetRunTimeStateQuery()).IsRunning) return;

			this.SendCommand<MovePlayerCommand>();
			mRigidbody.MovePosition(this.GetModel<PlayerModel>().Position);
		}

		private void OnDestroy()
		{
			if (GameArchitecture.Interface.GetModel<PlayerModel>().IsRegistered)
			{
				this.SendCommand<UnregisterPlayerCommand>();
			}
		}
	}
}
