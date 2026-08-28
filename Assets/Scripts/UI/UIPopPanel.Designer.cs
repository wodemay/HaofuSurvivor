using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	// Generate Id:83a3776d-c059-4d82-9b89-9a3db6a9f5ff
	public partial class UIPopPanel
	{
		public const string Name = "UIPopPanel";

		[SerializeField]
		public UnityEngine.UI.Text Text_Title;
		[SerializeField]
		public UnityEngine.UI.Text Text_Content;
		[SerializeField]
		public UnityEngine.UI.Button Button_OK;
		[SerializeField]
		public UnityEngine.UI.Button Button_Cancel;

		private UIPopPanelData mPrivateData = null;

		protected override void ClearUIComponents()
		{
			Text_Title = null;
			Text_Content = null;
			Button_OK = null;
			Button_Cancel = null;

			mData = null;
		}

		public UIPopPanelData Data
		{
			get
			{
				return mData;
			}
		}

		UIPopPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UIPopPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
