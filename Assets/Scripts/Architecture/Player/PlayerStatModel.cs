namespace HaoFuSurvivor
{
	public class PlayerStatModel : QFramework.AbstractModel
	{
		public float MaxHealth { get; internal set; }
		public float MoveSpeed { get; internal set; }
		public float AttackPower { get; internal set; }

		protected override void OnInit()
		{
			MaxHealth = 100f;
			MoveSpeed = 5f;
			AttackPower = 10f;
		}
	}
}
