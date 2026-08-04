using UnityEngine;

namespace HaoFuSurvivor
{
	public enum EnemyAttackType
	{
		Contact,
		Projectile,
		Special
	}

	[CreateAssetMenu(menuName = "ProjectSurvivor/Enemy Attack Config")]
	public class EnemyAttackConfig : ScriptableObject
	{
		public int Id;
		public EnemyAttackType AttackType;
		public float Damage = 10f;
		public float Cooldown = 1f;
	}
}
