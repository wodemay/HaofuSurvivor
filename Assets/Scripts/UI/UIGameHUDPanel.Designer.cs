using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	// Generate Id:62f4e676-9d32-42d8-b552-2ac2695efddb
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
		[SerializeField]
		public UnityEngine.UI.Slider Slider_BossHP;
		
		private UIGameHUDPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			Text_RemainingTime = null;
			Button_Back = null;
			Button_Pause = null;
			Text_RunCoin = null;
			Slider_BossHP = null;
			
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
