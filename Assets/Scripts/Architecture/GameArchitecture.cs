using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class GameArchitecture : Architecture<GameArchitecture>
	{
		protected override void Init()
		{
			RegisterModel(new RunModel());
			RegisterModel(new RunTimerModel());
			RegisterModel(new PlayerModel());
			RegisterModel(new PlayerStatModel());
			RegisterModel(new InputModel());
			RegisterModel(new CharacterSelectionModel());

			RegisterUtility(new CharacterCatalog());
			RegisterUtility(new CharacterSelectionStorage());
			RegisterUtility(new RunTimelineCatalog());

			RegisterSystem(new RunSystem());
			RegisterSystem(new RunTimerSystem());
			RegisterSystem(new InputSystem());
			RegisterSystem(new PlayerSystem());
			RegisterSystem(new PlayerSpawnSystem());
			RegisterSystem(new StatSystem());
			RegisterSystem(new DamageSystem());
			RegisterSystem(new CharacterSelectionSystem());
		}
	}

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

	public class CharacterSelectionModel : AbstractModel
	{
		public int SelectedCharacterId { get; internal set; }

		protected override void OnInit()
		{
			SelectedCharacterId = -1;
		}
	}

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

	public class CharacterSelectionSystem : AbstractSystem
	{
		public void Select(int characterId)
		{
			var catalog = this.GetUtility<CharacterCatalog>();
			if (!catalog.Contains(characterId)) return;

			this.GetModel<CharacterSelectionModel>().SelectedCharacterId = characterId;
			this.SendEvent(new CharacterSelectionChangedEvent(characterId));
		}

		public void ConfirmSelection()
		{
			var selectedCharacterId = this.GetModel<CharacterSelectionModel>().SelectedCharacterId;
			this.GetUtility<CharacterSelectionStorage>().SaveSelectedCharacterId(selectedCharacterId);
			this.SendEvent(new CharacterSelectionConfirmedEvent(selectedCharacterId));
		}

		protected override void OnInit()
		{
			var catalog = this.GetUtility<CharacterCatalog>();
			var storedCharacterId = this.GetUtility<CharacterSelectionStorage>().LoadSelectedCharacterId();
			this.GetModel<CharacterSelectionModel>().SelectedCharacterId = catalog.Contains(storedCharacterId)
				? storedCharacterId
				: catalog.All[0].Id;
		}
	}
}
