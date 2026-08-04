using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class ContactAttackTrigger : MonoBehaviour, IController
	{
		private int mAttackId;
		private CombatFaction mOwnerFaction;
		private bool mIsRegistered;

		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		public void Initialize(int attackId, CombatFaction ownerFaction)
		{
			mAttackId = attackId;
			mOwnerFaction = ownerFaction;
			mIsRegistered = true;
			this.SendCommand(new RegisterAttackCommand(GetInstanceID(), attackId, ownerFaction));
		}

		private void OnTriggerStay2D(Collider2D other)
		{
			var target = other.GetComponentInParent<CombatEntity>();
			if (target == null || target.Faction == mOwnerFaction) return;
			this.SendCommand(new TryExecuteAttackCommand(GetInstanceID(), target.Faction));
		}

		private void OnDisable()
		{
			if (!mIsRegistered) return;
			this.SendCommand(new UnregisterAttackCommand(GetInstanceID()));
			mIsRegistered = false;
		}
	}
}
