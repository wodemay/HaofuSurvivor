using System;
using System.Collections.Generic;

namespace HaoFuSurvivor
{
	[Serializable]
	public class ProfileData
	{
		public int SaveVersion;
		public string ProfileCoin = "0";
		public List<MetaUpgradeSaveData> MetaUpgrades = new();
	}

	[Serializable]
	public class MetaUpgradeSaveData
	{
		public int UpgradeId;
		public int Level;
	}
}
