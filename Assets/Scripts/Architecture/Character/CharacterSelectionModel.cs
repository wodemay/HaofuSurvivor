using QFramework;
namespace HaoFuSurvivor
{
	public class CharacterSelectionModel : AbstractModel
	{
		public int SelectedCharacterId { get; internal set; }
		protected override void OnInit() => SelectedCharacterId = -1;
	}
}
