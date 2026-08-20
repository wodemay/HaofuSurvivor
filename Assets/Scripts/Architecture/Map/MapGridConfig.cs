using UnityEngine;
using UnityEngine.Tilemaps;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Map/Grid Map Config")]
	public class MapGridConfig : ScriptableObject
	{
		public GameObject ChunkPrefab;
		public TileBase GroundTile;
		[Min(1)] public int ChunkSize = 32;
		[Min(0)] public int LoadRadius = 2;
		[Min(1)] public int UnloadRadius = 3;
		[Min(1)] public int MaxChunkOperationsPerTick = 4;
	}
}
