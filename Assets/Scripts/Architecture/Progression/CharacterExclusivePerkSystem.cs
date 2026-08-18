using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class CharacterExclusivePerkSystem : AbstractSystem, IRunUpdateable
	{
		private float mDodgeProjectileBonusRemaining;
		private float mSkillCooldownBonusRemaining;

		public IReadOnlyList<CharacterExclusivePerkDefinition> GetEligible()
		{
			var player = this.GetModel<PlayerModel>();
			if (!player.IsRegistered) return new List<CharacterExclusivePerkDefinition>();
			var result = new List<CharacterExclusivePerkDefinition>();
			foreach (var definition in this.GetUtility<CharacterExclusivePerkCatalog>().GetByCharacter(player.CharacterId))
				if (HasUpgrade(definition.Id)) result.Add(definition);
			return result;
		}

		public bool HasUpgrade(int perkId)
		{
			var player = this.GetModel<PlayerModel>();
			var definition = this.GetUtility<CharacterExclusivePerkCatalog>().Get(perkId);
			return player.IsRegistered && definition != null && definition.CharacterId == player.CharacterId &&
				this.GetModel<CharacterExclusivePerkModel>().GetLevel(perkId) < definition.MaxLevel;
		}

		public bool Upgrade(int perkId)
		{
			if (!HasUpgrade(perkId)) return false;
			var model = this.GetModel<CharacterExclusivePerkModel>();
			var level = model.GetLevel(perkId) + 1;
			model.SetLevel(perkId, level);
			this.SendEvent(new CharacterExclusivePerkUpgradedEvent(perkId, level));
			return true;
		}

		public int GetLevel(int perkId)
		{
			return this.GetModel<CharacterExclusivePerkModel>().GetLevel(perkId);
		}

		public float GetDamageMultiplier()
		{
			var player = this.GetModel<PlayerModel>();
			var stats = this.GetModel<PlayerStatModel>();
			var definition = GetDefinition(CharacterExclusivePerkType.LowHealthDamage);
			if (!player.IsRegistered || definition == null || stats.MaxHealth <= 0f ||
				player.CurrentHealth > stats.MaxHealth * Mathf.Clamp01(definition.HealthThreshold)) return 1f;
			var level = GetLevel(definition.Id);
			return level <= 0 ? 1f : 1f + (definition.GetLevel(level)?.Value ?? 0f);
		}

		public int GetWeaponProjectileCountAdd()
		{
			return mDodgeProjectileBonusRemaining > 0f
				? Mathf.Max(0, Mathf.RoundToInt(GetActiveValue(CharacterExclusivePerkType.DodgeWeaponProjectileCount)))
				: 0;
		}

		public float GetWeaponCooldownMultiplier()
		{
			return mSkillCooldownBonusRemaining > 0f
				? Mathf.Max(0.01f, 1f - GetActiveValue(CharacterExclusivePerkType.SkillWeaponCooldownReduction))
				: 1f;
		}

		public void OnRunUpdate(float deltaTime)
		{
			mDodgeProjectileBonusRemaining = Mathf.Max(0f, mDodgeProjectileBonusRemaining - deltaTime);
			mSkillCooldownBonusRemaining = Mathf.Max(0f, mSkillCooldownBonusRemaining - deltaTime);
			if (mDodgeProjectileBonusRemaining <= 0f && mSkillCooldownBonusRemaining <= 0f)
				this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		public void Reset()
		{
			this.GetModel<CharacterExclusivePerkModel>().Reset();
			mDodgeProjectileBonusRemaining = 0f;
			mSkillCooldownBonusRemaining = 0f;
			this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		public CharacterExclusivePerkRuntimeSaveData GetRuntimeSaveData()
		{
			return new CharacterExclusivePerkRuntimeSaveData
			{
				DodgeProjectileBonusRemaining = mDodgeProjectileBonusRemaining,
				SkillCooldownBonusRemaining = mSkillCooldownBonusRemaining
			};
		}

		public void Restore(IEnumerable<CharacterExclusivePerkSaveData> levels, CharacterExclusivePerkRuntimeSaveData runtime)
		{
			this.GetModel<CharacterExclusivePerkModel>().Restore(levels);
			mDodgeProjectileBonusRemaining = Mathf.Max(0f, runtime?.DodgeProjectileBonusRemaining ?? 0f);
			mSkillCooldownBonusRemaining = Mathf.Max(0f, runtime?.SkillCooldownBonusRemaining ?? 0f);
			RegisterTimedEffect();
		}

		private void OnDodgeEnded(DodgeEndedEvent dodgeEvent)
		{
			var definition = GetDefinition(CharacterExclusivePerkType.DodgeWeaponProjectileCount);
			var level = definition == null ? 0 : GetLevel(definition.Id);
			if (definition == null || (definition.TriggerDodgeId != 0 && definition.TriggerDodgeId != dodgeEvent.DodgeId)) return;
			if (level <= 0) return;
			mDodgeProjectileBonusRemaining = definition.GetLevel(level)?.Duration ?? 0f;
			RegisterTimedEffect();
		}

		private void OnSkillUsed(SkillUsedEvent skillEvent)
		{
			var definition = GetDefinition(CharacterExclusivePerkType.SkillWeaponCooldownReduction);
			var level = definition == null ? 0 : GetLevel(definition.Id);
			if (definition == null || level <= 0 || definition.TriggerSkillId != skillEvent.SkillId) return;
			mSkillCooldownBonusRemaining = definition.GetLevel(level)?.Duration ?? 0f;
			RegisterTimedEffect();
		}

		private void RegisterTimedEffect()
		{
			if (mDodgeProjectileBonusRemaining > 0f || mSkillCooldownBonusRemaining > 0f)
				this.GetSystem<GameLoopSystem>().RegisterUpdateable(this);
		}

		private float GetActiveValue(CharacterExclusivePerkType type)
		{
			var level = GetHighestLevel(type);
			return level <= 0 ? 0f : GetLevelDefinition(type, level)?.Value ?? 0f;
		}

		private int GetHighestLevel(CharacterExclusivePerkType type)
		{
			var definition = GetDefinition(type);
			return definition == null ? 0 : GetLevel(definition.Id);
		}

		private CharacterExclusivePerkLevel GetLevelDefinition(CharacterExclusivePerkType type, int level)
		{
			return GetDefinition(type)?.GetLevel(level);
		}

		private CharacterExclusivePerkDefinition GetDefinition(CharacterExclusivePerkType type)
		{
			foreach (var definition in this.GetUtility<CharacterExclusivePerkCatalog>().GetByCharacter(this.GetModel<PlayerModel>().CharacterId))
				if (definition.Type == type) return definition;
			return null;
		}

		protected override void OnInit()
		{
			this.RegisterEvent<DodgeEndedEvent>(OnDodgeEnded);
			this.RegisterEvent<SkillUsedEvent>(OnSkillUsed);
			Reset();
		}
	}
}
