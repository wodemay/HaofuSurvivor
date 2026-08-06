using System.Collections.Generic;
using QFramework;

namespace HaoFuSurvivor
{
	public readonly struct WeaponRuntimeState
	{
		public readonly int RuntimeId;
		public readonly int WeaponId;
		public readonly int Level;
		public readonly bool CanUpgrade;
		public readonly IReadOnlyList<int> AttackIds;

		public WeaponRuntimeState(WeaponRuntimeData runtime)
		{
			RuntimeId = runtime.RuntimeId;
			WeaponId = runtime.WeaponId;
			Level = runtime.Level;
			CanUpgrade = runtime.CanUpgrade;
			AttackIds = new List<int>(runtime.AttackIds);
		}
	}

	public class GetPlayerWeaponsQuery : AbstractQuery<IReadOnlyList<WeaponRuntimeState>>
	{
		protected override IReadOnlyList<WeaponRuntimeState> OnDo()
		{
			var result = new List<WeaponRuntimeState>();
			foreach (var runtime in this.GetModel<PlayerLoadoutModel>().Weapons) result.Add(new WeaponRuntimeState(runtime));
			return result;
		}
	}
}
