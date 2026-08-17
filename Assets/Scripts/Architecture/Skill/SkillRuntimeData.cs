using System.Collections.Generic;

namespace HaoFuSurvivor
{
	public class SkillRuntimeData
	{
		private readonly List<int> mAttackIds = new();

		public int RuntimeId { get; }
		public int SkillId { get; }
		public int Level { get; internal set; } = 1;
		public bool CanUpgrade { get; }
		public IReadOnlyList<int> AttackIds => mAttackIds;

		public SkillRuntimeData(int runtimeId, int skillId, bool canUpgrade, IEnumerable<int> attackIds)
		{
			RuntimeId = runtimeId;
			SkillId = skillId;
			CanUpgrade = canUpgrade;
			if (attackIds == null) return;
			foreach (var attackId in attackIds)
				if (attackId != 0 && !mAttackIds.Contains(attackId)) mAttackIds.Add(attackId);
		}
	}
}
