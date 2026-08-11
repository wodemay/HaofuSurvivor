using System.Collections.Generic;

namespace HaoFuSurvivor
{
	public class WeaponRuntimeData
	{
		private readonly List<int> mAttackIds = new();
		private readonly Dictionary<int, Dictionary<string, float>> mAttackModifiers = new();

		public int RuntimeId { get; }
		public int WeaponId { get; internal set; }
		public int Level { get; internal set; }
		public bool CanUpgrade { get; internal set; }
		public IReadOnlyList<int> AttackIds => mAttackIds;

		public WeaponRuntimeData(int runtimeId, int weaponId, bool canUpgrade, IEnumerable<int> attackIds)
		{
			RuntimeId = runtimeId;
			WeaponId = weaponId;
			Level = 1;
			CanUpgrade = canUpgrade;
			SetAttackIds(attackIds);
		}

		internal void SetAttackIds(IEnumerable<int> attackIds)
		{
			mAttackIds.Clear();
			if (attackIds == null) return;
			foreach (var attackId in attackIds)
			{
				if (attackId != 0 && !mAttackIds.Contains(attackId)) mAttackIds.Add(attackId);
			}
		}

		public float GetModifierValue(int attackId, string key, float defaultValue)
		{
			if (string.IsNullOrWhiteSpace(key) || !mAttackModifiers.TryGetValue(attackId, out var modifiers)) return defaultValue;
			return modifiers.TryGetValue(key, out var value) ? defaultValue + value : defaultValue;
		}

		internal void ApplyModifiers(IEnumerable<WeaponAttackModifier> modifiers)
		{
			if (modifiers == null) return;
			foreach (var modifier in modifiers)
			{
				if (modifier == null || modifier.AttackId == 0 || string.IsNullOrWhiteSpace(modifier.Key)) continue;
				if (!mAttackModifiers.TryGetValue(modifier.AttackId, out var values))
				{
					values = new Dictionary<string, float>();
					mAttackModifiers.Add(modifier.AttackId, values);
				}
				values[modifier.Key] = GetModifierValue(modifier.AttackId, modifier.Key, 0f) + modifier.Value;
			}
		}

		internal void ResetModifiers()
		{
			mAttackModifiers.Clear();
		}
	}
}
