using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Combat/Barrage Projectile Attack Parameter Config")]
	public class BarrageProjectileAttackParameterConfig : ProjectileAttackParameterConfig
	{
		public float Duration = 3f;
		public float ProjectileInterval = .1f;
		public float OrbitRadius = 1.25f;
		public float EmissionRadius = .35f;
		public float OrbitDegreesPerSecond = 180f;
	}
}
