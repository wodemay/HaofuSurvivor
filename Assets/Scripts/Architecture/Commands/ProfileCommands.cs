using QFramework;

namespace HaoFuSurvivor
{
	public class AddProfileCoinsCommand : AbstractCommand
	{
		private readonly BigCoin mAmount;

		public AddProfileCoinsCommand(BigCoin amount)
		{
			mAmount = amount;
		}

		protected override void OnExecute()
		{
			this.GetSystem<ProfileSystem>().AddCoins(mAmount);
		}
	}

	public class SpendProfileCoinsCommand : AbstractCommand
	{
		private readonly BigCoin mAmount;

		public SpendProfileCoinsCommand(BigCoin amount)
		{
			mAmount = amount;
		}

		protected override void OnExecute()
		{
			this.GetSystem<ProfileSystem>().TrySpendCoins(mAmount);
		}
	}

	public class AddRunCoinsCommand : AbstractCommand
	{
		private readonly BigCoin mAmount;

		public AddRunCoinsCommand(BigCoin amount)
		{
			mAmount = amount;
		}

		protected override void OnExecute()
		{
			this.GetSystem<RunEconomySystem>().AddCoins(mAmount);
		}
	}
}
