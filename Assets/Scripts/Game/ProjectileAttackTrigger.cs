using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class ProjectileAttackTrigger : MonoBehaviour, IController, IAttackTrigger
	{
		private int mAttackId;
		private int mWeaponRuntimeId;
		private CombatFaction mOwnerFaction;
		private float mAttackRange;
		private bool mIsRegistered;
		public int AttackId => mAttackId;
		public int WeaponRuntimeId => mWeaponRuntimeId;
		public bool IsRegistered => mIsRegistered;

		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		public void Initialize(int attackId, CombatFaction ownerFaction, float attackRange, int weaponRuntimeId = 0)
		{
			mAttackId = attackId;
			mWeaponRuntimeId = weaponRuntimeId;
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

		public void Unregister()
		{
			if (!mIsRegistered) return;
			this.SendCommand(new UnregisterAttackCommand(GetInstanceID()));
			mIsRegistered = false;
		}

		private void OnDisable()
		{
			Unregister();
		}
	}
}
