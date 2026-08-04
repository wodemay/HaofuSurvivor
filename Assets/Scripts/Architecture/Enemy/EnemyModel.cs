using QFramework;
namespace HaoFuSurvivor
{
	public class EnemyModel : AbstractModel
	{
		public int AliveCount { get; internal set; }
		protected override void OnInit() => AliveCount = 0;
	}
}
