using UnityEngine;

namespace HaoFuSurvivor
{
	public enum WorldRootSlot
	{
		MapBackground,
		MapDecoration,
		GroundEffect,
		Pickup,
		Enemy,
		Player,
		Projectile,
		CombatEffect,
		WorldUI
	}

	public static class WorldRootLocator
	{
		private const string WorldRootName = "WorldRoot";

		public static Transform Get(WorldRootSlot slot)
		{
			var worldRoot = GameObject.Find(WorldRootName);
			if (worldRoot == null)
			{
				Debug.LogError("MainScene requires WorldRoot.");
				return null;
			}

			var root = worldRoot.transform.Find(slot + "Root");
			if (root == null) Debug.LogError("WorldRoot requires " + slot + "Root.");
			return root;
		}
	}
}