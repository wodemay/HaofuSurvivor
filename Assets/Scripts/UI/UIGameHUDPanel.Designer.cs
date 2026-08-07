using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	// Generate Id:0325b9d9-61fc-4866-9dc0-905f0a7697a8
	public partial class UIGameHUDPanel
	{
		public const string Name = "UIGameHUDPanel";
		
		[SerializeField]
		public UnityEngine.UI.Text Text_RemainingTime;
		[SerializeField]
		public UnityEngine.UI.Button Button_Back;
		[SerializeField]
		public UnityEngine.UI.Button Button_Pause;
		
		private UIGameHUDPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			Text_RemainingTime = null;
			Button_Back = null;
			Button_Pause = null;
			
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
