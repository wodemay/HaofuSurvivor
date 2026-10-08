using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class CharacterExclusivePerkSystem : AbstractSystem, IRunUpdateable
	{
		private float mDodgeProjectileBonusRemaining;
		private float mSkillCooldownBonusRemaining;
		private float mDodgeDamageRemaining;
		private float mSkillGuardRemaining;

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
			if (!player.IsRegistered || stats.MaxHealth <= 0f) return 1f;
			var bonus = mDodgeDamageRemaining > 0f ? GetActiveValue(CharacterExclusivePerkType.DodgeDamageBoost) : 0f;
			foreach (var definition in this.GetUtility<CharacterExclusivePerkCatalog>().GetByCharacter(player.CharacterId))
			{
				var ratio = player.CurrentHealth / stats.MaxHealth;
				var value = definition.GetLevel(GetLevel(definition.Id))?.Value ?? 0f;
				if (definition.Type == CharacterExclusivePerkType.MissingHealthDamage)
					bonus += value * (1f - Mathf.Clamp01(ratio));
				if (definition.Type == CharacterExclusivePerkType.MovingDamage && this.GetModel<InputModel>().Movement.sqrMagnitude > 0.01f)
					bonus += value;
				if ((definition.Type == CharacterExclusivePerkType.LowHealthDamage && ratio <= definition.HealthThreshold) ||
					(definition.Type == CharacterExclusivePerkType.HealthyDamage && ratio >= definition.HealthThreshold))
					bonus += definition.GetLevel(GetLevel(definition.Id))?.Value ?? 0f;
			}
			return 1f + bonus;
		}

		public float GetIncomingDamageMultiplier() => mSkillGuardRemaining > 0f
			? 1f - Mathf.Clamp01(GetActiveValue(CharacterExclusivePerkType.SkillDamageReduction)) : 1f;

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
			mDodgeDamageRemaining = Mathf.Max(0f, mDodgeDamageRemaining - deltaTime);
			mSkillGuardRemaining = Mathf.Max(0f, mSkillGuardRemaining - deltaTime);
			if (mDodgeProjectileBonusRemaining <= 0f && mSkillCooldownBonusRemaining <= 0f && mDodgeDamageRemaining <= 0f && mSkillGuardRemaining <= 0f)
				this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		public void Reset()
		{
			this.GetModel<CharacterExclusivePerkModel>().Reset();
			mDodgeProjectileBonusRemaining = 0f;
			mSkillCooldownBonusRemaining = 0f;
			mDodgeDamageRemaining = 0f;
			mSkillGuardRemaining = 0f;
			this.GetSystem<GameLoopSystem>().UnregisterUpdateable(this);
		}

		public CharacterExclusivePerkRuntimeSaveData GetRuntimeSaveData()
		{
			return new CharacterExclusivePerkRuntimeSaveData
			{
				DodgeProjectileBonusRemaining = mDodgeProjectileBonusRemaining,
				SkillCooldownBonusRemaining = mSkillCooldownBonusRemaining,
				DodgeDamageRemaining = mDodgeDamageRemaining,
				SkillGuardRemaining = mSkillGuardRemaining
			};
		}

		public void Restore(IEnumerable<CharacterExclusivePerkSaveData> levels, CharacterExclusivePerkRuntimeSaveData runtime)
		{
			this.GetModel<CharacterExclusivePerkModel>().Restore(levels);
			mDodgeProjectileBonusRemaining = Mathf.Max(0f, runtime?.DodgeProjectileBonusRemaining ?? 0f);
			mSkillCooldownBonusRemaining = Mathf.Max(0f, runtime?.SkillCooldownBonusRemaining ?? 0f);
			mDodgeDamageRemaining = Mathf.Max(0f, runtime?.DodgeDamageRemaining ?? 0f);
			mSkillGuardRemaining = Mathf.Max(0f, runtime?.SkillGuardRemaining ?? 0f);
			RegisterTimedEffect();
		}

		private void OnDodgeEnded(DodgeEndedEvent dodgeEvent)
		{
			var boost = GetDefinition(CharacterExclusivePerkType.DodgeDamageBoost);
			if (boost != null && boost.TriggerDodgeId == dodgeEvent.DodgeId)
				mDodgeDamageRemaining = boost.GetLevel(GetLevel(boost.Id))?.Duration ?? 0f;
			RegisterTimedEffect();
			var definition = GetDefinition(CharacterExclusivePerkType.DodgeWeaponProjectileCount);
			var level = definition == null ? 0 : GetLevel(definition.Id);
			if (definition == null || (definition.TriggerDodgeId != 0 && definition.TriggerDodgeId != dodgeEvent.DodgeId)) return;
			if (level <= 0) return;
			mDodgeProjectileBonusRemaining = definition.GetLevel(level)?.Duration ?? 0f;
			RegisterTimedEffect();
		}

		private void OnSkillUsed(SkillUsedEvent skillEvent)
		{
			var guard = GetDefinition(CharacterExclusivePerkType.SkillDamageReduction);
			if (guard != null && guard.TriggerSkillId == skillEvent.SkillId)
				mSkillGuardRemaining = guard.GetLevel(GetLevel(guard.Id))?.Duration ?? 0f;
			RegisterTimedEffect();
			var definition = GetDefinition(CharacterExclusivePerkType.SkillWeaponCooldownReduction);
			var level = definition == null ? 0 : GetLevel(definition.Id);
			if (definition == null || level <= 0 || definition.TriggerSkillId != skillEvent.SkillId) return;
			mSkillCooldownBonusRemaining = definition.GetLevel(level)?.Duration ?? 0f;
			RegisterTimedEffect();
		}

		private void RegisterTimedEffect()
		{
			if (mDodgeProjectileBonusRemaining > 0f || mSkillCooldownBonusRemaining > 0f || mDodgeDamageRemaining > 0f || mSkillGuardRemaining > 0f)
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
