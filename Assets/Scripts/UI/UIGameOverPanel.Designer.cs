using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	// Generate Id:c49d6f43-684d-4399-affe-41ef04df7f9c
	public partial class UIGameOverPanel
	{
		public const string Name = "UIGameOverPanel";
		
		[SerializeField]
		public UnityEngine.UI.Button Button_Restart;
		[SerializeField]
		public UnityEngine.UI.Button Button_Return;
		
		private UIGameOverPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			Button_Restart = null;
			Button_Return = null;
			
			mData = null;
		}
		
		public UIGameOverPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		UIGameOverPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UIGameOverPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
