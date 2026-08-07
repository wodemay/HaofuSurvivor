using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class CollisionAttackTrigger : MonoBehaviour, IController, IAttackTrigger
	{
		private int mAttackId;
		private int mWeaponRuntimeId;
		private CombatFaction mOwnerFaction;
		private bool mIsRegistered;
		public int AttackId => mAttackId;
		public int WeaponRuntimeId => mWeaponRuntimeId;
		public bool IsRegistered => mIsRegistered;

		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		public void Initialize(int attackId, CombatFaction ownerFaction, int weaponRuntimeId = 0)
		{
			mAttackId = attackId;
			mWeaponRuntimeId = weaponRuntimeId;
			mOwnerFaction = ownerFaction;
			mIsRegistered = true;
			this.SendCommand(new RegisterAttackCommand(GetInstanceID(), attackId, gameObject, ownerFaction));
		}

		private void OnTriggerStay2D(Collider2D other)
		{
			if (!this.SendQuery(new GetRunTimeStateQuery()).IsRunning) return;

			var target = other.GetComponentInParent<CombatEntity>();
			if (target == null || target.Faction == mOwnerFaction) return;
			this.SendCommand(new TryExecuteAttackCommand(GetInstanceID(), target));
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
