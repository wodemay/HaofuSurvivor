using UnityEngine;

namespace HaoFuSurvivor
{
	public class MapGridCatalog : QFramework.IUtility
	{
		private const string ConfigPath = "Configs/Map/MapGrid";

		public MapGridConfig Config { get; }

		public MapGridCatalog()
		{
			Config = Resources.Load<MapGridConfig>(ConfigPath);
		}
	}
}
