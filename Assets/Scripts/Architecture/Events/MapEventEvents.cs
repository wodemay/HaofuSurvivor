namespace HaoFuSurvivor
{
	public readonly struct MapEventChangedEvent
	{
		public readonly MapEventState State;

		public MapEventChangedEvent(MapEventState state)
		{
			State = state;
		}
	}
}
