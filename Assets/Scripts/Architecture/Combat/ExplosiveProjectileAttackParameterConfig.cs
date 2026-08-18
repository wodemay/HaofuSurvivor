using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Combat/Explosive Projectile Attack Parameter Config")]
	public class ExplosiveProjectileAttackParameterConfig : ProjectileAttackParameterConfig
	{
		public GameObject ExplosionPrefab;
		public float ExplosionVisualDuration = 0.25f;
		public float ExplosionRadius = 2.5f;
		public GameObject GroundFlamePrefab;
		public float GroundFlameRadius = 1.9f;
		public float GroundFlameDuration = 3f;
		public float GroundFlameTickInterval = 0.5f;
		public float GroundFlameDamageMultiplier = 0.222222f;
	}
}
