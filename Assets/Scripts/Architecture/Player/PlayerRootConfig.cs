using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Player Root Config")]
	public class PlayerRootConfig : ScriptableObject
	{
		public GameObject PlayerPrefab;
		public GameObject HealthBarPrefab;
	}
}
