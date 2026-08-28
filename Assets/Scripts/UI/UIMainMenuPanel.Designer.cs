using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	// Generate Id:03ce0587-4938-41cd-bc77-efd927a7990c
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
		[SerializeField]
		public UnityEngine.UI.Text Text_ProfileCoin;
		
		private UIMainMenuPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			Button_ContinueGame = null;
			Button_StartGame = null;
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
