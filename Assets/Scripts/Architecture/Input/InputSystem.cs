using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class InputSystem : AbstractSystem
	{
		public void SetMovement(Vector2 movement)
		{
			this.GetModel<InputModel>().Movement = Vector2.ClampMagnitude(movement, 1f);
			if (movement.sqrMagnitude > 0.001f) this.GetModel<InputModel>().LastMovementDirection = movement.normalized;
		}

		protected override void OnInit()
		{
		}
	}
}
