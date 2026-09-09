using System;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	// Generate Id:9f4184f1-9290-45c5-88af-a93885ddbc93
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
		[SerializeField]
		public UnityEngine.UI.Image Image_ExperienceFill;
		[SerializeField]
		public UnityEngine.UI.Text Text_PlayerLevel;
		[SerializeField]
		public UnityEngine.UI.Text Text_Experience;
		[SerializeField]
		public UnityEngine.UI.Image Image_SkillCooldownFill;
		[SerializeField]
		public UnityEngine.UI.Text Text_SkillName;
		[SerializeField]
		public UnityEngine.UI.Text Text_SkillCooldown;
		
		private UIGameHUDPanelData mPrivateData = null;
		
		protected override void ClearUIComponents()
		{
			Text_RemainingTime = null;
			Button_Back = null;
			Button_Pause = null;
			Text_RunCoin = null;
			Slider_BossHP = null;
			Image_ExperienceFill = null;
			Text_PlayerLevel = null;
			Text_Experience = null;
			Image_SkillCooldownFill = null;
			Text_SkillName = null;
			Text_SkillCooldown = null;
			
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
