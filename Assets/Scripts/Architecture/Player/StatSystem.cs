using QFramework;

namespace HaoFuSurvivor
{
	public class StatSystem : AbstractSystem
	{
		public float GetMaxHealth() => this.GetModel<PlayerStatModel>().MaxHealth;
		public float GetMoveSpeed() => this.GetModel<PlayerStatModel>().MoveSpeed;
		public float GetAttackPower() => this.GetModel<PlayerStatModel>().AttackPower;
		public float GetAttackDamageMultiplier() => this.GetModel<PlayerStatModel>().AttackDamageMultiplier * this.GetSystem<CharacterExclusivePerkSystem>().GetDamageMultiplier();
		public float GetCooldownMultiplier() => this.GetModel<PlayerStatModel>().CooldownMultiplier;
		public float GetExperienceMultiplier() => this.GetModel<PlayerStatModel>().ExperienceMultiplier;
		public float GetNaturalHealthRegenerationRatio() => this.GetModel<PlayerStatModel>().NaturalHealthRegenerationRatio;
		public float GetRecoveryEfficiencyMultiplier() => this.GetModel<PlayerStatModel>().RecoveryEfficiencyMultiplier;

		protected override void OnInit()
		{
		}
	}
}
