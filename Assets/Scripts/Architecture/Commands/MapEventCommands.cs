using QFramework;

namespace HaoFuSurvivor
{
	public class SetMapEventPlayerPresenceCommand : AbstractCommand
	{
		private readonly string mStableId;
		private readonly bool mIsInside;

		public SetMapEventPlayerPresenceCommand(string stableId, bool isInside)
		{
			mStableId = stableId;
			mIsInside = isInside;
		}

		protected override void OnExecute()
		{
			this.GetSystem<MapEventSystem>().SetPlayerPresence(mStableId, mIsInside);
		}
	}
}
