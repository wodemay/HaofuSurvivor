using QFramework;

namespace HaoFuSurvivor
{
	public class DodgeModel : AbstractModel
	{
		public DodgeRuntimeData Runtime { get; internal set; }

		public void Reset()
		{
			Runtime = null;
		}

		protected override void OnInit()
		{
			Reset();
		}
	}
}
