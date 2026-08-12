using UnityEngine;

namespace HaoFuSurvivor
{
	public class PlayerModel : QFramework.AbstractModel
	{
		public bool IsRegistered { get; internal set; }
		public bool IsDead { get; internal set; }
		public int CharacterId { get; internal set; }
		public float CurrentHealth { get; internal set; }
		public float DamageInvulnerabilityRemaining { get; internal set; }
		public Vector2 Position { get; internal set; }
		public GameObject RuntimeRoot { get; internal set; }

		protected override void OnInit()
		{
			IsRegistered = false;
			IsDead = false;
			CharacterId = -1;
			CurrentHealth = 0f;
			DamageInvulnerabilityRemaining = 0f;
			Position = Vector2.zero;
			RuntimeRoot = null;
		}
	}
}
