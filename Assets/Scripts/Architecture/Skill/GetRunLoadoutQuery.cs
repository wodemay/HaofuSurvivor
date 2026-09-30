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
			foreach (var weapon in loadout.Weapons)
			{
				var config = GameArchitecture.Interface.GetUtility<WeaponCatalog>().Get(weapon.WeaponId);
				if (config != null) result.Weapons.Add(new RunLoadoutItem(config.DisplayName, config.Icon, weapon.Level));
			}
			foreach (var skill in loadout.Skills)
			{
				var config = GameArchitecture.Interface.GetUtility<SkillCatalog>().Get(skill.SkillId);
				if (config != null) result.Skills.Add(new RunLoadoutItem(config.DisplayName, config.Icon, skill.Level));
			}
			var dodge = loadout.DodgeId > 0 ? GameArchitecture.Interface.GetUtility<DodgeCatalog>().Get(loadout.DodgeId) : null;
			if (dodge != null) result.Skills.Add(new RunLoadoutItem(dodge.DisplayName, dodge.Icon, this.GetModel<DodgeModel>().Runtime?.Level ?? 1));
			foreach (var perk in GameArchitecture.Interface.GetUtility<CharacterExclusivePerkCatalog>().GetByCharacter(this.GetModel<PlayerModel>().CharacterId))
			{
				var level = this.GetModel<CharacterExclusivePerkModel>().GetLevel(perk.Id);
				if (level > 0) result.Skills.Add(new RunLoadoutItem(perk.DisplayName, perk.Icon, level));
			}
			return result;
		}
	}
}
