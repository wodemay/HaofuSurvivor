using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	// Generate Id:225888d6-7067-4b76-8895-6e2a1679db0d
	public partial class UIMetaUpgradePanel
	{
		public const string Name = "UIMetaUpgradePanel";

		[SerializeField]
		public UnityEngine.GameObject Content;
		[SerializeField]
		public UnityEngine.GameObject UpgradeItem;
		[SerializeField]
		public UnityEngine.UI.Button Button_Back;
		[SerializeField]
		public UnityEngine.UI.Text Text_Coin;

		private UIMetaUpgradePanelData mPrivateData = null;

		protected override void ClearUIComponents()
		{
			Content = null;
			UpgradeItem = null;
			Button_Back = null;
			Text_Coin = null;

			mData = null;
		}

		public UIMetaUpgradePanelData Data
		{
			get
			{
				return mData;
			}
		}

		UIMetaUpgradePanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UIMetaUpgradePanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
