using QFramework;
namespace HaoFuSurvivor
{
	public class CharacterSelectionSystem : AbstractSystem
	{
		public void Select(int characterId)
		{
			if (!this.GetUtility<CharacterCatalog>().Contains(characterId)) return;
			this.GetModel<CharacterSelectionModel>().SelectedCharacterId = characterId;
			this.SendEvent(new CharacterSelectionChangedEvent(characterId));
		}
		public void ConfirmSelection()
		{
			var id = this.GetModel<CharacterSelectionModel>().SelectedCharacterId;
			this.GetUtility<CharacterSelectionStorage>().SaveSelectedCharacterId(id);
			this.SendEvent(new CharacterSelectionConfirmedEvent(id));
		}
		protected override void OnInit()
		{
			var catalog = this.GetUtility<CharacterCatalog>();
			var id = this.GetUtility<CharacterSelectionStorage>().LoadSelectedCharacterId();
			this.GetModel<CharacterSelectionModel>().SelectedCharacterId = catalog.Contains(id) ? id : catalog.All[0].Id;
		}
	}
}
