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
		public readonly int Amount;
		public readonly int CurrentExperience;
		public readonly int RequiredExperience;

		public ExperienceCollectedEvent(int amount, int currentExperience, int requiredExperience)
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

	public struct WeaponEvolvedEvent
	{
		public readonly int WeaponRuntimeId;
		public readonly int SourceWeaponId;
		public readonly int TargetWeaponId;

		public WeaponEvolvedEvent(int weaponRuntimeId, int sourceWeaponId, int targetWeaponId)
		{
			WeaponRuntimeId = weaponRuntimeId;
			SourceWeaponId = sourceWeaponId;
			TargetWeaponId = targetWeaponId;
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
