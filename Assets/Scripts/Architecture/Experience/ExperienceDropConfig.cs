using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Experience Drop Config")]
	public class ExperienceDropConfig : ScriptableObject
	{
		public int Id;
		public string DisplayName;
		public int BaseExperience = 1;
		public GameObject Prefab;
	}
}
