using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Skill/Weapon Evolution Config")]
	public class WeaponEvolutionConfig : ScriptableObject
	{
		public int SourceWeaponId;
		public int RequiredLevel = 1;
		public int TargetWeaponId;
	}

}
