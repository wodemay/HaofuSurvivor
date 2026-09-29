using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public readonly struct CombatFinisherEvent { }
	[Serializable]
	public class CharacterActionState
	{
		public int AttackId;
		public int Level;
		public float Age;
		public int Step;
		public float Damage;
		public float Radius;
		public float Angle;
		public float DirectionX;
		public float DirectionY;
		public int Swing = 1;
		[NonSerialized] public CombatEntity Target;
		[NonSerialized] public long TargetSpawnId;
		[NonSerialized] public readonly HashSet<long> HitTargets = new();
		[NonSerialized] public readonly List<GameObject> Visuals = new();
	}

	public class CharacterCombatModel : AbstractModel
	{
		public readonly List<CharacterActionState> Actions = new();
		public readonly HashSet<long> DodgeHits = new();
		public float ReductionRemaining;
		public float Reduction;
		public float EmpowerRemaining;
		public float EmpowerMultiplier = 2f;
		public float TrailElapsed;
		public int TrailCount;
		public int Swing = 1;
		protected override void OnInit() { }
	}
}
