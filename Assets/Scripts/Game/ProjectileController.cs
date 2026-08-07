using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
	public class ProjectileController : MonoBehaviour, IController
	{
		private Rigidbody2D mRigidbody;
		private Vector2 mDirection;
		private CombatFaction mOwnerFaction;
		private float mDamage;
		private float mMoveSpeed;
		private float mRemainingLifetime;
		private bool mIsActive;

		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		private void Awake()
		{
			mRigidbody = GetComponent<Rigidbody2D>();
			mRigidbody.bodyType = RigidbodyType2D.Kinematic;
			mRigidbody.gravityScale = 0f;
		}

		public void Launch(Vector2 position, Vector2 direction, CombatFaction ownerFaction, float damage,
			float moveSpeed, float lifetime)
		{
			transform.position = position;
			mDirection = direction.normalized;
			mOwnerFaction = ownerFaction;
			mDamage = damage;
			mMoveSpeed = Mathf.Max(0f, moveSpeed);
			mRemainingLifetime = Mathf.Max(0.01f, lifetime);
			mIsActive = true;
		}

		private void FixedUpdate()
		{
			if (!mIsActive) return;
			var deltaTime = this.SendQuery(new GetRunTimeStateQuery()).FixedDeltaTime;
			if (deltaTime <= 0f) return;
			mRigidbody.MovePosition(mRigidbody.position + mDirection * mMoveSpeed * deltaTime);
		}

		private void Update()
		{
			if (!mIsActive) return;
			var deltaTime = this.SendQuery(new GetRunTimeStateQuery()).DeltaTime;
			if (deltaTime <= 0f) return;
			mRemainingLifetime -= deltaTime;
			if (mRemainingLifetime <= 0f) ProjectileFactory.Instance.Release(this);
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (!mIsActive || !this.SendQuery(new GetRunTimeStateQuery()).IsRunning) return;
			var target = other.GetComponentInParent<CombatEntity>();
			if (target == null || target.Faction == mOwnerFaction) return;
			this.SendCommand(new ApplyCombatDamageCommand(target, mDamage));
			ProjectileFactory.Instance.Release(this);
		}

		public void ResetState()
		{
			mIsActive = false;
			mRigidbody.velocity = Vector2.zero;
		}
	}
}
