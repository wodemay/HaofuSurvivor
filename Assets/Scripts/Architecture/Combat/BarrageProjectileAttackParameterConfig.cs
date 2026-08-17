using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Combat/Barrage Projectile Attack Parameter Config")]
	public class BarrageProjectileAttackParameterConfig : ProjectileAttackParameterConfig
	{
		public int BurstCount = 3;
		public int ProjectilesPerBurst = 8;
		public float BurstInterval = .15f;
	}
}
