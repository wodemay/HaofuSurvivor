using QFramework;

namespace HaoFuSurvivor
{
	public class StatSystem : AbstractSystem
	{
		public float GetMaxHealth() => this.GetModel<PlayerStatModel>().MaxHealth;
		public float GetMoveSpeed() => this.GetModel<PlayerStatModel>().MoveSpeed;
		public float GetAttackPower() => this.GetModel<PlayerStatModel>().AttackPower;

		protected override void OnInit()
		{
		}
	}
}
