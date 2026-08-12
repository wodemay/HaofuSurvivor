using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	// Generate Id:05cd06a1-a5af-45c9-9dec-c685c341fc06
	public partial class UIMainMenuPanel
	{
		public const string Name = "UIMainMenuPanel";
		
		
		private UIMainMenuPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			
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
