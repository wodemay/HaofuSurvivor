using UnityEngine;
namespace HaoFuSurvivor 
{ 
	[CreateAssetMenu(menuName="ProjectSurvivor/Enemy Config")] 
	public class EnemyConfig:ScriptableObject 
	{ 
		public int Id; public GameObject Prefab; public float MoveSpeed=2f;
	}
}
