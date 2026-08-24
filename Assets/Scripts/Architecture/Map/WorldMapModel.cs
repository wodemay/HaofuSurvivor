namespace HaoFuSurvivor
{
	public class WorldMapModel : QFramework.AbstractModel
	{
		public int WorldSeed { get; private set; }
		public int ThemeId { get; private set; }
		public int GeneratorVersion { get; private set; }
		public bool HasWorld { get; private set; }

		internal void BeginNew(int themeId, int generatorVersion)
		{
			Restore(System.Guid.NewGuid().GetHashCode(), themeId, generatorVersion);
		}

		internal void Restore(int worldSeed, int themeId, int generatorVersion)
		{
			WorldSeed = worldSeed;
			ThemeId = themeId;
			GeneratorVersion = generatorVersion;
			HasWorld = true;
		}

		internal void Clear()
		{
			WorldSeed = 0;
			ThemeId = 0;
			GeneratorVersion = 0;
			HasWorld = false;
		}

		protected override void OnInit()
		{
			Clear();
		}
	}
}
