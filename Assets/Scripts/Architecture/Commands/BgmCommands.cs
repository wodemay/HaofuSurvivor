using UnityEngine;
using QFramework;

namespace HaoFuSurvivor
{
	public class PlayMainMenuBgmCommand : AbstractCommand
	{
		protected override void OnExecute()
		{
			this.GetSystem<BgmSystem>().PlayFromStart(this.GetSystem<BgmSystem>().CurrentClip);
		}
	}

	public class ConfigureMainMenuBgmCommand : AbstractCommand
	{
		private readonly AudioClip mClip;

		public ConfigureMainMenuBgmCommand(AudioClip clip)
		{
			mClip = clip;
		}

		protected override void OnExecute()
		{
			this.GetSystem<BgmSystem>().SetClip(mClip);
		}
	}
}
