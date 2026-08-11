namespace HaoFuSurvivor
{
	public class PlayerStatModel : QFramework.AbstractModel
	{
		public float MaxHealth { get; internal set; }
		public float MoveSpeed { get; internal set; }
		public float AttackPower { get; internal set; }
		public float DamageInvulnerabilityDuration { get; internal set; }
		public float ExperienceAbsorbRadius { get; internal set; }
		public float ExperienceAbsorbAcceleration { get; internal set; }
		public float ExperienceAbsorbMaxSpeed { get; internal set; }

		protected override void OnInit()
		{
			MaxHealth = 100f;
			MoveSpeed = 5f;
			AttackPower = 10f;
			DamageInvulnerabilityDuration = 0f;
			ExperienceAbsorbRadius = 2.5f;
			ExperienceAbsorbAcceleration = 50f;
			ExperienceAbsorbMaxSpeed = 30f;
		}
	}
}
