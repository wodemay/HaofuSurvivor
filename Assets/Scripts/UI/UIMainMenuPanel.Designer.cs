using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	// Generate Id:a7c1268d-7c5b-49cc-8544-a024f72adb9f
	public partial class UIMainMenuPanel
	{
		public const string Name = "UIMainMenuPanel";
		
		[SerializeField]
		public UnityEngine.UI.Button Button_ContinueGame;
		[SerializeField]
		public UnityEngine.UI.Button Button_StartGame;
		[SerializeField]
		public UnityEngine.UI.Button Button_Settings;
		[SerializeField]
		public UnityEngine.UI.Button Button_QuitGame;
		
		private UIMainMenuPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			Button_ContinueGame = null;
			Button_StartGame = null;
			Button_Settings = null;
			Button_QuitGame = null;
			
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
