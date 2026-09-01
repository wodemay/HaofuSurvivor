using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	// Generate Id:9f4991ae-1a2c-4e20-9880-7cbb322f6962
	public partial class UIRunSettlementPanel
	{
		public const string Name = "UIRunSettlementPanel";

		[SerializeField]
		public UnityEngine.UI.Image Image_Background;
		[SerializeField]
		public UnityEngine.UI.Text Text_Title;
		[SerializeField]
		public UnityEngine.UI.Text Text_Result;
		[SerializeField]
		public UnityEngine.UI.Text Text_Time;
		[SerializeField]
		public UnityEngine.UI.Text Text_Kills;
		[SerializeField]
		public UnityEngine.UI.Text Text_BossKills;
		[SerializeField]
		public UnityEngine.UI.Text Text_Coin;
		[SerializeField]
		public UnityEngine.UI.Button Button_Confirm;

		private UIRunSettlementPanelData mPrivateData = null;

		protected override void ClearUIComponents()
		{
			Image_Background = null;
			Text_Title = null;
			Text_Result = null;
			Text_Time = null;
			Text_Kills = null;
			Text_BossKills = null;
			Text_Coin = null;
			Button_Confirm = null;

			mData = null;
		}

		public UIRunSettlementPanelData Data
		{
			get
			{
				return mData;
			}
		}

		UIRunSettlementPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UIRunSettlementPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
