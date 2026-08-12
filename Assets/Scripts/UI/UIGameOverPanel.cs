using UnityEngine;
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
			Button_Restart.onClick.AddListener(Restart);
			Button_Return.onClick.AddListener(ReturnToCharacterSelection);
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
			Button_Return.onClick.RemoveListener(ReturnToCharacterSelection);
		}

		private void Restart()
		{
			this.SendCommand(new RestartSelectedCharacterRunCommand());
			CloseSelf();
		}

		private void ReturnToCharacterSelection()
		{
			this.SendCommand(new ExitRunToCharacterSelectionCommand());
			CloseSelf();
			UIKit.OpenPanel<UICharacterSelectPanel>(
				assetBundleName: "uicharacterselectpanel_prefab",
				prefabName: UICharacterSelectPanel.Name);
		}
	}
}
