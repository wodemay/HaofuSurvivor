using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	// Generate Id:6c20289f-9ead-414e-b742-0e806b8255a8
	public partial class UIGameHUDPanel
	{
		public const string Name = "UIGameHUDPanel";
		
		[SerializeField]
		public UnityEngine.UI.Text Text_RemainingTime;
		[SerializeField]
		public UnityEngine.UI.Button Button_Back;
		[SerializeField]
		public UnityEngine.UI.Button Button_Pause;
		[SerializeField]
		public UnityEngine.UI.Text Text_RunCoin;
		
		private UIGameHUDPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			Text_RemainingTime = null;
			Button_Back = null;
			Button_Pause = null;
			Text_RunCoin = null;
			
			mData = null;
		}
		
		public UIGameHUDPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		UIGameHUDPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UIGameHUDPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
