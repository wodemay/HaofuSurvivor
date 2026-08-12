using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	public class UIMainMenuPanelData : UIPanelData
	{
	}
	public partial class UIMainMenuPanel : UIPanel
	{
		private Button mStartGameButton;
		private Button mQuitGameButton;

		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIMainMenuPanelData ?? new UIMainMenuPanelData();
			mStartGameButton = transform.Find("BG/Button_StartGame").GetComponent<Button>();
			mQuitGameButton = transform.Find("BG/Button_QuitGame").GetComponent<Button>();
			mStartGameButton.onClick.AddListener(StartGame);
			mQuitGameButton.onClick.AddListener(QuitGame);
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
			mStartGameButton.onClick.RemoveListener(StartGame);
			mQuitGameButton.onClick.RemoveListener(QuitGame);
		}

		private void StartGame()
		{
			CloseSelf();
			UIKit.OpenPanel<UICharacterSelectPanel>(
				assetBundleName: "uicharacterselectpanel_prefab",
				prefabName: UICharacterSelectPanel.Name);
		}

		private void QuitGame()
		{
			Application.Quit();
		}
	}
}
