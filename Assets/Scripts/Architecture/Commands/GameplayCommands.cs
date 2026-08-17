using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
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

	public class TickGameLoopCommand : AbstractCommand
	{
		private readonly float mDeltaTime;

		public TickGameLoopCommand(float deltaTime)
		{
			mDeltaTime = deltaTime;
		}

		protected override void OnExecute()
		{
			this.GetSystem<GameLoopSystem>().TickFrame(mDeltaTime);
		}
	}

	public class TickGamePhysicsCommand : AbstractCommand
	{
		private readonly float mFixedDeltaTime;

		public TickGamePhysicsCommand(float fixedDeltaTime)
		{
			mFixedDeltaTime = fixedDeltaTime;
		}

		protected override void OnExecute()
		{
			this.GetSystem<GameLoopSystem>().TickFixed(mFixedDeltaTime);
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


	public class CompleteLevelUpWeaponCommand : AbstractCommand
	{
		private readonly int mWeaponRuntimeId;
		private readonly bool mIsEvolution;
		private readonly bool mIsDodge;

		public CompleteLevelUpWeaponCommand(int weaponRuntimeId, bool isEvolution, bool isDodge = false)
		{
			mWeaponRuntimeId = weaponRuntimeId;
			mIsEvolution = isEvolution;
			mIsDodge = isDodge;
		}

		protected override void OnExecute()
		{
			this.GetSystem<LevelUpSystem>().CompleteWeaponUpgrade(mWeaponRuntimeId, mIsEvolution, mIsDodge);
		}
	}

	public class ExitRunToCharacterSelectionCommand : AbstractCommand
	{
		protected override void OnExecute()
		{
			this.GetSystem<RunSystem>().ExitToCharacterSelection();
		}
	}

	public class RequestDodgeCommand : AbstractCommand
	{
		protected override void OnExecute() { this.GetSystem<DodgeSystem>().TryStart(); }
	}

	public class RequestSkillCommand : AbstractCommand
	{
		protected override void OnExecute() { this.GetSystem<PlayerLoadoutSystem>().TryUseSkills(); }
	}

	public class UpgradeDodgeCommand : AbstractCommand
	{
		protected override void OnExecute() { this.GetSystem<DodgeSystem>().Upgrade(); }
	}

	public class ContinueSavedRunCommand : AbstractCommand
	{
		protected override void OnExecute()
		{
			this.GetSystem<RunSystem>().ContinueSavedRun();
		}
	}

	public class RestartSelectedCharacterRunCommand : AbstractCommand
	{
		protected override void OnExecute()
		{
			this.GetSystem<RunSystem>().RestartSelectedCharacterRun();
		}
	}

	public class RegisterAttackCommand : AbstractCommand
	{
		private readonly int mRuntimeId;
		private readonly int mAttackId;
		private readonly GameObject mOwner;
		private readonly CombatFaction mOwnerFaction;
		private readonly int mWeaponRuntimeId;

		public RegisterAttackCommand(int runtimeId, int attackId, GameObject owner, CombatFaction ownerFaction, int weaponRuntimeId = 0)
		{
			mRuntimeId = runtimeId;
			mAttackId = attackId;
			mOwner = owner;
			mOwnerFaction = ownerFaction;
			mWeaponRuntimeId = weaponRuntimeId;
		}

		protected override void OnExecute()
		{
			this.GetSystem<AttackSystem>().Register(mRuntimeId, mAttackId, mOwner, mOwnerFaction, mWeaponRuntimeId);
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
		private readonly GameObject mRuntimeRoot;
		private readonly Vector2 mInitialPosition;
		private readonly CharacterConfig mCharacter;

		public RegisterPlayerCommand(GameObject runtimeRoot, Vector2 initialPosition, CharacterConfig character)
		{
			mRuntimeRoot = runtimeRoot;
			mInitialPosition = initialPosition;
			mCharacter = character;
		}

		protected override void OnExecute()
		{
			this.GetSystem<PlayerSystem>().Register(mRuntimeRoot, mInitialPosition, mCharacter);
		}
	}

	public class UnregisterPlayerCommand : AbstractCommand
	{
		private readonly GameObject mRuntimeRoot;

		public UnregisterPlayerCommand(GameObject runtimeRoot)
		{
			mRuntimeRoot = runtimeRoot;
		}

		protected override void OnExecute()
		{
			this.GetSystem<PlayerSystem>().Unregister(mRuntimeRoot);
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
