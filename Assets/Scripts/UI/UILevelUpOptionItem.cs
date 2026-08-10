using System;
using UnityEngine;
using UnityEngine.UI;

namespace HaoFuSurvivor
{
	public class UILevelUpOptionItem : MonoBehaviour
	{
		private Button mSelectButton;

		public void Initialize(LevelUpWeaponOption option, Action<int> onSelected)
		{
			var icon = transform.Find("Image_Icon")?.GetComponent<Image>();
			if (icon != null) icon.sprite = option.Icon;
			SetText("TextGroup/Text_Name", option.DisplayName);
			SetText("TextGroup/Text_Level", $"Level{option.CurrentLevel}->Level{option.CurrentLevel + 1}");
			SetText("TextGroup/Text_Description", option.Description);
			mSelectButton = transform.Find("Button_Select")?.GetComponent<Button>();
			if (mSelectButton == null) return;
			mSelectButton.onClick.RemoveAllListeners();
			mSelectButton.onClick.AddListener(() => onSelected?.Invoke(option.RuntimeId));
		}

		private void SetText(string path, string content)
		{
			var text = transform.Find(path)?.GetComponent<Text>();
			if (text != null) text.text = content;
		}

		private void OnDestroy()
		{
			if (mSelectButton != null) mSelectButton.onClick.RemoveAllListeners();
		}
	}
}
