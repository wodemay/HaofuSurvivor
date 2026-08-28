using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class MapEventTriggerView : MonoBehaviour, IController
	{
		private string mStableId;

		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		public void Configure(string stableId, float radius)
		{
			mStableId = stableId;
			var collider = GetComponent<CircleCollider2D>();
			if (collider == null) return;
			collider.isTrigger = true;
			collider.radius = Mathf.Max(0.5f, radius);
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (IsPlayer(other)) this.SendCommand(new SetMapEventPlayerPresenceCommand(mStableId, true));
		}

		private void OnTriggerExit2D(Collider2D other)
		{
			if (IsPlayer(other)) this.SendCommand(new SetMapEventPlayerPresenceCommand(mStableId, false));
		}

		private static bool IsPlayer(Collider2D other)
		{
			var player = GameArchitecture.Interface.GetModel<PlayerModel>();
			return player.RuntimeRoot != null && other.attachedRigidbody != null &&
				other.attachedRigidbody.gameObject == player.RuntimeRoot;
		}
	}
}
