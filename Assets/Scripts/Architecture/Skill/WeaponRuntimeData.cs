using System.Collections.Generic;

namespace HaoFuSurvivor
{
	public class WeaponRuntimeData
	{
		private readonly List<int> mAttackIds = new();

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
	}
}
