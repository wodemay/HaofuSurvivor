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
			Text_Title.text = victory ? "胜利" : "失败";
			Text_Result.text = victory ? "Boss 已被击败" : "本局挑战结束";
			Text_Time.text = $"存活时间：{data.SurvivalSeconds / 60:00}:{data.SurvivalSeconds % 60:00}";
			Text_Kills.text = $"普通击杀：{data.NormalKillCount}";
			Text_BossKills.text = $"Boss 击杀：{data.BossKillCount}";
			Text_Coin.text = $"金币数：{data.Coins?.ToDisplayString() ?? "0"}";
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
