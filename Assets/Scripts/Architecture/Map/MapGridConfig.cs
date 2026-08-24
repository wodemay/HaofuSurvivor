using UnityEngine;
using UnityEngine.Tilemaps;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Map/Grid Map Config")]
	public class MapGridConfig : ScriptableObject
	{
		public GameObject ChunkPrefab;
		public TileBase GroundTile;
		public MapThemeConfig Theme;
		[Min(1)] public int GeneratorVersion = 1;
		[Min(1)] public int ChunkSize = 32;
		[Min(0)] public int LoadRadius = 2;
		[Min(1)] public int UnloadRadius = 3;
		[Min(0)] public int InitialLoadRadius = 1;
		[Min(1)] public int MaxChunkOperationsPerTick = 4;

		public int ThemeId => Theme == null ? 0 : Theme.Id;

		public TileBase GetGroundTile()
		{
			return Theme != null && Theme.GroundTile != null ? Theme.GroundTile : GroundTile;
		}
	}
}
