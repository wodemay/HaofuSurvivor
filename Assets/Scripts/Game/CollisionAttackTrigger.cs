using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class CollisionAttackTrigger : MonoBehaviour, IController, IAttackTrigger
	{
		private int mAttackId;
		private CombatFaction mOwnerFaction;
		private bool mIsRegistered;
		public int AttackId => mAttackId;

		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		public void Initialize(int attackId, CombatFaction ownerFaction)
		{
			mAttackId = attackId;
			mOwnerFaction = ownerFaction;
			mIsRegistered = true;
			this.SendCommand(new RegisterAttackCommand(GetInstanceID(), attackId, gameObject, ownerFaction));
		}

		private void OnTriggerStay2D(Collider2D other)
		{
			var target = other.GetComponentInParent<CombatEntity>();
			if (target == null || target.Faction == mOwnerFaction) return;
			this.SendCommand(new TryExecuteAttackCommand(GetInstanceID(), target));
		}

		private void OnDisable()
		{
			if (!mIsRegistered) return;
			this.SendCommand(new UnregisterAttackCommand(GetInstanceID()));
			mIsRegistered = false;
		}
	}
}
