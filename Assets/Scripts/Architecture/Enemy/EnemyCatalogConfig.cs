using System.Collections.Generic;
using UnityEngine;
namespace HaoFuSurvivor 
{ 
	[CreateAssetMenu(menuName="ProjectSurvivor/Enemy Catalog")] 
	public class EnemyCatalogConfig:ScriptableObject 
	{
		public List<EnemyConfig> Enemies=new(); 
		public float BaseSpawnInterval=1.25f; 
		public float MinSpawnInterval=.2f; 
		public float SpawnRampSeconds=300f;
		public float SecondsPerExtraEnemy=120f;
		public int MaxEnemiesPerWave=6; 
		public int MaxAliveEnemies=200; 
		public float ViewportPadding=.12f;
		public float BossSpawnTimeSeconds=1800f;
		public List<int> BossIds=new();
	} 
}
