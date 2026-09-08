using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	public class UIMetaUpgradePanelData : UIPanelData
	{
	}
	public partial class UIMetaUpgradePanel : UIPanel, IController
	{
		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UIMetaUpgradePanelData ?? new UIMetaUpgradePanelData();
			Button_Back.onClick.AddListener(BackToMainMenu);
			this.RegisterEvent<ProfileCoinsChangedEvent>(_ => Refresh())
				.UnRegisterWhenGameObjectDestroyed(gameObject);
			this.RegisterEvent<MetaUpgradeChangedEvent>(_ => Refresh())
				.UnRegisterWhenGameObjectDestroyed(gameObject);
		}

		protected override void OnOpen(IUIData uiData = null)
		{
			Refresh();
		}

		protected override void OnShow()
		{
		}

		protected override void OnHide()
		{
		}

		protected override void OnClose()
		{
			Button_Back.onClick.RemoveListener(BackToMainMenu);
		}

		private void Refresh()
		{
			var meta = this.GetSystem<MetaUpgradeSystem>();
			var profile = this.SendQuery(new GetProfileStateQuery());
			if (Text_Coin != null) Text_Coin.text = $"金币数：{profile.ProfileCoin.ToDisplayString()}";
			var parent = UpgradeItem.transform.parent;
			for (var index = parent.childCount - 1; index >= 0; index--)
			{
				var child = parent.GetChild(index);
				if (child != null && child.gameObject != UpgradeItem) Destroy(child.gameObject);
			}
			if (meta.Definitions == null) return;
			foreach (var definition in meta.Definitions)
			{
				if (definition == null) continue;
				var itemObject = Instantiate(UpgradeItem, parent);
				itemObject.SetActive(true);
				var item = itemObject.GetComponent<UIMetaUpgradeItem>() ?? itemObject.AddComponent<UIMetaUpgradeItem>();
				var id = definition.Id;
				item.Initialize(definition, meta.GetLevel(id), profile.ProfileCoin, () => this.SendCommand(new UpgradeMetaUpgradeCommand(id)));
			}
		}

		private void BackToMainMenu()
		{
			CloseSelf();
			UIKit.OpenPanel<UIMainMenuPanel>(assetBundleName: "uimainmenupanel_prefab", prefabName: UIMainMenuPanel.Name);
		}
	}
}
