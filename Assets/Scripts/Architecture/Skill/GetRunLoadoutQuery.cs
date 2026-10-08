using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public readonly struct RunLoadoutItem
	{
		public readonly string Name;
		public readonly Sprite Icon;
		public readonly int Level;
		public RunLoadoutItem(string name, Sprite icon, int level) { Name = name; Icon = icon; Level = level; }
	}

	public sealed class RunLoadoutState
	{
		public readonly List<RunLoadoutItem> Weapons = new();
		public readonly List<RunLoadoutItem> Skills = new();
	}

	public class GetRunLoadoutQuery : AbstractQuery<RunLoadoutState>
	{
		protected override RunLoadoutState OnDo()
		{
			var result = new RunLoadoutState();
			var loadout = this.GetModel<PlayerLoadoutModel>();
			var characters = GameArchitecture.Interface.GetUtility<CharacterCatalog>();
			var exclusiveSkillIds = new HashSet<int>();
			foreach (var character in characters.All)
			{
				var group = GameArchitecture.Interface.GetUtility<SkillGroupCatalog>().Get(character.SkillGroupId);
				if (group?.StartingSkillIds != null) exclusiveSkillIds.UnionWith(group.StartingSkillIds);
			}
			foreach (var weapon in loadout.Weapons)
			{
				var config = GameArchitecture.Interface.GetUtility<WeaponCatalog>().Get(weapon.WeaponId);
				if (config != null) result.Weapons.Add(new RunLoadoutItem(config.DisplayName, config.Icon, weapon.Level));
			}
			foreach (var skill in loadout.Skills)
			{
				if (exclusiveSkillIds.Contains(skill.SkillId)) continue;
				var config = GameArchitecture.Interface.GetUtility<SkillCatalog>().Get(skill.SkillId);
				if (config != null) result.Skills.Add(new RunLoadoutItem(config.DisplayName, config.Icon, skill.Level));
			}
			foreach (var upgrade in this.GetModel<PlayerStatUpgradeModel>().GetSaveData())
			{
				var config = GameArchitecture.Interface.GetUtility<StatUpgradeCatalog>().Get(upgrade.UpgradeId);
				if (config != null) result.Skills.Add(new RunLoadoutItem(config.DisplayName, config.Icon, upgrade.Level));
			}
			return result;
		}
	}
}
