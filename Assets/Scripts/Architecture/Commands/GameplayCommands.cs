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

	public class TickAttacksCommand : AbstractCommand
	{
		private readonly float mDeltaTime;

		public TickAttacksCommand(float deltaTime)
		{
			mDeltaTime = deltaTime;
		}

		protected override void OnExecute()
		{
			this.GetSystem<AttackSystem>().Advance(mDeltaTime);
		}
	}

	public class TickPlayerDamageInvulnerabilityCommand : AbstractCommand
	{
		private readonly float mDeltaTime;

		public TickPlayerDamageInvulnerabilityCommand(float deltaTime)
		{
			mDeltaTime = deltaTime;
		}

		protected override void OnExecute()
		{
			this.GetSystem<PlayerSystem>().AdvanceDamageInvulnerability(mDeltaTime);
		}
	}

	public class RegisterAttackCommand : AbstractCommand
	{
		private readonly int mRuntimeId;
		private readonly int mAttackId;
		private readonly CombatFaction mOwnerFaction;

		public RegisterAttackCommand(int runtimeId, int attackId, CombatFaction ownerFaction)
		{
			mRuntimeId = runtimeId;
			mAttackId = attackId;
			mOwnerFaction = ownerFaction;
		}

		protected override void OnExecute()
		{
			this.GetSystem<AttackSystem>().Register(mRuntimeId, mAttackId, mOwnerFaction);
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
		private readonly CombatFaction mTargetFaction;

		public TryExecuteAttackCommand(int runtimeId, CombatFaction targetFaction)
		{
			mRuntimeId = runtimeId;
			mTargetFaction = targetFaction;
		}

		protected override void OnExecute()
		{
			this.GetSystem<AttackSystem>().TryExecute(mRuntimeId, mTargetFaction);
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
