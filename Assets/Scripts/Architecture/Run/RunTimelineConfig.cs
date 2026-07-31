using System;
using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[Serializable]
	public class RunTimelineStage
	{
		public int TimeSeconds;
		public int EventId;
		public float EnemyHealthMultiplier = 1f;
		public float EnemyDamageMultiplier = 1f;
		public float EnemyMoveSpeedMultiplier = 1f;
		public float SpawnRateMultiplier = 1f;
	}

	[CreateAssetMenu(menuName = "ProjectSurvivor/Run Timeline Config")]
	public class RunTimelineConfig : ScriptableObject
	{
		public List<RunTimelineStage> Stages = new List<RunTimelineStage>();
	}

	public class RunTimelineCatalog : QFramework.IUtility
	{
		public RunTimelineConfig Config { get; }

		public RunTimelineCatalog()
		{
			Config = Resources.Load<RunTimelineConfig>("Configs/RunTimeline");
			if (Config == null) Debug.LogError("RunTimelineConfig was not found at Resources/Configs/RunTimeline.");
		}
	}
}
