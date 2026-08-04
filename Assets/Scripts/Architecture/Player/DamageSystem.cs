using QFramework;

namespace HaoFuSurvivor
{
	public class DamageSystem : AbstractSystem
	{
		public void ApplyDamage(CombatFaction targetFaction, float damage)
		{
			if (targetFaction == CombatFaction.Player)
			{
				this.GetSystem<PlayerSystem>().ApplyDamage(damage);
			}
		}

		protected override void OnInit()
		{
		}
	}
}
