using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class PlayerSystem : AbstractSystem
	{
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
			playerModel.CurrentHealth = statModel.MaxHealth;
			playerModel.DamageInvulnerabilityRemaining = 0f;
			playerModel.IsDead = false;
			playerModel.DamageInvulnerabilityRemaining = 0f;
			playerModel.IsRegistered = true;
		}

		public void Unregister(GameObject runtimeRoot)
		{
			var playerModel = this.GetModel<PlayerModel>();
			if (playerModel.RuntimeRoot != runtimeRoot) return;

			playerModel.IsRegistered = false;
			playerModel.IsDead = false;
			playerModel.RuntimeRoot = null;
			this.GetModel<InputModel>().Movement = Vector2.zero;
			this.GetSystem<PlayerLoadoutSystem>().Reset();
		}

		public void Move()
		{
			var deltaTime = this.GetModel<RunTimerModel>().FixedDeltaTime;
			var playerModel = this.GetModel<PlayerModel>();
			if (!playerModel.IsRegistered || playerModel.IsDead) return;
			if (deltaTime <= 0f || !this.GetSystem<RunTimerSystem>().IsRunning()) return;

			var direction = this.GetModel<InputModel>().Movement;
			playerModel.Position += direction * this.GetSystem<StatSystem>().GetMoveSpeed() * deltaTime;
		}

		public void ApplyDamage(float damage)
		{
			var playerModel = this.GetModel<PlayerModel>();
			if (!playerModel.IsRegistered || playerModel.IsDead || damage <= 0f || playerModel.DamageInvulnerabilityRemaining > 0f) return;

			playerModel.CurrentHealth = Mathf.Max(0f, playerModel.CurrentHealth - damage);
			playerModel.DamageInvulnerabilityRemaining = this.GetModel<PlayerStatModel>().DamageInvulnerabilityDuration;
			this.SendEvent(new PlayerDamagedEvent(damage, playerModel.CurrentHealth));

			if (playerModel.CurrentHealth > 0f) return;

			playerModel.IsDead = true;
			this.SendEvent(new PlayerDiedEvent());
			this.GetSystem<RunSystem>().EndWithDefeat();
		}

		public void AdvanceDamageInvulnerability()
		{
			var deltaTime = this.GetModel<RunTimerModel>().DeltaTime;
			if (deltaTime <= 0f) return;
			var playerModel = this.GetModel<PlayerModel>();
			playerModel.DamageInvulnerabilityRemaining = Mathf.Max(0f, playerModel.DamageInvulnerabilityRemaining - deltaTime);
		}

		protected override void OnInit()
		{
		}
	}
}
