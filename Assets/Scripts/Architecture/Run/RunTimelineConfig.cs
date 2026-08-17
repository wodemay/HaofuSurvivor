using System;
using System.Collections.Generic;
using UnityEngine;
namespace HaoFuSurvivor { [Serializable] public class RunTimelineStage { public int TimeSeconds; public int EventId; public List<int> EnemyIds=new(); public float EnemyHealthMultiplier=1f; public float EnemyDamageMultiplier=1f; public float EnemyMoveSpeedMultiplier=1f; public float SpawnRateMultiplier=1f; } [CreateAssetMenu(menuName="ProjectSurvivor/Run Timeline Config")] public class RunTimelineConfig:ScriptableObject { public List<RunTimelineStage> Stages=new(); } public class RunTimelineCatalog:QFramework.IUtility { public RunTimelineConfig Config{get;} public RunTimelineCatalog(){Config=Resources.Load<RunTimelineConfig>("Configs/Run/RunTimeline");} } }
