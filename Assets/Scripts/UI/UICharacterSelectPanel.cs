using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace HaoFuSurvivor
{
	public class UICharacterSelectPanelData : UIPanelData
	{
	}
	public partial class UICharacterSelectPanel : UIPanel, IController
	{
		private int mSelectedCharacterId = -1;

		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		protected override void OnInit(IUIData uiData = null)
		{
			mData = uiData as UICharacterSelectPanelData ?? new UICharacterSelectPanelData();
			UICharacterSelectItem.SetActive(false);
			Button_ConfirmCharacter.onClick.AddListener(ConfirmSelection);
			Button_Back.onClick.AddListener(Back);
			BuildCharacterItems();
		}
		
		protected override void OnOpen(IUIData uiData = null)
		{
		}
		
		protected override void OnShow()
		{
			RefreshSelection();
		}
		
		protected override void OnHide()
		{
		}
		
		protected override void OnClose()
		{
			Button_ConfirmCharacter.onClick.RemoveListener(ConfirmSelection);
			Button_Back.onClick.RemoveListener(Back);
		}

		private void BuildCharacterItems()
		{
			mSelectedCharacterId = this.GetModel<CharacterSelectionModel>().SelectedCharacterId;

			foreach (var character in this.GetUtility<CharacterCatalog>().All)
			{
				var itemObject = Instantiate(UICharacterSelectItem, CharacterGroup_Content.transform);
				var item = itemObject.AddComponent<UICharacterSelectItem>();
				item.Initialize(character, OnCharacterSelected);
				itemObject.SetActive(true);
			}
		}

		private void OnCharacterSelected(int characterId)
		{
			mSelectedCharacterId = characterId;
			this.SendCommand(new SelectCharacterCommand(characterId));
			RefreshSelection();
		}

		private void RefreshSelection()
		{
			Button_ConfirmCharacter.interactable = mSelectedCharacterId >= 0;

			foreach (Transform child in CharacterGroup_Content.transform)
			{
				var item = child.GetComponent<UICharacterSelectItem>();
				if (item != null) item.SetSelected(item.CharacterId == mSelectedCharacterId);
			}
		}

		private void ConfirmSelection()
		{
			if (mSelectedCharacterId < 0) return;

			this.SendCommand<ConfirmCharacterSelectionCommand>();
			this.SendCommand<StartSelectedCharacterRunCommand>();
			CloseSelf();
		}
	}

}
