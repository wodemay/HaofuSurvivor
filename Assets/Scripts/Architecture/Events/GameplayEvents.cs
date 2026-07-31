namespace HaoFuSurvivor
{
	public struct RunStartedEvent
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
