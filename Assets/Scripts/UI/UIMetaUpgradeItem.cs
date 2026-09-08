using System;
using UnityEngine;
using UnityEngine.UI;

namespace HaoFuSurvivor
{
	public class UIMetaUpgradeItem : MonoBehaviour
	{
		private Button mButton;

		public void Initialize(MetaUpgradeDefinition definition, int level, BigCoin coins, Action onUpgrade)
		{
			var icon = transform.Find("Icon")?.GetComponent<Image>();
			if (icon != null) icon.sprite = definition.Icon;
			SetText("TextGroup/Text_Name", definition.DisplayName);
			SetText("TextGroup/Text_Description", definition.Description);
			var maxLevel = Mathf.Max(0, definition.MaxLevel);
			var costText = level >= maxLevel ? "已满级" : $"金币数：{new BigCoin(definition.GetCost(level + 1)).ToDisplayString()}";
			SetText("TextGroup/Text_Coin", costText);
			var group = transform.Find("UpgradePointGroup");
			var template = group?.Find("UpgradePoint");
			if (template != null)
			{
				template.gameObject.SetActive(false);
				for (var index = group.childCount - 1; index >= 0; index--)
					if (group.GetChild(index) != template) Destroy(group.GetChild(index).gameObject);
				for (var index = 0; index < maxLevel; index++)
				{
					var point = Instantiate(template.gameObject, group.transform);
					point.name = $"Point_{index}";
					point.SetActive(true);
					var selected = point.transform.Find("UpgradePointSelect");
					if (selected != null) selected.gameObject.SetActive(index < level);
				}
			}
			mButton = GetComponent<Button>();
			if (mButton == null) return;
			mButton.onClick.RemoveAllListeners();
			mButton.interactable = level < maxLevel && coins.CompareTo(new BigCoin(definition.GetCost(level + 1))) >= 0;
			mButton.onClick.AddListener(() => onUpgrade?.Invoke());
		}

		private void SetText(string path, string value)
		{
			var text = transform.Find(path)?.GetComponent<Text>();
			if (text != null) text.text = value ?? string.Empty;
		}

		private void OnDestroy()
		{
			if (mButton != null) mButton.onClick.RemoveAllListeners();
		}
	}
}
