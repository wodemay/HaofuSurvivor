using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	public class UIGameHUDPanelData : UIPanelData
	{
	}
	public partial class UIGameHUDPanel : UIPanel, IController
	{
		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGameHUDPanelData ?? new UIGameHUDPanelData();
			Button_Back.onClick.AddListener(Back);
			this.RegisterEvent<RunTimerUpdatedEvent>(OnRunTimerUpdated)
				.UnRegisterWhenGameObjectDestroyed(gameObject);
			RefreshTime();
		}
		
		protected override void OnOpen(IUIData uiData = null)
		{
		}
		
		protected override void OnShow()
		{
		}
		
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
			Button_Back.onClick.RemoveListener(Back);
		}

		private void OnRunTimerUpdated(RunTimerUpdatedEvent timerEvent)
		{
			Text_RemainingTime.text = FormatTime(timerEvent.ElapsedSeconds);
		}

		private void RefreshTime()
		{
			var timer = this.SendQuery(new GetRunTimerStateQuery());
			Text_RemainingTime.text = FormatTime(timer.ElapsedSeconds);
		}

		private static string FormatTime(int seconds)
		{
			return $"{seconds / 60:00}:{seconds % 60:00}";
		}
	}
}
