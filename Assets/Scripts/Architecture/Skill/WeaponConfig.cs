using System.Collections.Generic;
using UnityEngine;

namespace HaoFuSurvivor
{
	[CreateAssetMenu(menuName = "ProjectSurvivor/Skill/Weapon Config")]
	public class WeaponConfig : ScriptableObject
	{
		public int Id;
		public string DisplayName;
		[TextArea] public string Description;
		public Sprite Icon;
		public List<int> InitialAttackIds = new();
		public int MaxLevel = 5;
		public bool CanUpgrade = true;
		public bool CanAcquireDuringRun;
		public List<WeaponLevelUpgrade> LevelUpgrades = new();
	}

	[System.Serializable]
	public class WeaponLevelUpgrade
	{
		public int Level;
		[TextArea] public string Description;
		public List<int> AddAttackIds = new();
		public List<int> RemoveAttackIds = new();
		public List<WeaponAttackModifier> AttackModifiers = new();
	}

	[System.Serializable]
	public class WeaponAttackModifier
	{
		public int AttackId;
		public string Key;
		public float Value;
	}

	public static class WeaponUpgradeModifierKeys
	{
		public const string AttackCooldownMultiplier = "Attack.CooldownMultiplier";
		public const string ProjectileDamageAdd = "Projectile.DamageAdd";
		public const string ProjectileCountAdd = "Projectile.CountAdd";
		public const string ProjectileSpeedMultiplier = "Projectile.SpeedMultiplier";
		public const string ProjectilePierceAdd = "Projectile.PierceAdd";
	}
}
