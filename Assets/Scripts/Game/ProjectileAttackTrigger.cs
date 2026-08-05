using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class ProjectileAttackTrigger : MonoBehaviour, IController, IAttackTrigger
	{
		private int mAttackId;
		private CombatFaction mOwnerFaction;
		private float mAttackRange;
		private bool mIsRegistered;
		public int AttackId => mAttackId;

		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		public void Initialize(int attackId, CombatFaction ownerFaction, float attackRange)
		{
			mAttackId = attackId;
			mOwnerFaction = ownerFaction;
			mAttackRange = Mathf.Max(0f, attackRange);
			mIsRegistered = true;
			this.SendCommand(new RegisterAttackCommand(GetInstanceID(), attackId, gameObject, ownerFaction));
		}

		private void Update()
		{
			var target = this.SendQuery(new FindClosestCombatTargetQuery(transform.position, mOwnerFaction, mAttackRange));
			if (target != null) this.SendCommand(new TryExecuteAttackCommand(GetInstanceID(), target));
		}

		private void OnDisable()
		{
			if (!mIsRegistered) return;
			this.SendCommand(new UnregisterAttackCommand(GetInstanceID()));
			mIsRegistered = false;
		}
	}
}
