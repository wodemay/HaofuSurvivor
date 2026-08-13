using System;
using System.Collections.Generic;

namespace HaoFuSurvivor
{
	[Serializable]
	public class RunSaveData
	{
		public int CharacterId;
		public float ElapsedSeconds;
		public int CurrentStageIndex;
		public float CurrentHealth;
		public float PositionX;
		public float PositionY;
		public int Level;
		public int CurrentExperience;
		public int RequiredExperience;
		public int DodgeId;
		public int DodgeLevel;
		public List<WeaponSaveData> Weapons = new();
	}

	[Serializable]
	public class WeaponSaveData
	{
		public int RuntimeId;
		public int WeaponId;
		public int Level;
		public bool CanUpgrade;
		public List<int> AttackIds = new();
		public List<WeaponModifierSaveData> Modifiers = new();
	}

	[Serializable]
	public class WeaponModifierSaveData
	{
		public int AttackId;
		public string Key;
		public float Value;
	}
}
