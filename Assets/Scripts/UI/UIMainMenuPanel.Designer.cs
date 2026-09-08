using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	// Generate Id:fde2bc9b-93c6-4498-b2a4-3dce673ace80
	public partial class UIMainMenuPanel
	{
		public const string Name = "UIMainMenuPanel";
		
		[SerializeField]
		public UnityEngine.UI.Button Button_ContinueGame;
		[SerializeField]
		public UnityEngine.UI.Button Button_StartGame;
		[SerializeField]
		public UnityEngine.UI.Button Button_MetaUpgrade;
		[SerializeField]
		public UnityEngine.UI.Button Button_Settings;
		[SerializeField]
		public UnityEngine.UI.Button Button_QuitGame;
		[SerializeField]
		public UnityEngine.UI.Text Text_ProfileCoin;
		
		private UIMainMenuPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			Button_ContinueGame = null;
			Button_StartGame = null;
			Button_MetaUpgrade = null;
			Button_Settings = null;
			Button_QuitGame = null;
			Text_ProfileCoin = null;
			
			mData = null;
		}
		
		public UIMainMenuPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		UIMainMenuPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UIMainMenuPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
