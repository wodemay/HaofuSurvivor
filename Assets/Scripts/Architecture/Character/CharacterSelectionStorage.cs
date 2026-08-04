using QFramework;
using UnityEngine;
namespace HaoFuSurvivor
{
	public class CharacterSelectionStorage : IUtility
	{
		private const string SelectedCharacterKey = "HaoFuSurvivor.SelectedCharacterId";
		public int LoadSelectedCharacterId() => PlayerPrefs.GetInt(SelectedCharacterKey, -1);
		public void SaveSelectedCharacterId(int characterId)
		{
			PlayerPrefs.SetInt(SelectedCharacterKey, characterId);
			PlayerPrefs.Save();
		}
	}
}
