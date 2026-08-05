using System.Collections.Generic;
using QFramework;

namespace HaoFuSurvivor
{
	public class PlayerLoadoutModel : AbstractModel
	{
		public readonly List<int> WeaponIds = new();
		public readonly List<int> SkillIds = new();
		public int DodgeId { get; internal set; }

		public void Reset()
		{
			WeaponIds.Clear();
			SkillIds.Clear();
			DodgeId = 0;
		}

		protected override void OnInit()
		{
			Reset();
		}
	}
}
