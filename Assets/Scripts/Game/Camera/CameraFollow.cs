using UnityEngine;

namespace HaoFuSurvivor
{
	public class CameraFollow : MonoBehaviour
	{
		private Transform mTarget;
		private float mZPosition;

		public void Bind(Transform target)
		{
			mTarget = target;
			mZPosition = transform.position.z;
		}

		private void LateUpdate()
		{
			if (mTarget == null) return;

			var position = mTarget.position;
			position.z = mZPosition;
			transform.position = position;
		}
	}
}
