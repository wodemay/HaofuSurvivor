using QFramework;
using UnityEngine;
using System.Collections.Generic;

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
		private int mRemainingPierce;
		private readonly HashSet<int> mHitTargetIds = new();
		private bool mIsActive;

		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		private void Awake()
		{
			mRigidbody = GetComponent<Rigidbody2D>();
			mRigidbody.bodyType = RigidbodyType2D.Kinematic;
			mRigidbody.gravityScale = 0f;
		}

		public void Launch(Vector2 position, Vector2 direction, CombatFaction ownerFaction, float damage,
			float moveSpeed, float lifetime, int pierce)
		{
			transform.position = position;
			mDirection = direction.normalized;
			mOwnerFaction = ownerFaction;
			mDamage = damage;
			mMoveSpeed = Mathf.Max(0f, moveSpeed);
			mRemainingLifetime = Mathf.Max(0.01f, lifetime);
			mRemainingPierce = Mathf.Max(0, pierce);
			mHitTargetIds.Clear();
			mIsActive = true;
		}

		public void AdvanceFixed(float deltaTime)
		{
			if (!mIsActive) return;
			mRigidbody.MovePosition(mRigidbody.position + mDirection * mMoveSpeed * deltaTime);
		}

		public void Advance(float deltaTime)
		{
			if (!mIsActive) return;
			mRemainingLifetime -= deltaTime;
			if (mRemainingLifetime <= 0f) ProjectileFactory.Instance.Release(this);
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (!mIsActive || !this.SendQuery(new GetRunTimeStateQuery()).IsRunning) return;
			var target = other.GetComponentInParent<CombatEntity>();
			if (target == null || target.Faction == mOwnerFaction || !mHitTargetIds.Add(target.GetInstanceID())) return;
			this.SendCommand(new ApplyCombatDamageCommand(target, mDamage));
			if (mRemainingPierce-- <= 0) ProjectileFactory.Instance.Release(this);
		}

		public void ResetState()
		{
			mIsActive = false;
			mRigidbody.velocity = Vector2.zero;
			mRemainingPierce = 0;
			mHitTargetIds.Clear();
		}
	}
}
