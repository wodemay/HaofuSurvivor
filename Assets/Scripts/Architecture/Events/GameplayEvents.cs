namespace HaoFuSurvivor
{
	public struct RunStartedEvent
	{
	}

	public struct RunPausedEvent
	{
	}

	public struct RunResumedEvent
	{
	}

	public struct RunEndedEvent
	{
		public readonly RunPhase Phase;

		public RunEndedEvent(RunPhase phase)
		{
			Phase = phase;
		}
	}

	public struct RunSettledEvent
	{
		public readonly RunSettlementData Data;

		public RunSettledEvent(RunSettlementData data)
		{
			Data = data;
		}
	}

	public struct RunTimerUpdatedEvent
	{
		public readonly int ElapsedSeconds;

		public RunTimerUpdatedEvent(int elapsedSeconds)
		{
			ElapsedSeconds = elapsedSeconds;
		}
	}

	public struct RunTimelineStageReachedEvent
	{
		public readonly int StageIndex;
		public readonly int EventId;
		public readonly float EnemyHealthMultiplier;
		public readonly float EnemyDamageMultiplier;
		public readonly float EnemyMoveSpeedMultiplier;
		public readonly float SpawnRateMultiplier;

		public RunTimelineStageReachedEvent(int stageIndex, RunTimelineStage stage)
		{
			StageIndex = stageIndex;
			EventId = stage.EventId;
			EnemyHealthMultiplier = stage.EnemyHealthMultiplier;
			EnemyDamageMultiplier = stage.EnemyDamageMultiplier;
			EnemyMoveSpeedMultiplier = stage.EnemyMoveSpeedMultiplier;
			SpawnRateMultiplier = stage.SpawnRateMultiplier;
		}
	}

	public struct PlayerDamagedEvent
	{
		public readonly float Damage;
		public readonly float RemainingHealth;

		public PlayerDamagedEvent(float damage, float remainingHealth)
		{
			Damage = damage;
			RemainingHealth = remainingHealth;
		}
	}

	public struct PlayerDiedEvent
	{
	}

	public struct PlayerHealedEvent
	{
		public readonly float Amount;
		public readonly float CurrentHealth;

		public PlayerHealedEvent(float amount, float currentHealth)
		{
			Amount = amount;
			CurrentHealth = currentHealth;
		}
	}

	public struct PlayerHealthRestoredEvent
	{
	}

	public struct EnemyDamagedEvent
	{
		public readonly CombatEntity Enemy;
		public readonly float Damage;
		public readonly float RemainingHealth;

		public EnemyDamagedEvent(CombatEntity enemy, float damage, float remainingHealth)
		{
			Enemy = enemy;
			Damage = damage;
			RemainingHealth = remainingHealth;
		}
	}

	public struct EnemyDiedEvent
	{
		public readonly ExperienceDropConfig ExperienceDrop;
		public readonly UnityEngine.Vector2 DeathPosition;

		public EnemyDiedEvent(ExperienceDropConfig experienceDrop, UnityEngine.Vector2 deathPosition)
		{
			ExperienceDrop = experienceDrop;
			DeathPosition = deathPosition;
		}
	}

	public struct ExperienceCollectedEvent
	{
		public readonly float Amount;
		public readonly float CurrentExperience;
		public readonly float RequiredExperience;

		public ExperienceCollectedEvent(float amount, float currentExperience, float requiredExperience)
		{
			Amount = amount;
			CurrentExperience = currentExperience;
			RequiredExperience = requiredExperience;
		}
	}

	public struct PlayerLevelUpEvent
	{
		public readonly int Level;

		public PlayerLevelUpEvent(int level)
		{
			Level = level;
		}
	}

	public struct PlayerStatUpgradedEvent
	{
		public readonly int UpgradeId;
		public readonly int Level;

		public PlayerStatUpgradedEvent(int upgradeId, int level)
		{
			UpgradeId = upgradeId;
			Level = level;
		}
	}

	public struct LevelUpSelectionRequestedEvent
	{
	}

	public struct WeaponEquippedEvent
	{
		public readonly int WeaponRuntimeId;
		public readonly int WeaponId;

		public WeaponEquippedEvent(int weaponRuntimeId, int weaponId)
		{
			WeaponRuntimeId = weaponRuntimeId;
			WeaponId = weaponId;
		}
	}

	public struct WeaponUpgradedEvent
	{
		public readonly int WeaponRuntimeId;
		public readonly int WeaponId;
		public readonly int Level;

		public WeaponUpgradedEvent(int weaponRuntimeId, int weaponId, int level)
		{
			WeaponRuntimeId = weaponRuntimeId;
			WeaponId = weaponId;
			Level = level;
		}
	}

	public struct WeaponReplacedEvent
	{
		public readonly int WeaponRuntimeId;
		public readonly int WeaponId;

		public WeaponReplacedEvent(int weaponRuntimeId, int weaponId)
		{
			WeaponRuntimeId = weaponRuntimeId;
			WeaponId = weaponId;
		}
	}

	public struct WeaponCombinedEvent
	{
		public readonly int CombinationId;
		public readonly int TargetWeaponId;

		public WeaponCombinedEvent(int combinationId, int targetWeaponId)
		{
			CombinationId = combinationId;
			TargetWeaponId = targetWeaponId;
		}
	}

	public struct SkillUpgradedEvent
	{
		public readonly int SkillRuntimeId;
		public readonly int SkillId;
		public readonly int Level;

		public SkillUpgradedEvent(int skillRuntimeId, int skillId, int level)
		{
			SkillRuntimeId = skillRuntimeId;
			SkillId = skillId;
			Level = level;
		}
	}

	public struct SkillUsedEvent
	{
		public readonly int SkillId;

		public SkillUsedEvent(int skillId)
		{
			SkillId = skillId;
		}
	}

	public struct CharacterExclusivePerkUpgradedEvent
	{
		public readonly int PerkId;
		public readonly int Level;

		public CharacterExclusivePerkUpgradedEvent(int perkId, int level)
		{
			PerkId = perkId;
			Level = level;
		}
	}

	public struct CharacterSelectionChangedEvent
	{
		public readonly int CharacterId;

		public CharacterSelectionChangedEvent(int characterId)
		{
			CharacterId = characterId;
		}
	}

	public struct CharacterSelectionConfirmedEvent
	{
		public readonly int CharacterId;

		public CharacterSelectionConfirmedEvent(int characterId)
		{
			CharacterId = characterId;
		}
	}
}
