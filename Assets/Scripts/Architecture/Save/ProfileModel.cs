using QFramework;

namespace HaoFuSurvivor
{
	public class ProfileModel : AbstractModel
	{
		public BigCoin ProfileCoin { get; internal set; } = BigCoin.Zero;
		public bool IsLoaded { get; internal set; }

		protected override void OnInit()
		{
			ProfileCoin = BigCoin.Zero;
			IsLoaded = false;
		}
	}
}
