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
			var coins = this.GetSystem<RunEconomySystem>().CalculateSettlementCoins(result, Mathf.FloorToInt(this.GetModel<RunTimerModel>().ElapsedSeconds));
			var economy = this.GetModel<RunEconomyModel>();
			settlementModel.LastSettlement = new RunSettlementData(
				player.CharacterId,
				result,
				Mathf.FloorToInt(this.GetModel<RunTimerModel>().ElapsedSeconds),
				experience.Level,
				experience.CurrentExperience,
				coins,
				0,
				economy.NormalKillCount,
				economy.BossKillCount);
			settlementModel.HasSettlement = true;
			this.GetSystem<ProfileSystem>().AddCoins(coins);
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

	public readonly struct RunSettlementState
	{
		public readonly bool HasSettlement;
		public readonly RunSettlementData Data;

		public RunSettlementState(bool hasSettlement, RunSettlementData data)
		{
			HasSettlement = hasSettlement;
			Data = data;
		}
	}

	public class GetRunSettlementStateQuery : AbstractQuery<RunSettlementState>
	{
		protected override RunSettlementState OnDo()
		{
			var model = this.GetModel<RunSettlementModel>();
			return new RunSettlementState(model.HasSettlement, model.LastSettlement);
		}
	}
}



