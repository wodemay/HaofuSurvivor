using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	public class UIMainMenuPanelData : UIPanelData
	{
	}
	public partial class UIMainMenuPanel : UIPanel, IController
	{
		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIMainMenuPanelData ?? new UIMainMenuPanelData();
			Button_StartGame.onClick.AddListener(StartGame);
			Button_ContinueGame.onClick.AddListener(ContinueGame);
			Button_QuitGame.onClick.AddListener(QuitGame);
		}
		
		protected override void OnOpen(IUIData uiData = null)
		{
		}
		
		protected override void OnShow()
		{
			Button_ContinueGame.gameObject.SetActive(this.SendQuery(new HasSavedRunQuery()).HasSave);
			var profile = this.SendQuery(new GetProfileStateQuery());
			Text_ProfileCoin.text = $"金币数：{profile.ProfileCoin.ToDisplayString()}";
		}
		
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
			Button_StartGame.onClick.RemoveListener(StartGame);
			Button_ContinueGame.onClick.RemoveListener(ContinueGame);
			Button_QuitGame.onClick.RemoveListener(QuitGame);
		}

		private void StartGame()
		{
			CloseSelf();
			UIKit.OpenPanel<UICharacterSelectPanel>(
				assetBundleName: "uicharacterselectpanel_prefab",
				prefabName: UICharacterSelectPanel.Name);
		}

		private void ContinueGame()
		{
			if (!this.SendQuery(new HasSavedRunQuery()).HasSave) return;
			CloseSelf();
			this.SendCommand<ContinueSavedRunCommand>();
		}

		private void QuitGame()
		{
			Application.Quit();
		}
	}
}
