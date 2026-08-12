using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class RunSettlementSystem : QFramework.AbstractSystem
	{
		public void Settle(RunPhase result)
		{
			var settlementModel = this.GetModel<RunSettlementModel>();
			if (settlementModel.HasSettlement) return;

			var player = this.GetModel<PlayerModel>();
			var experience = this.GetModel<ExperienceModel>();
			settlementModel.LastSettlement = new RunSettlementData(
				player.CharacterId,
				result,
				Mathf.FloorToInt(this.GetModel<RunTimerModel>().ElapsedSeconds),
				experience.Level,
				experience.CurrentExperience,
				0,
				0);
			settlementModel.HasSettlement = true;
			this.SendEvent(new RunSettledEvent(settlementModel.LastSettlement));
		}

		public void Reset()
		{
			this.GetModel<RunSettlementModel>().Reset();
		}

		protected override void OnInit()
		{
		}
	}
}



