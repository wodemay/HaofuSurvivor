using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class InputSystem : AbstractSystem
	{
		public void SetMovement(Vector2 movement)
		{
			this.GetModel<InputModel>().Movement = Vector2.ClampMagnitude(movement, 1f);
		}

		protected override void OnInit()
		{
		}
	}
}
