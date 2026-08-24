using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class PlayerSystem : AbstractSystem, IRunUpdateable, IRunFixedUpdateable
	{
		private Rigidbody2D mRigidbody;
		private readonly RaycastHit2D[] mMapHits = new RaycastHit2D[16];

		public void Register(GameObject runtimeRoot, Vector2 initialPosition, CharacterConfig character)
		{
			var playerModel = this.GetModel<PlayerModel>();
			var statModel = this.GetModel<PlayerStatModel>();

			statModel.BaseMaxHealth = statModel.MaxHealth = Mathf.Max(1f, character.MaxHealth);
			statModel.BaseMoveSpeed = statModel.MoveSpeed = Mathf.Max(0f, character.MoveSpeed);
			statModel.BaseAttackPower = statModel.AttackPower = Mathf.Max(0f, character.AttackPower);
			statModel.BaseExperienceAbsorbRadius = statModel.ExperienceAbsorbRadius = Mathf.Max(0f, character.BaseExperienceAbsorbRadius);
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
			this.GetSystem<PlayerStatUpgradeSystem>().Reset();
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
			MoveBy(direction * this.GetSystem<StatSystem>().GetMoveSpeed() * deltaTime);
		}

		public void MoveBy(Vector2 delta)
		{
			var playerModel = this.GetModel<PlayerModel>();
			if (!playerModel.IsRegistered || playerModel.IsDead) return;
			playerModel.Position = ResolveMapCollision(playerModel.Position + delta);
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

		private Vector2 ResolveMapCollision(Vector2 target)
		{
			if (mRigidbody == null) return target;
			var origin = mRigidbody.position;
			var offset = target - origin;
			var distance = offset.magnitude;
			if (distance <= 0.0001f) return target;
			var filter = new ContactFilter2D();
			filter.NoFilter();
			filter.useTriggers = false;
			var count = mRigidbody.Cast(offset / distance, filter, mMapHits, distance);
			var nearestDistance = float.MaxValue;
			var nearestNormal = Vector2.zero;
			for (var i = 0; i < count; i++)
			{
				var collider = mMapHits[i].collider;
				if (!MapColliderUtility.IsMoveBlocker(collider)) continue;
				if (Vector2.Dot(offset, mMapHits[i].normal) >= 0f) continue;
				if (mMapHits[i].distance >= nearestDistance) continue;
				nearestDistance = mMapHits[i].distance;
				nearestNormal = mMapHits[i].normal;
			}
			if (nearestDistance == float.MaxValue) return target;
			var contactPosition = origin + offset / distance * Mathf.Max(0f, nearestDistance - 0.001f);
			var remaining = target - contactPosition;
			return contactPosition + (remaining - nearestNormal * Vector2.Dot(remaining, nearestNormal));
		}

		public float RestoreHealth(float amount, bool applyRecoveryEfficiency = true)
		{
			var playerModel = this.GetModel<PlayerModel>();
			if (!playerModel.IsRegistered || playerModel.IsDead || amount <= 0f) return 0f;
			var multiplier = applyRecoveryEfficiency ? this.GetSystem<StatSystem>().GetRecoveryEfficiencyMultiplier() : 1f;
			var recovered = Mathf.Min(this.GetSystem<StatSystem>().GetMaxHealth() - playerModel.CurrentHealth, amount * multiplier);
			if (recovered <= 0f) return 0f;
			playerModel.CurrentHealth += recovered;
			this.SendEvent(new PlayerHealedEvent(recovered, playerModel.CurrentHealth));
			return recovered;
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
