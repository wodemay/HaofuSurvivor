using UnityEngine;

namespace HaoFuSurvivor
{
	public class InputModel : QFramework.AbstractModel
	{
		public Vector2 Movement { get; internal set; }

		protected override void OnInit()
		{
			Movement = Vector2.zero;
		}
	}
}
