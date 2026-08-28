using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	public class UIPopPanelData : UIPanelData
	{
	}
	public partial class UIPopPanel : UIPanel, IController
	{
		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIPopPanelData ?? new UIPopPanelData();
			Button_OK.onClick.AddListener(Confirm);
			Button_Cancel.onClick.AddListener(Cancel);
		}

		protected override void OnOpen(IUIData uiData = null)
		{
			var request = GameArchitecture.Interface.GetSystem<UIPopPanelSystem>().CurrentRequest;
			if (request == null)
			{
				CloseSelf();
				return;
			}

			Text_Title.text = request.Title;
			Text_Title.gameObject.SetActive(!string.IsNullOrEmpty(request.Title));
			Text_Content.text = request.Content;
			Text_Content.gameObject.SetActive(!string.IsNullOrEmpty(request.Content));
			Button_OK.GetComponentInChildren<Text>(true).text = request.OkText;
			Button_OK.gameObject.SetActive(!string.IsNullOrEmpty(request.OkText));
			Button_Cancel.GetComponentInChildren<Text>(true).text = request.CancelText;
			Button_Cancel.gameObject.SetActive(!string.IsNullOrEmpty(request.CancelText));
		}

		protected override void OnShow()
		{
		}

		protected override void OnHide()
		{
		}

		protected override void OnClose()
		{
			Button_OK.onClick.RemoveListener(Confirm);
			Button_Cancel.onClick.RemoveListener(Cancel);
			GameArchitecture.Interface.GetSystem<UIPopPanelSystem>().AcknowledgeClosed();
		}

		private void Confirm()
		{
			this.SendCommand(new CompleteUIPopPanelCommand(true));
			CloseSelf();
		}

		private void Cancel()
		{
			this.SendCommand(new CompleteUIPopPanelCommand(false));
			CloseSelf();
		}
	}
}
