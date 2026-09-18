using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	public class RunSettlementSystem : QFramework.AbstractSystem
	{
		public bool Settle(RunPhase result)
		{
			var settlementModel = this.GetModel<RunSettlementModel>();
			if (settlementModel.IsCommitted) return true;
			if (result != RunPhase.Defeat && result != RunPhase.Victory) return false;

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
			if (!this.GetSystem<ProfileSystem>().TryCommitRunSettlement(this.GetModel<RunModel>().RunId, coins))
			{
				this.SendEvent(new RunSaveFailedEvent("结算金币未能保存，已保留局内存档。请检查磁盘后点击结算页确认重试。"));
				return false;
			}
			settlementModel.IsCommitted = true;
			this.GetSystem<RunSaveSystem>().Clear();
			this.SendEvent(new RunSettledEvent(settlementModel.LastSettlement));
			return true;
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
		public readonly bool IsCommitted;
		public readonly RunSettlementData Data;

		public RunSettlementState(bool hasSettlement, RunSettlementData data, bool isCommitted)
		{
			HasSettlement = hasSettlement;
			IsCommitted = isCommitted;
			Data = data;
		}
	}

	public class GetRunSettlementStateQuery : AbstractQuery<RunSettlementState>
	{
		protected override RunSettlementState OnDo()
		{
			var model = this.GetModel<RunSettlementModel>();
			return new RunSettlementState(model.HasSettlement, model.LastSettlement, model.IsCommitted);
		}
	}

	public class RetryRunSettlementCommand : AbstractCommand
	{
		protected override void OnExecute() => this.GetSystem<RunSettlementSystem>().Settle(this.GetModel<RunModel>().Phase);
	}
}



