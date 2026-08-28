namespace HaoFuSurvivor
{
	public struct ProfileLoadCompletedEvent
	{
		public readonly bool RequiresNotice;
		public readonly string Message;

		public ProfileLoadCompletedEvent(bool requiresNotice, string message)
		{
			RequiresNotice = requiresNotice;
			Message = message;
		}
	}

	public struct ProfileCoinsChangedEvent
	{
		public readonly BigCoin Coins;

		public ProfileCoinsChangedEvent(BigCoin coins)
		{
			Coins = coins;
		}
	}
}
