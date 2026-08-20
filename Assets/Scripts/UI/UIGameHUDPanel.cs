using UnityEngine;
using UnityEngine.EventSystems;
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
			SetKeyboardNavigation(false);
			Button_Back.onClick.AddListener(ReturnToMainMenu);
			Button_Pause.onClick.AddListener(TogglePause);
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
			Button_Back.onClick.RemoveListener(ReturnToMainMenu);
			Button_Pause.onClick.RemoveListener(TogglePause);
			SetKeyboardNavigation(true);
		}

		private static void SetKeyboardNavigation(bool enabled)
		{
			if (EventSystem.current == null) return;
			EventSystem.current.sendNavigationEvents = enabled;
			if (!enabled) EventSystem.current.SetSelectedGameObject(null);
		}

		private void TogglePause()
		{
			if (this.SendQuery(new GetRunTimeStateQuery()).IsRunning)
			{
				this.SendCommand(new PauseRunCommand());
			}
			else
			{
				this.SendCommand(new ResumeRunCommand());
			}
		}

		private void ReturnToMainMenu()
		{
			this.SendCommand(new ExitRunToCharacterSelectionCommand());
			CloseSelf();
			UIKit.OpenPanel<UIMainMenuPanel>(
				assetBundleName: "uimainmenupanel_prefab",
				prefabName: UIMainMenuPanel.Name);
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
