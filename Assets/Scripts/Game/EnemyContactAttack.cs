using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class EnemyContactAttack : MonoBehaviour, IController
	{
		private int mAttackId;
		private bool mIsRegistered;

		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		public void Initialize(int attackId)
		{
			mAttackId = attackId;
			mIsRegistered = true;
			this.SendCommand(new RegisterEnemyContactAttackCommand(GetInstanceID(), mAttackId));
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (other.GetComponentInParent<PlayerController>() == null) return;
			this.SendCommand(new SetEnemyContactStateCommand(GetInstanceID(), true));
		}

		private void OnTriggerExit2D(Collider2D other)
		{
			if (other.GetComponentInParent<PlayerController>() == null) return;
			this.SendCommand(new SetEnemyContactStateCommand(GetInstanceID(), false));
		}

		private void OnDisable()
		{
			if (!mIsRegistered) return;
			this.SendCommand(new UnregisterEnemyContactAttackCommand(GetInstanceID()));
			mIsRegistered = false;
		}
	}
}
