using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	public class UILevelUpPanelData : UIPanelData
	{
	}
	public partial class UILevelUpPanel : UIPanel, IController
	{
		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UILevelUpPanelData ?? new UILevelUpPanelData();
		}
		
		protected override void OnOpen(IUIData uiData = null)
		{
			RefreshOptions();
		}
		
		protected override void OnShow()
		{
		}
		
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
		}

		private void RefreshOptions()
		{
			var state = this.SendQuery(new GetLevelUpStateQuery());
			if (state.PendingSelectionCount <= 0)
			{
				CloseSelf();
				return;
			}
			Text_Level.text = $"Level{state.CurrentLevel - 1}->Level{state.CurrentLevel}";
			var parent = UILevelUpOptionItemTemplate.transform.parent;
			for (var index = parent.childCount - 1; index >= 0; index--)
			{
				var child = parent.GetChild(index).gameObject;
				if (child != UILevelUpOptionItemTemplate) Destroy(child);
			}
			foreach (var option in this.SendQuery(new GetLevelUpWeaponOptionsQuery()))
			{
				var itemObject = Instantiate(UILevelUpOptionItemTemplate, parent);
				itemObject.SetActive(true);
				var item = itemObject.GetComponent<UILevelUpOptionItem>();
				if (item == null) item = itemObject.AddComponent<UILevelUpOptionItem>();
				item.Initialize(option, SelectWeapon);
			}
		}

		private void SelectWeapon(LevelUpWeaponOption option)
		{
			this.SendCommand(new CompleteLevelUpWeaponCommand(option.RuntimeId, option.IsEvolution));
			if (this.SendQuery(new GetLevelUpStateQuery()).PendingSelectionCount > 0)
			{
				RefreshOptions();
			}
			else
			{
				CloseSelf();
			}
		}
	}
}
