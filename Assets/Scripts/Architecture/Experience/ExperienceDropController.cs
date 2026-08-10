using UnityEngine;

namespace HaoFuSurvivor
{
	public class ExperienceDropController : MonoBehaviour
	{
		public int Experience { get; private set; }

		public void Configure(int experience)
		{
			Experience = Mathf.Max(1, experience);
		}

		public void MoveTowards(Vector2 targetPosition, float maxDistanceDelta)
		{
			if (maxDistanceDelta <= 0f) return;
			transform.position = Vector2.MoveTowards(transform.position, targetPosition, maxDistanceDelta);
		}
	}

}
