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

		protected CombatFaction OwnerFaction => mOwnerFaction;
		protected float Damage => mDamage;

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

		public virtual void ConfigureParameters(ProjectileAttackParameterConfig parameters)
		{
		}

		public ProjectileSaveData GetSaveData(int attackId)
		{
			return new ProjectileSaveData
			{
				AttackId = attackId,
				PositionX = transform.position.x,
				PositionY = transform.position.y,
				DirectionX = mDirection.x,
				DirectionY = mDirection.y,
				OwnerFaction = (int)mOwnerFaction,
				Damage = mDamage,
				MoveSpeed = mMoveSpeed,
				RemainingLifetime = mRemainingLifetime,
				RemainingPierce = mRemainingPierce
			};
		}

		public void Restore(ProjectileSaveData data)
		{
			if (data == null) return;
			Launch(new Vector2(data.PositionX, data.PositionY), new Vector2(data.DirectionX, data.DirectionY),
				(CombatFaction)data.OwnerFaction, data.Damage, data.MoveSpeed, data.RemainingLifetime, data.RemainingPierce);
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
			if (MapColliderUtility.IsProjectileBlocker(other))
			{
				ResolveObstacleHit();
				return;
			}
			var target = other.GetComponentInParent<CombatEntity>();
			if (target == null || target.Faction == mOwnerFaction || !mHitTargetIds.Add(target.GetInstanceID())) return;
			ResolveHit(target);
			if (mRemainingPierce-- <= 0) ProjectileFactory.Instance.Release(this);
		}

		protected virtual void ResolveHit(CombatEntity target)
		{
			this.SendCommand(new ApplyCombatDamageCommand(target, mDamage));
		}

		protected virtual void ResolveObstacleHit()
		{
			ProjectileFactory.Instance.Release(this);
		}

		public virtual void ResetState()
		{
			mIsActive = false;
			mRigidbody.velocity = Vector2.zero;
			mRemainingPierce = 0;
			mHitTargetIds.Clear();
		}
	}
}
