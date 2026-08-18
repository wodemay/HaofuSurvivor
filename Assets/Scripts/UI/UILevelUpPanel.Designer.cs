using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	// Generate Id:bfcac6d0-8ff5-4f21-bbfe-8413554773b2
	public partial class UILevelUpPanel
	{
		public const string Name = "UILevelUpPanel";
		
		[SerializeField]
		public UnityEngine.UI.Text Text_Level;
		[SerializeField]
		public UnityEngine.GameObject UILevelUpOptionItemTemplate;
		
		private UILevelUpPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			Text_Level = null;
			UILevelUpOptionItemTemplate = null;
			
			mData = null;
		}
		
		public UILevelUpPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		UILevelUpPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UILevelUpPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
