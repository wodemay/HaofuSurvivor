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
		public float UltimateDuration = 5f;
		public float UltimateProjectileInterval = .0125f;
		public float UltimateOrbitRadius = 2f;
		public float UltimateOrbitDegreesPerSecond = 1080f;
		public float UltimateDamageMultiplier = 2f;
		public float UltimateSpeedMultiplier = 1.5f;
		public int UltimatePierce = 2;
	}
}
