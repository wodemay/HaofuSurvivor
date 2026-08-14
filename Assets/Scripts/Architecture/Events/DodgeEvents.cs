namespace HaoFuSurvivor
{
	public struct DodgeStartedEvent
	{
		public readonly int DodgeId;
		public readonly int Level;
		public DodgeStartedEvent(int dodgeId, int level) { DodgeId = dodgeId; Level = level; }
	}

	public struct DodgeEndedEvent
	{
		public readonly int DodgeId;
		public readonly int Level;
		public DodgeEndedEvent(int dodgeId, int level) { DodgeId = dodgeId; Level = level; }
	}

	public struct DodgeUpgradedEvent
	{
		public readonly int DodgeId;
		public readonly int Level;
		public DodgeUpgradedEvent(int dodgeId, int level) { DodgeId = dodgeId; Level = level; }
	}
}
