using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class CharacterExclusivePerkCatalog : IUtility
	{
		public CharacterExclusivePerkConfig Config { get; }

		public CharacterExclusivePerkCatalog()
		{
			Config = Resources.Load<CharacterExclusivePerkConfig>("Configs/Progression/CharacterExclusivePerkCatalog");
		}

		public CharacterExclusivePerkDefinition Get(int perkId)
		{
			return Config?.Perks.Find(item => item != null && item.Id == perkId);
		}

		public IReadOnlyList<CharacterExclusivePerkDefinition> GetByCharacter(int characterId)
		{
			return Config?.Perks.FindAll(item => item != null && item.CharacterId == characterId)
				?? new List<CharacterExclusivePerkDefinition>();
		}
	}
}
