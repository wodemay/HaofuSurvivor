using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Experience Drop Config")]
	public class ExperienceDropConfig : ScriptableObject
	{
		public int Id;
		public string DisplayName;
		public float BaseExperience = 1f;
		public GameObject Prefab;
	}
}
