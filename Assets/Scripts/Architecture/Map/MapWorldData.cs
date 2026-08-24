using System;
using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[Flags]
	public enum MapCellFlags
	{
		None = 0,
		Walkable = 1 << 0,
		BlocksMovement = 1 << 1,
		BlocksProjectile = 1 << 2,
		Reserved = 1 << 3
	}

	[Serializable]
	public class MapObstaclePlacementData
	{
		public int TemplateId;
		public int WorldCellX;
		public int WorldCellY;
		public int QuarterTurns;
		public bool IsMirrored;
		public string StableId;
	}

	[Serializable]
	public class MapStaticEntityAnchorData
	{
		public string StableId;
		public int WorldCellX;
		public int WorldCellY;
	}

	[Serializable]
	public class MapChunkData
	{
		public Vector2Int Coordinate;
		public int ChunkSize;
		public List<MapCellFlags> CellFlags = new();
		public List<MapObstaclePlacementData> Obstacles = new();
		public List<MapStaticEntityAnchorData> StaticEntityAnchors = new();
	}
}
