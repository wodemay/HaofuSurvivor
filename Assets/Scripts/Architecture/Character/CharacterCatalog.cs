using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class CharacterCatalog : IUtility
	{
		private readonly List<CharacterConfig> mCharacters;

		public CharacterCatalog()
		{
			mCharacters = new List<CharacterConfig>(Resources.LoadAll<CharacterConfig>("Configs/Characters"));
			mCharacters.Sort((left, right) => left.SortOrder.CompareTo(right.SortOrder));
		}

		public IReadOnlyList<CharacterConfig> All => mCharacters;

		public CharacterConfig Get(int id)
		{
			foreach (var character in mCharacters)
			{
				if (character.Id == id) return character;
			}

			return mCharacters.Count > 0 ? mCharacters[0] : null;
		}

		public bool Contains(int id)
		{
			foreach (var character in mCharacters)
			{
				if (character.Id == id) return true;
			}

			return false;
		}
	}
}
