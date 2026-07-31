using System;
using UnityEngine;
using UnityEngine.UI;

namespace HaoFuSurvivor
{
	public class UICharacterSelectItem : MonoBehaviour
	{
		private Button mButton;
		private GameObject mSelection;
		private Text mNameText;
		private Text mSkillText;
		private Image mIcon;
		private Action<int> mOnSelected;

		public int CharacterId { get; private set; }

		public void Initialize(CharacterConfig character, Action<int> onSelected)
		{
			CharacterId = character.Id;
			mOnSelected = onSelected;
			mButton = GetComponent<Button>();
			mSelection = transform.Find("Image_Select").gameObject;
			mNameText = transform.Find("GameObject/Text_Name").GetComponent<Text>();
			mSkillText = transform.Find("GameObject/Text_Skill").GetComponent<Text>();
			mIcon = transform.Find("Image_Icon").GetComponent<Image>();

			mNameText.text = character.DisplayName;
			mSkillText.text = character.SkillDescription;
			mIcon.sprite = character.Icon;
			mButton.onClick.AddListener(Select);
		}

		public void SetSelected(bool selected)
		{
			mSelection.SetActive(selected);
		}

		private void Select()
		{
			mOnSelected?.Invoke(CharacterId);
		}

		private void OnDestroy()
		{
			if (mButton != null) mButton.onClick.RemoveListener(Select);
		}
	}
}
