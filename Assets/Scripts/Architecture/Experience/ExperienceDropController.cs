using UnityEngine;

namespace HaoFuSurvivor
{
	public class ExperienceDropController : MonoBehaviour
	{
		public float Experience { get; private set; }

		public void Configure(float experience)
		{
			Experience = Mathf.Max(0.01f, experience);
		}

		public void MoveTowards(Vector2 targetPosition, float maxDistanceDelta)
		{
			if (maxDistanceDelta <= 0f) return;
			transform.position = Vector2.MoveTowards(transform.position, targetPosition, maxDistanceDelta);
		}
	}

}
