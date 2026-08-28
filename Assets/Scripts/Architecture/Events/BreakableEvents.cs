using UnityEngine;

namespace HaoFuSurvivor
{
	public readonly struct BreakableObjectDestroyedEvent
	{
		public readonly int ConfigId;
		public readonly int DropTableId;
		public readonly Vector2 Position;

		public BreakableObjectDestroyedEvent(int configId, int dropTableId, Vector2 position)
		{
			ConfigId = configId;
			DropTableId = dropTableId;
			Position = position;
		}
	}
}
