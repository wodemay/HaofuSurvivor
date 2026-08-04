using QFramework;
using UnityEngine;
namespace HaoFuSurvivor 
{ 
	public class EnemyCatalog:IUtility 
	{ 
		public EnemyCatalogConfig Config{get;} 
		public EnemyCatalog()
		{
			Config=Resources.Load<EnemyCatalogConfig>("Configs/Enemies/EnemyCatalog");
		} 
		public EnemyConfig Get(int id)=>Config.Enemies.Find(x=>x!=null&&x.Id==id); 
	} 
}
