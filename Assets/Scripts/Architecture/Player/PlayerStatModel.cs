namespace HaoFuSurvivor
{
	public class PlayerStatModel : QFramework.AbstractModel
	{
		public float BaseMaxHealth { get; internal set; }
		public float BaseMoveSpeed { get; internal set; }
		public float BaseAttackPower { get; internal set; }
		public float BaseExperienceAbsorbRadius { get; internal set; }
		public float MaxHealth { get; internal set; }
		public float MoveSpeed { get; internal set; }
		public float AttackPower { get; internal set; }
		public float DamageInvulnerabilityDuration { get; internal set; }
		public float ExperienceAbsorbRadius { get; internal set; }
		public float ExperienceAbsorbAcceleration { get; internal set; }
		public float ExperienceAbsorbMaxSpeed { get; internal set; }
		public float AttackDamageMultiplier { get; internal set; }
		public float CooldownMultiplier { get; internal set; }
		public float ExperienceMultiplier { get; internal set; }
		public float NaturalHealthRegenerationRatio { get; internal set; }
		public float RecoveryEfficiencyMultiplier { get; internal set; }

		protected override void OnInit()
		{
			BaseMaxHealth = MaxHealth = 100f;
			BaseMoveSpeed = MoveSpeed = 5f;
			BaseAttackPower = AttackPower = 10f;
			DamageInvulnerabilityDuration = 0f;
			BaseExperienceAbsorbRadius = ExperienceAbsorbRadius = 2.5f;
			ExperienceAbsorbAcceleration = 50f;
			ExperienceAbsorbMaxSpeed = 30f;
			AttackDamageMultiplier = 1f;
			CooldownMultiplier = 1f;
			ExperienceMultiplier = 1f;
			NaturalHealthRegenerationRatio = 0f;
			RecoveryEfficiencyMultiplier = 1f;
		}
	}
}
