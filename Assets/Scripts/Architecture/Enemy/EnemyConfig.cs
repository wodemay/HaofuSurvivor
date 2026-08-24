using System.Collections.Generic;
using UnityEngine;
namespace HaoFuSurvivor 
{ 
	[CreateAssetMenu(menuName="ProjectSurvivor/Enemy Config")] 
	public class EnemyConfig:ScriptableObject 
	{ 
		public int Id;
		public GameObject Prefab;
		public float BaseHealth = 20f;
		public float MoveSpeed = 2f;
		public List<int> AttackIds = new();
		public ExperienceDropConfig ExperienceDrop;
		public int DropTableId;
		public bool IsPersistent;
	}
}
