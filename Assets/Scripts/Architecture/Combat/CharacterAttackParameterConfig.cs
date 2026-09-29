using System;
using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Combat/Character Attack Parameters")]
	public class CharacterAttackParameterConfig : ProjectileAttackParameterConfig
	{
		public float Radius = 2.2f;
		public float Angle = 120f;
		public float Windup = 0.1f;
		public float Duration = 0.35f;
		public float Interval = 0.2f;
		public int Shots = 6;
		public int Pierce;
		public float Knockback;
		public float ReductionDuration;
		public float Reduction = 0.5f;
		public float FinalDamage;
		public float OrbitRadius = 1.2f;
		public GameObject WeaponVisual;
		public GameObject EffectVisual;
		public AudioClip StartSound;
		public AudioClip ShotSound;
		public AudioClip EndSound;
		public List<CharacterAttackLevel> Levels = new();
		public CharacterAttackLevel AtLevel(int level) => Levels.Find(item => item.Level == level);
	}

	[Serializable]
	public class CharacterAttackLevel
	{
		public int Level;
		public float Damage;
		public float Radius;
		public float Cooldown;
		public float ReductionDuration;
		public float FinalDamage;
		public int Shots;
		public float Duration;
	}
}
