using QFramework;

namespace HaoFuSurvivor
{
	public class DamageSystem : AbstractSystem
	{
		public void ApplyDamage(CombatEntity target, float damage)
		{
			if (!this.GetSystem<RunTimerSystem>().IsRunning()) return;
			var breakable = target == null ? null : target.GetComponent<BreakableObjectView>();
			if (breakable != null)
			{
				this.GetSystem<BreakableObjectSystem>().ApplyDamage(breakable, damage);
				return;
			}

			if (target != null && target.Faction == CombatFaction.Player)
			{
				this.GetSystem<PlayerSystem>().ApplyDamage(damage);
			}
			else if (target != null && target.Faction == CombatFaction.Enemy)
			{
				this.GetSystem<EnemyHealthSystem>().ApplyDamage(target, damage);
			}
		}

		protected override void OnInit()
		{
		}
	}
}
