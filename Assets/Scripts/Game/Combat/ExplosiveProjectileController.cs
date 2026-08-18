using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class ExplosiveProjectileController : ProjectileController
	{
		private ExplosiveProjectileAttackParameterConfig mParameters;

		public override void ConfigureParameters(ProjectileAttackParameterConfig parameters)
		{
			mParameters = parameters as ExplosiveProjectileAttackParameterConfig;
		}

		protected override void ResolveHit(CombatEntity target)
		{
			if (mParameters == null) return;
			this.SendCommand(new SpawnExplosiveProjectileImpactCommand(mParameters, transform.position, OwnerFaction, Damage));
		}

		public override void ResetState()
		{
			mParameters = null;
			base.ResetState();
		}
	}
}
