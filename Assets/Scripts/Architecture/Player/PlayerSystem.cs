using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class PlayerSystem : AbstractSystem, IRunUpdateable, IRunFixedUpdateable
	{
		private Rigidbody2D mRigidbody;

		public void Register(GameObject runtimeRoot, Vector2 initialPosition, CharacterConfig character)
		{
			var playerModel = this.GetModel<PlayerModel>();
			var statModel = this.GetModel<PlayerStatModel>();

			statModel.MaxHealth = Mathf.Max(1f, character.MaxHealth);
			statModel.MoveSpeed = Mathf.Max(0f, character.MoveSpeed);
			statModel.AttackPower = Mathf.Max(0f, character.AttackPower);
			statModel.ExperienceAbsorbRadius = Mathf.Max(0f, character.BaseExperienceAbsorbRadius);
			statModel.ExperienceAbsorbAcceleration = Mathf.Max(0.01f, character.BaseExperienceAbsorbAcceleration);
			statModel.ExperienceAbsorbMaxSpeed = Mathf.Max(0.01f, character.BaseExperienceAbsorbMaxSpeed);
			playerModel.CharacterId = character.Id;
			playerModel.Position = initialPosition;
			playerModel.RuntimeRoot = runtimeRoot;
			mRigidbody = runtimeRoot.GetComponent<Rigidbody2D>();
			playerModel.CurrentHealth = statModel.MaxHealth;
			playerModel.DamageInvulnerabilityRemaining = 0f;
			playerModel.DodgeInvulnerabilityRemaining = 0f;
			playerModel.IsDead = false;
			playerModel.IsRegistered = true;
		}

		public void Unregister(GameObject runtimeRoot)
		{
			var playerModel = this.GetModel<PlayerModel>();
			if (playerModel.RuntimeRoot != runtimeRoot) return;

			playerModel.IsRegistered = false;
			playerModel.IsDead = false;
			playerModel.RuntimeRoot = null;
			mRigidbody = null;
			this.GetModel<InputModel>().Movement = Vector2.zero;
			this.GetSystem<PlayerLoadoutSystem>().Reset();
		}

		private void Move(float deltaTime)
		{
			var playerModel = this.GetModel<PlayerModel>();
			if (!playerModel.IsRegistered || playerModel.IsDead) return;
			if (this.GetModel<DodgeModel>().Runtime?.IsActive == true) return;

			var direction = this.GetModel<InputModel>().Movement;
			playerModel.Position += direction * this.GetSystem<StatSystem>().GetMoveSpeed() * deltaTime;
		}

		private void SyncRuntimePosition()
		{
			var playerModel = this.GetModel<PlayerModel>();
			if (mRigidbody != null) mRigidbody.MovePosition(playerModel.Position);
			else if (playerModel.RuntimeRoot != null) playerModel.RuntimeRoot.transform.position = playerModel.Position;
		}

		public void ApplyDamage(float damage)
		{
			var playerModel = this.GetModel<PlayerModel>();
			if (!playerModel.IsRegistered || playerModel.IsDead || damage <= 0f || playerModel.DamageInvulnerabilityRemaining > 0f || playerModel.DodgeInvulnerabilityRemaining > 0f) return;

			playerModel.CurrentHealth = Mathf.Max(0f, playerModel.CurrentHealth - damage);
			playerModel.DamageInvulnerabilityRemaining = this.GetModel<PlayerStatModel>().DamageInvulnerabilityDuration;
			this.SendEvent(new PlayerDamagedEvent(damage, playerModel.CurrentHealth));

			if (playerModel.CurrentHealth > 0f) return;

			playerModel.IsDead = true;
			this.SendEvent(new PlayerDiedEvent());
			this.GetSystem<RunSystem>().EndWithDefeat();
		}

		public void OnRunUpdate(float deltaTime)
		{
			var playerModel = this.GetModel<PlayerModel>();
			playerModel.DamageInvulnerabilityRemaining = Mathf.Max(0f, playerModel.DamageInvulnerabilityRemaining - deltaTime);
			playerModel.DodgeInvulnerabilityRemaining = Mathf.Max(0f, playerModel.DodgeInvulnerabilityRemaining - deltaTime);
		}

		public void OnRunFixedUpdate(float deltaTime)
		{
			Move(deltaTime);
			SyncRuntimePosition();
		}

		protected override void OnInit()
		{
		}
	}
}
