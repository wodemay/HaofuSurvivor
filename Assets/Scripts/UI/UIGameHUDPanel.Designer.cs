using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	// Generate Id:45a21b55-09e0-4cee-8f13-ac01976461f1
	public partial class UIGameHUDPanel
	{
		public const string Name = "UIGameHUDPanel";
		
		[SerializeField]
		public UnityEngine.UI.Text Text_RemainingTime;
		[SerializeField]
		public UnityEngine.UI.Button Button_Back;
		
		private UIGameHUDPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			Text_RemainingTime = null;
			Button_Back = null;
			
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
