using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class InputSystem : AbstractSystem, IRunUpdateable
	{
		public void SetMovement(Vector2 movement)
		{
			this.GetModel<InputModel>().Movement = Vector2.ClampMagnitude(movement, 1f);
			if (movement.sqrMagnitude > 0.001f) this.GetModel<InputModel>().LastMovementDirection = movement.normalized;
		}

		public void Clear()
		{
			this.GetModel<InputModel>().Movement = Vector2.zero;
		}

		public void OnRunUpdate(float deltaTime)
		{
			var movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
			SetMovement(movement);
			if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
				GameArchitecture.Interface.SendCommand(new RequestDodgeCommand());
			if (Input.GetKeyDown(KeyCode.Space))
				GameArchitecture.Interface.SendCommand(new RequestSkillCommand());
		}

		protected override void OnInit()
		{
		}
	}
}
