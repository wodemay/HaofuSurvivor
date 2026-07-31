using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	// Generate Id:b6348b22-7fca-4ef5-872c-e866cbab48fa
	public partial class UICharacterSelectPanel
	{
		public const string Name = "UICharacterSelectPanel";
		
		[SerializeField]
		public UnityEngine.GameObject CharacterGroup_Content;
		[SerializeField]
		public UnityEngine.GameObject UICharacterSelectItem;
		[SerializeField]
		public UnityEngine.UI.Button Button_ConfirmCharacter;
		[SerializeField]
		public UnityEngine.UI.Button Button_Back;
		
		private UICharacterSelectPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			CharacterGroup_Content = null;
			UICharacterSelectItem = null;
			Button_ConfirmCharacter = null;
			Button_Back = null;
			
			mData = null;
		}
		
		public UICharacterSelectPanelData Data
		{
			get
			{
				return mData;
			}
		}
		
		UICharacterSelectPanelData mData
		{
			get
			{
				return mPrivateData ?? (mPrivateData = new UICharacterSelectPanelData());
			}
			set
			{
				mUIData = value;
				mPrivateData = value;
			}
		}
	}
}
