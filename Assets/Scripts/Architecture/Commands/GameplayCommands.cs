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
		private readonly float mDeltaTime;

		public TickEnemiesCommand(float deltaTime)
		{
			mDeltaTime = deltaTime;
		}

		protected override void OnExecute()
		{
			this.GetSystem<EnemySystem>().Tick(mDeltaTime);
		}
	}

	public class TickEnemyAttacksCommand : AbstractCommand
	{
		private readonly float mDeltaTime;

		public TickEnemyAttacksCommand(float deltaTime)
		{
			mDeltaTime = deltaTime;
		}

		protected override void OnExecute()
		{
			this.GetSystem<EnemyAttackSystem>().Tick(mDeltaTime);
		}
	}

	public class RegisterEnemyContactAttackCommand : AbstractCommand
	{
		private readonly int mRuntimeId;
		private readonly int mAttackId;

		public RegisterEnemyContactAttackCommand(int runtimeId, int attackId)
		{
			mRuntimeId = runtimeId;
			mAttackId = attackId;
		}

		protected override void OnExecute()
		{
			this.GetSystem<EnemyAttackSystem>().RegisterContactAttack(mRuntimeId, mAttackId);
		}
	}

	public class UnregisterEnemyContactAttackCommand : AbstractCommand
	{
		private readonly int mRuntimeId;

		public UnregisterEnemyContactAttackCommand(int runtimeId)
		{
			mRuntimeId = runtimeId;
		}

		protected override void OnExecute()
		{
			this.GetSystem<EnemyAttackSystem>().UnregisterContactAttack(mRuntimeId);
		}
	}

	public class SetEnemyContactStateCommand : AbstractCommand
	{
		private readonly int mRuntimeId;
		private readonly bool mIsContacting;

		public SetEnemyContactStateCommand(int runtimeId, bool isContacting)
		{
			mRuntimeId = runtimeId;
			mIsContacting = isContacting;
		}

		protected override void OnExecute()
		{
			this.GetSystem<EnemyAttackSystem>().SetContactState(mRuntimeId, mIsContacting);
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
		private readonly float mDeltaTime;

		public MovePlayerCommand(float deltaTime)
		{
			mDeltaTime = deltaTime;
		}

		protected override void OnExecute()
		{
			this.GetSystem<PlayerSystem>().Move(mDeltaTime);
		}
	}

	public class ApplyPlayerDamageCommand : AbstractCommand
	{
		private readonly float mDamage;

		public ApplyPlayerDamageCommand(float damage)
		{
			mDamage = damage;
		}

		protected override void OnExecute()
		{
			this.GetSystem<DamageSystem>().ApplyPlayerDamage(mDamage);
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
