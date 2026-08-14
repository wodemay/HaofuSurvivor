using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class DodgeCatalog : IUtility
	{
		private DodgeCatalogConfig mConfig;

		public DodgeCatalog()
		{
			mConfig = Resources.Load<DodgeCatalogConfig>("Configs/Dodge/DodgeCatalog");
			if (mConfig == null) Debug.LogError("DodgeCatalog could not load Configs/Dodge/DodgeCatalog.");
		}

		public DodgeConfig Get(int id)
		{
			if (mConfig != null && mConfig.Dodges != null)
			{
				var configured = mConfig.Dodges.Find(item => item != null && item.Id == id);
				if (configured != null) return configured;
			}

			foreach (var config in Resources.LoadAll<DodgeConfig>("Configs/Dodge"))
				if (config != null && config.Id == id) return config;

			Debug.LogError($"Dodge config {id} was not found in Configs/Dodge.");
			return null;
		}
	}
}
