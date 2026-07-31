using QFramework;

namespace HaoFuSurvivor
{
	public class DamageSystem : AbstractSystem
	{
		public void ApplyPlayerDamage(float damage)
		{
			this.GetSystem<PlayerSystem>().ApplyDamage(damage);
		}

		protected override void OnInit()
		{
		}
	}
}
