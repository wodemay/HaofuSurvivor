using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	// Generate Id:070e8a61-cea4-43c6-bf1e-4dba4b9de6a3
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
