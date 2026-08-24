using UnityEngine;
using UnityEngine.Tilemaps;

namespace HaoFuSurvivor
{
	public static class MapColliderUtility
	{
		public static bool IsMoveBlocker(Collider2D collider) => IsNamedTilemap(collider, "MoveBlockerTilemap");

		public static bool IsProjectileBlocker(Collider2D collider) => IsNamedTilemap(collider, "ProjectileBlockerTilemap");

		private static bool IsNamedTilemap(Collider2D collider, string name)
		{
			var tilemap = collider == null ? null : collider.GetComponentInParent<TilemapCollider2D>();
			return tilemap != null && tilemap.name == name;
		}
	}
}
