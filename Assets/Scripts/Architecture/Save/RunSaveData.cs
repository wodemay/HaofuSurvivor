using System;
using System.Collections.Generic;

namespace HaoFuSurvivor
{
	[Serializable]
	public class RunSaveData
	{
		public int SaveVersion;
		public int CharacterId;
		public bool HasMapSnapshot;
		public int WorldSeed;
		public int MapThemeId;
		public int MapGeneratorVersion;
		public float ElapsedSeconds;
		public string RunCoin = "0";
		public int NormalKillCount;
		public int BossKillCount;
		public int EndlessRound;
		public int CurrentStageIndex;
		public float CurrentHealth;
		public float PositionX;
		public float PositionY;
		public int Level;
		public float CurrentExperience;
		public float RequiredExperience;
		public int DodgeId;
		public int DodgeLevel;
		public bool HasSkillSnapshot;
		public List<SkillSaveData> Skills = new();
		public List<WeaponSaveData> Weapons = new();
		public int SavedPhase;
		public string RandomStateJson;
		public float DamageInvulnerabilityRemaining;
		public float DodgeInvulnerabilityRemaining;
		public float DodgeCooldownRemaining;
		public float DodgeDurationRemaining;
		public float DodgeDirectionX;
		public float DodgeDirectionY;
		public bool DodgeIsActive;
		public List<StatUpgradeSaveData> StatUpgrades = new();
		public List<CharacterExclusivePerkSaveData> CharacterPerks = new();
		public CharacterExclusivePerkRuntimeSaveData CharacterPerkRuntime = new();
		public List<int> PendingLevelSelections = new();
		public List<LevelUpOptionSaveData> CurrentLevelOptions = new();
		public float EnemySpawnElapsed;
		public List<EnemySaveData> Enemies = new();
		public List<ExperienceDropSaveData> ExperienceDrops = new();
		public List<ProjectileSaveData> Projectiles = new();
		public List<GroundFlameSaveData> GroundFlames = new();
		public List<TimedEffectSaveData> TimedEffects = new();
		public List<BarrageSaveData> Barrages = new();
		public List<AttackCooldownSaveData> AttackCooldowns = new();
		public List<BreakableSaveData> Breakables = new();
		public List<MapEventSaveData> MapEvents = new();
		public float MapEventSpawnElapsed;
		public int MapEventSpawnIndex;
		public List<PickupSaveData> Pickups = new();
	}

	[Serializable] public class StatUpgradeSaveData { public int UpgradeId; public int Level; }
	[Serializable] public class CharacterExclusivePerkSaveData { public int PerkId; public int Level; }
	[Serializable] public class CharacterExclusivePerkRuntimeSaveData { public float DodgeProjectileBonusRemaining; public float SkillCooldownBonusRemaining; }
	[Serializable] public class LevelUpOptionSaveData { public string CandidateKey; public int Type; public int RuntimeId; public int ContentId; public int CurrentLevel; public string DisplayName; public string Description; public bool IsNewWeapon; public string LevelText; public float Weight; }
	[Serializable] public class EnemySaveData { public int ConfigId; public float PositionX; public float PositionY; public float CurrentHealth; public float MoveSpeed; public List<AttackCooldownSaveData> AttackCooldowns = new(); }
	[Serializable] public class ExperienceDropSaveData { public int ConfigId; public float Experience; public float PositionX; public float PositionY; public bool IsCaptured; public float AbsorbSpeed; }
	[Serializable] public class ProjectileSaveData { public int AttackId; public float PositionX; public float PositionY; public float DirectionX; public float DirectionY; public int OwnerFaction; public float Damage; public float MoveSpeed; public float RemainingLifetime; public int RemainingPierce; }
	[Serializable] public class GroundFlameSaveData { public int AttackId; public float PositionX; public float PositionY; public int OwnerFaction; public float Damage; public float RemainingDuration; public float RemainingUntilTick; }
	[Serializable] public class TimedEffectSaveData { public int AttackId; public float PositionX; public float PositionY; public float RemainingDuration; }
	[Serializable] public class BarrageSaveData { public int AttackId; public int OwnerFaction; public float Damage; public bool IsUltimate; public float DurationRemaining; public float TimeUntilNextProjectile; public float OrbitAngle; }
	[Serializable] public class AttackCooldownSaveData { public int RuntimeId; public int AttackId; public float CooldownRemaining; }
	[Serializable] public class BreakableSaveData { public string StableId; public bool IsDestroyed; }
	[Serializable] public class MapEventSaveData { public string StableId; public int ConfigId; public float PositionX; public float PositionY; public int Progress; public float HoldElapsed; public bool IsCompleted; }
	[Serializable] public class PickupSaveData { public int TableId; public int EntryId; public float PositionX; public float PositionY; public bool IsCaptured; public float AbsorbSpeed; }

	[Serializable]
	public class SkillSaveData
	{
		public int RuntimeId;
		public int SkillId;
		public int Level;
	}

	[Serializable]
	public class WeaponSaveData
	{
		public int RuntimeId;
		public int WeaponId;
		public int Level;
		public bool CanUpgrade;
		public List<int> AttackIds = new();
		public List<WeaponModifierSaveData> Modifiers = new();
	}

	[Serializable]
	public class WeaponModifierSaveData
	{
		public int AttackId;
		public string Key;
		public float Value;
	}
}
