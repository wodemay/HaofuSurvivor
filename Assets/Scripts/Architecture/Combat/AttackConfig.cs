using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Combat/Attack Config")]
	public class AttackConfig : ScriptableObject
	{
		public int Id;
		public string ExecutorId;
		public CombatFaction TargetFaction;
		public float Damage = 10f;
		public float Cooldown = 1f;
	}
}
