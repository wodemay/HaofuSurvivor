using UnityEngine;

namespace HaoFuSurvivor
{
	public class MapModel : QFramework.AbstractModel
	{
		public Vector2Int CurrentChunk { get; internal set; }
		public bool HasCurrentChunk { get; internal set; }
		public int LoadedChunkCount { get; internal set; }

		protected override void OnInit()
		{
			CurrentChunk = Vector2Int.zero;
			HasCurrentChunk = false;
			LoadedChunkCount = 0;
		}
	}
}
