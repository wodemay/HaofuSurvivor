using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class StartRunCommand : AbstractCommand
	{
		protected override void OnExecute()
		{
			this.GetSystem<RunSystem>().StartRun();
		}
	}

	public class StartSelectedCharacterRunCommand : AbstractCommand
	{
		protected override void OnExecute()
		{
			if (this.GetSystem<PlayerSpawnSystem>().SpawnSelectedCharacter())
			{
				this.GetSystem<RunSystem>().StartRun();
			}
		}
	}

	public class TickRunTimerCommand : AbstractCommand
	{
		private readonly float mDeltaTime;

		public TickRunTimerCommand(float deltaTime)
		{
			mDeltaTime = deltaTime;
		}

		protected override void OnExecute()
		{
			this.GetSystem<RunTimerSystem>().Advance(mDeltaTime);
		}
	}

	public class TickEnemiesCommand : AbstractCommand
	{
		protected override void OnExecute()
		{
			this.GetSystem<EnemySystem>().Tick();
		}
	}

	public class TickRunPhysicsCommand : AbstractCommand
	{
		private readonly float mUnscaledFixedDeltaTime;

		public TickRunPhysicsCommand(float unscaledFixedDeltaTime)
		{
			mUnscaledFixedDeltaTime = unscaledFixedDeltaTime;
		}

		protected override void OnExecute()
		{
			this.GetSystem<RunTimerSystem>().AdvanceFixed(mUnscaledFixedDeltaTime);
		}
	}

	public class PauseRunCommand : AbstractCommand
	{
		protected override void OnExecute()
		{
			this.GetSystem<RunSystem>().Pause();
		}
	}

	public class ResumeRunCommand : AbstractCommand
	{
		protected override void OnExecute()
		{
			this.GetSystem<RunSystem>().Resume();
		}
	}

	public class TickAttacksCommand : AbstractCommand
	{
		protected override void OnExecute()
		{
			this.GetSystem<AttackSystem>().Advance();
		}
	}

	public class TickPlayerDamageInvulnerabilityCommand : AbstractCommand
	{
		protected override void OnExecute()
		{
			this.GetSystem<PlayerSystem>().AdvanceDamageInvulnerability();
		}
	}

	public class TickExperienceCommand : AbstractCommand
	{
		protected override void OnExecute()
		{
			this.GetSystem<ExperienceSystem>().Tick();
		}
	}

	public class CompleteLevelUpWeaponCommand : AbstractCommand
	{
		private readonly int mWeaponRuntimeId;

		public CompleteLevelUpWeaponCommand(int weaponRuntimeId)
		{
			mWeaponRuntimeId = weaponRuntimeId;
		}

		protected override void OnExecute()
		{
			this.GetSystem<LevelUpSystem>().CompleteWeaponUpgrade(mWeaponRuntimeId);
		}
	}

	public class ExitRunToCharacterSelectionCommand : AbstractCommand
	{
		protected override void OnExecute()
		{
			this.GetSystem<RunSystem>().ExitToCharacterSelection();
		}
	}

	public class RegisterAttackCommand : AbstractCommand
	{
		private readonly int mRuntimeId;
		private readonly int mAttackId;
		private readonly GameObject mOwner;
		private readonly CombatFaction mOwnerFaction;

		public RegisterAttackCommand(int runtimeId, int attackId, GameObject owner, CombatFaction ownerFaction)
		{
			mRuntimeId = runtimeId;
			mAttackId = attackId;
			mOwner = owner;
			mOwnerFaction = ownerFaction;
		}

		protected override void OnExecute()
		{
			this.GetSystem<AttackSystem>().Register(mRuntimeId, mAttackId, mOwner, mOwnerFaction);
		}
	}

	public class UnregisterAttackCommand : AbstractCommand
	{
		private readonly int mRuntimeId;

		public UnregisterAttackCommand(int runtimeId)
		{
			mRuntimeId = runtimeId;
		}

		protected override void OnExecute()
		{
			this.GetSystem<AttackSystem>().Unregister(mRuntimeId);
		}
	}

	public class TryExecuteAttackCommand : AbstractCommand
	{
		private readonly int mRuntimeId;
		private readonly CombatEntity mTarget;

		public TryExecuteAttackCommand(int runtimeId, CombatEntity target)
		{
			mRuntimeId = runtimeId;
			mTarget = target;
		}

		protected override void OnExecute()
		{
			this.GetSystem<AttackSystem>().TryExecute(mRuntimeId, mTarget);
		}
	}

	public class ApplyCombatDamageCommand : AbstractCommand
	{
		private readonly CombatEntity mTarget;
		private readonly float mDamage;

		public ApplyCombatDamageCommand(CombatEntity target, float damage)
		{
			mTarget = target;
			mDamage = damage;
		}

		protected override void OnExecute()
		{
			this.GetSystem<DamageSystem>().ApplyDamage(mTarget, mDamage);
		}
	}

	public class RegisterCombatTargetCommand : AbstractCommand
	{
		private readonly CombatEntity mEntity;

		public RegisterCombatTargetCommand(CombatEntity entity)
		{
			mEntity = entity;
		}

		protected override void OnExecute()
		{
			this.GetSystem<CombatTargetSystem>().Register(mEntity);
		}
	}

	public class UnregisterCombatTargetCommand : AbstractCommand
	{
		private readonly CombatEntity mEntity;

		public UnregisterCombatTargetCommand(CombatEntity entity)
		{
			mEntity = entity;
		}

		protected override void OnExecute()
		{
			this.GetSystem<CombatTargetSystem>().Unregister(mEntity);
		}
	}

	public class RegisterEnemyHealthCommand : AbstractCommand
	{
		private readonly CombatEntity mEnemy;
		private readonly float mBaseHealth;

		public RegisterEnemyHealthCommand(CombatEntity enemy, float baseHealth)
		{
			mEnemy = enemy;
			mBaseHealth = baseHealth;
		}

		protected override void OnExecute()
		{
			this.GetSystem<EnemyHealthSystem>().Register(mEnemy, mBaseHealth);
		}
	}

	public class UpgradeWeaponCommand : AbstractCommand
	{
		private readonly int mWeaponRuntimeId;

		public UpgradeWeaponCommand(int weaponRuntimeId)
		{
			mWeaponRuntimeId = weaponRuntimeId;
		}

		protected override void OnExecute()
		{
			this.GetSystem<PlayerLoadoutSystem>().UpgradeWeapon(mWeaponRuntimeId);
		}
	}

	public class EvolveWeaponCommand : AbstractCommand
	{
		private readonly int mWeaponRuntimeId;

		public EvolveWeaponCommand(int weaponRuntimeId)
		{
			mWeaponRuntimeId = weaponRuntimeId;
		}

		protected override void OnExecute()
		{
			this.GetSystem<PlayerLoadoutSystem>().TryEvolveWeapon(mWeaponRuntimeId);
		}
	}

	public class ReplaceWeaponCommand : AbstractCommand
	{
		private readonly int mWeaponRuntimeId;
		private readonly int mTargetWeaponId;

		public ReplaceWeaponCommand(int weaponRuntimeId, int targetWeaponId)
		{
			mWeaponRuntimeId = weaponRuntimeId;
			mTargetWeaponId = targetWeaponId;
		}

		protected override void OnExecute()
		{
			this.GetSystem<PlayerLoadoutSystem>().ReplaceWeapon(mWeaponRuntimeId, mTargetWeaponId);
		}
	}

	public class ReplaceWeaponAttacksCommand : AbstractCommand
	{
		private readonly int mWeaponRuntimeId;
		private readonly IReadOnlyList<int> mAttackIds;

		public ReplaceWeaponAttacksCommand(int weaponRuntimeId, IReadOnlyList<int> attackIds)
		{
			mWeaponRuntimeId = weaponRuntimeId;
			mAttackIds = attackIds;
		}

		protected override void OnExecute()
		{
			this.GetSystem<PlayerLoadoutSystem>().ReplaceWeaponAttacks(mWeaponRuntimeId, mAttackIds);
		}
	}

	public class RegisterPlayerCommand : AbstractCommand
	{
		private readonly Vector2 mInitialPosition;
		private readonly CharacterConfig mCharacter;

		public RegisterPlayerCommand(Vector2 initialPosition, CharacterConfig character)
		{
			mInitialPosition = initialPosition;
			mCharacter = character;
		}

		protected override void OnExecute()
		{
			this.GetSystem<PlayerSystem>().Register(mInitialPosition, mCharacter);
		}
	}

	public class UnregisterPlayerCommand : AbstractCommand
	{
		protected override void OnExecute()
		{
			this.GetSystem<PlayerSystem>().Unregister();
		}
	}

	public class SetMovementInputCommand : AbstractCommand
	{
		private readonly Vector2 mMovement;

		public SetMovementInputCommand(Vector2 movement)
		{
			mMovement = movement;
		}

		protected override void OnExecute()
		{
			this.GetSystem<InputSystem>().SetMovement(mMovement);
		}
	}

	public class MovePlayerCommand : AbstractCommand
	{
		protected override void OnExecute()
		{
			this.GetSystem<PlayerSystem>().Move();
		}
	}

	public class SelectCharacterCommand : AbstractCommand
	{
		private readonly int mCharacterId;

		public SelectCharacterCommand(int characterId)
		{
			mCharacterId = characterId;
		}

		protected override void OnExecute()
		{
			this.GetSystem<CharacterSelectionSystem>().Select(mCharacterId);
		}
	}

	public class ConfirmCharacterSelectionCommand : AbstractCommand
	{
		protected override void OnExecute()
		{
			this.GetSystem<CharacterSelectionSystem>().ConfirmSelection();
		}
	}
}
