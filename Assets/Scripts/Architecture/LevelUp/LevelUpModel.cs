using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HaoFuSurvivor
{
	public enum LevelUpOptionType
	{
		Weapon,
		Dodge,
		Skill,
		Stat,
		WeaponCombination,
		CharacterPerk
	}

	public readonly struct LevelUpOption
	{
		public readonly string CandidateKey;
		public readonly LevelUpOptionType Type;
		public readonly int RuntimeId;
		public readonly int ContentId;
		public readonly int CurrentLevel;
		public readonly string DisplayName;
		public readonly string Description;
		public readonly UnityEngine.Sprite Icon;
		public readonly bool IsNewWeapon;
		public readonly string LevelText;
		public readonly float Weight;

		private LevelUpOption(string candidateKey, LevelUpOptionType type, int runtimeId, int contentId, int currentLevel,
			string displayName, string description, UnityEngine.Sprite icon, bool isNewWeapon, string levelText, float weight = 1f)
		{
			CandidateKey = candidateKey;
			Type = type;
			RuntimeId = runtimeId;
			ContentId = contentId;
			CurrentLevel = currentLevel;
			DisplayName = displayName;
			Description = description;
			Icon = icon;
			IsNewWeapon = isNewWeapon;
			LevelText = levelText;
			Weight = weight;
		}

		public static LevelUpOption CreateWeapon(WeaponRuntimeData runtime, WeaponConfig config, string description)
		{
			var displayName = string.IsNullOrWhiteSpace(config.DisplayName) ? $"Weapon {runtime.WeaponId}" : config.DisplayName;
			return new LevelUpOption(
				$"weapon:{runtime.RuntimeId}:{runtime.WeaponId}:{runtime.Level}", LevelUpOptionType.Weapon,
				runtime.RuntimeId, runtime.WeaponId, runtime.Level, displayName,
				string.IsNullOrWhiteSpace(description) ? config.Description : description, config.Icon, false,
				$"Level{runtime.Level}->Level{runtime.Level + 1}");
		}

		public static LevelUpOption CreateWeaponAcquisition(WeaponConfig config)
		{
			var displayName = string.IsNullOrWhiteSpace(config.DisplayName) ? $"Weapon {config.Id}" : config.DisplayName;
			return new LevelUpOption($"weapon-acquire:{config.Id}", LevelUpOptionType.Weapon, 0, config.Id, 0,
				displayName, config.Description, config.Icon, true, "Level0->Level1");
		}

		public static LevelUpOption CreateWeaponCombination(WeaponCombinationConfig config, WeaponConfig target)
		{
			var displayName = string.IsNullOrWhiteSpace(config.DisplayName) ? target.DisplayName : config.DisplayName;
			return new LevelUpOption($"weapon-combination:{config.Id}", LevelUpOptionType.WeaponCombination, 0, config.Id, 0,
				displayName, config.Description, config.Icon != null ? config.Icon : target.Icon, false, "最终形态");
		}

		public static LevelUpOption CreateDodge(DodgeRuntimeData runtime, DodgeConfig config, string description)
		{
			var displayName = string.IsNullOrWhiteSpace(config.DisplayName) ? $"Dodge {runtime.DodgeId}" : config.DisplayName;
			return new LevelUpOption(
				$"dodge:{runtime.DodgeId}:{runtime.Level}", LevelUpOptionType.Dodge,
				0, runtime.DodgeId, runtime.Level, displayName,
				string.IsNullOrWhiteSpace(description) ? config.Description : description, config.Icon, false,
				$"Level{runtime.Level}->Level{runtime.Level + 1}");
		}

		public static LevelUpOption CreateSkill(SkillRuntimeData runtime, CharacterExclusiveSkillUpgradeDefinition definition)
		{
			var displayName = string.IsNullOrWhiteSpace(definition.DisplayName) ? $"Skill {runtime.SkillId}" : definition.DisplayName;
			return new LevelUpOption(
				$"exclusive-skill:{definition.CharacterId}:{runtime.SkillId}", LevelUpOptionType.Skill,
				runtime.RuntimeId, runtime.SkillId, runtime.Level, displayName, definition.Description, definition.Icon, false,
				$"Level{runtime.Level}->Level{runtime.Level + 1}");
		}

		public static LevelUpOption CreateStat(StatUpgradeDefinition definition, int currentLevel)
		{
			return new LevelUpOption(
				$"stat:{definition.Id}:{currentLevel}", LevelUpOptionType.Stat,
				0, definition.Id, currentLevel, definition.DisplayName, definition.Description, definition.Icon, false,
				$"Level{currentLevel}->Level{currentLevel + 1}");
		}

		public static LevelUpOption CreateCharacterPerk(CharacterExclusivePerkDefinition definition, int currentLevel)
		{
			return new LevelUpOption(
				$"character-perk:{definition.Id}:{currentLevel}", LevelUpOptionType.CharacterPerk,
				0, definition.Id, currentLevel, definition.DisplayName, definition.Description, definition.Icon, false,
				$"Level{currentLevel}->Level{currentLevel + 1}", UnityEngine.Mathf.Max(0.01f, definition.CandidateWeight));
		}

		public LevelUpOptionSaveData GetSaveData()
		{
			return new LevelUpOptionSaveData
			{
				CandidateKey = CandidateKey,
				Type = (int)Type,
				RuntimeId = RuntimeId,
				ContentId = ContentId,
				CurrentLevel = CurrentLevel,
				DisplayName = DisplayName,
				Description = Description,
				IsNewWeapon = IsNewWeapon,
				LevelText = LevelText,
				Weight = Weight
			};
		}

		public static LevelUpOption FromSaveData(LevelUpOptionSaveData data)
		{
			return new LevelUpOption(data.CandidateKey, (LevelUpOptionType)data.Type, data.RuntimeId, data.ContentId,
				data.CurrentLevel, DecodeSavedText(data.DisplayName), DecodeSavedText(data.Description), null,
				data.IsNewWeapon, DecodeSavedText(data.LevelText), data.Weight);
		}

		private static string DecodeSavedText(string value)
		{
			if (string.IsNullOrEmpty(value) || !value.Contains("\\u")) return value;
			try { return Regex.Unescape(value); }
			catch { return value; }
		}
	}

	public class LevelUpModel : QFramework.AbstractModel
	{
		private readonly Queue<int> mPendingLevels = new();
		private readonly List<LevelUpOption> mCurrentOptions = new();

		public int PendingSelectionCount => mPendingLevels.Count;
		public int CurrentLevel => mPendingLevels.Count > 0 ? mPendingLevels.Peek() : 0;
		public IReadOnlyList<LevelUpOption> CurrentOptions => mCurrentOptions;

		public void Enqueue(int level)
		{
			mPendingLevels.Enqueue(level);
		}

		public void CompleteCurrent()
		{
			if (mPendingLevels.Count > 0) mPendingLevels.Dequeue();
			mCurrentOptions.Clear();
		}

		public bool ContainsOption(string candidateKey)
		{
			return mCurrentOptions.Exists(option => option.CandidateKey == candidateKey);
		}

		public void SetCurrentOptions(IEnumerable<LevelUpOption> options)
		{
			mCurrentOptions.Clear();
			if (options == null) return;
			mCurrentOptions.AddRange(options);
		}

		public IEnumerable<int> GetPendingLevels() => mPendingLevels;

		public void Restore(IEnumerable<int> pendingLevels, IEnumerable<LevelUpOption> options)
		{
			mPendingLevels.Clear();
			if (pendingLevels != null)
				foreach (var level in pendingLevels)
					if (level > 0) mPendingLevels.Enqueue(level);
			SetCurrentOptions(options);
		}

		public void Reset()
		{
			mPendingLevels.Clear();
			mCurrentOptions.Clear();
		}

		protected override void OnInit()
		{
			Reset();
		}
	}
}
