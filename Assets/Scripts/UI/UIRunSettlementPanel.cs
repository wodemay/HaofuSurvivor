	using UnityEngine;
	using UnityEngine.UI;
	using UnityEngine.EventSystems;
	using QFramework;

namespace HaoFuSurvivor
{
	public class UIRunSettlementPanelData : UIPanelData
	{
	}
	public partial class UIRunSettlementPanel : UIPanel, IController
	{
		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIRunSettlementPanelData ?? new UIRunSettlementPanelData();
			Button_Confirm.onClick.AddListener(ReturnToMainMenu);
			if (EventSystem.current != null) EventSystem.current.sendNavigationEvents = false;
		}

		protected override void OnOpen(IUIData uiData = null)
		{
		}

		protected override void OnShow()
		{
			var settlement = this.SendQuery(new GetRunSettlementStateQuery());
			if (!settlement.HasSettlement) return;
			var data = settlement.Data;
			var victory = data.Result == RunPhase.Victory;
			Text_Title.text = victory ? "鑳滃埄" : "澶辫触";
			Text_Result.text = victory ? "Boss 宸茶鍑昏触" : "鏈眬鎸戞垬缁撴潫";
			Text_Time.text = $"瀛樻椿鏃堕棿锛歿data.SurvivalSeconds / 60:00}:{data.SurvivalSeconds % 60:00}";
			Text_Kills.text = $"鏅€氬嚮鏉€锛歿data.NormalKillCount}";
			Text_BossKills.text = $"Boss 鍑绘潃锛歿data.BossKillCount}";
			Text_Coin.text = $"閲戝竵鏁帮細{data.Coins?.ToDisplayString() ?? "0"}";
		}

		protected override void OnHide()
		{
		}

		protected override void OnClose()
		{
			Button_Confirm.onClick.RemoveListener(ReturnToMainMenu);
			if (EventSystem.current != null) EventSystem.current.sendNavigationEvents = true;
		}

		private void ReturnToMainMenu()
		{
			this.SendCommand(new ExitRunToCharacterSelectionCommand());
			CloseSelf();
			UIKit.OpenPanel<UIMainMenuPanel>(assetBundleName: "uimainmenupanel_prefab", prefabName: UIMainMenuPanel.Name);
		}
	}
}
