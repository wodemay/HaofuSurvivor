using UnityEngine;

namespace HaoFuSurvivor
{
	public class DodgeRuntimeData
	{
		public int DodgeId { get; internal set; }
		public int Level { get; internal set; }
		public float CooldownRemaining { get; internal set; }
		public float DurationRemaining { get; internal set; }
		public Vector2 Direction { get; internal set; }
		public bool IsActive { get; internal set; }

		public DodgeRuntimeData(int dodgeId, int level = 1)
		{
			DodgeId = dodgeId;
			Level = Mathf.Max(1, level);
		}
	}
}
