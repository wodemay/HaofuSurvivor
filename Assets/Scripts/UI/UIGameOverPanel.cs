using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	public class UIGameOverPanelData : UIPanelData
	{
	}
	public partial class UIGameOverPanel : UIPanel, IController
	{
		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIGameOverPanelData ?? new UIGameOverPanelData();
			SetKeyboardNavigation(false);
			Button_Restart.onClick.AddListener(Restart);
			Button_Return.onClick.AddListener(ReturnToMainMenu);
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
			Button_Restart.onClick.RemoveListener(Restart);
			Button_Return.onClick.RemoveListener(ReturnToMainMenu);
			SetKeyboardNavigation(true);
		}

		private static void SetKeyboardNavigation(bool enabled)
		{
			if (EventSystem.current == null) return;
			EventSystem.current.sendNavigationEvents = enabled;
			if (!enabled) EventSystem.current.SetSelectedGameObject(null);
		}

		private void Restart()
		{
			this.SendCommand(new RestartSelectedCharacterRunCommand());
			CloseSelf();
		}

		private void ReturnToMainMenu()
		{
			this.SendCommand(new ExitRunToCharacterSelectionCommand());
			CloseSelf();
			UIKit.OpenPanel<UIMainMenuPanel>(
				assetBundleName: "uimainmenupanel_prefab",
				prefabName: UIMainMenuPanel.Name);
		}
	}
}
