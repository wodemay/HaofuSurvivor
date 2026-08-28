using QFramework;

namespace HaoFuSurvivor
{
	public class RequestUIPopPanelCommand : AbstractCommand
	{
		private readonly UIPopPanelRequest mRequest;

		public RequestUIPopPanelCommand(UIPopPanelRequest request)
		{
			mRequest = request;
		}

		protected override void OnExecute()
		{
			this.GetSystem<UIPopPanelSystem>().Enqueue(mRequest);
		}
	}

	public class CompleteUIPopPanelCommand : AbstractCommand
	{
		private readonly bool mConfirmed;

		public CompleteUIPopPanelCommand(bool confirmed)
		{
			mConfirmed = confirmed;
		}

		protected override void OnExecute()
		{
			this.GetSystem<UIPopPanelSystem>().CompleteCurrent(mConfirmed);
		}
	}
}
