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
			if (Input.GetKeyDown(KeyCode.Space)) this.SendCommand<RequestDodgeCommand>();
		}

		private void FixedUpdate()
		{
			if (!this.SendQuery(new GetRunTimeStateQuery()).IsRunning) return;

			this.SendCommand<MovePlayerCommand>();
			mRigidbody.MovePosition(this.GetModel<PlayerModel>().Position);
		}

		private void OnDestroy()
		{
			this.SendCommand(new UnregisterPlayerCommand(gameObject));
		}
	}
}
