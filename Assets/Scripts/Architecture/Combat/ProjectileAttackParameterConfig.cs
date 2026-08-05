using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Combat/Projectile Attack Parameter Config")]
	public class ProjectileAttackParameterConfig : ScriptableObject
	{
		public GameObject ProjectilePrefab;
		public float MoveSpeed = 8f;
		public float Lifetime = 3f;
		public float AttackRange = 8f;
		public int PoolCapacity = 16;
	}
}
