using System.Collections.Generic;
using QFramework;

namespace HaoFuSurvivor
{
	public interface IRunUpdateable
	{
		void OnRunUpdate(float deltaTime);
	}

	public interface IRunFixedUpdateable
	{
		void OnRunFixedUpdate(float deltaTime);
	}

	public class GameLoopSystem : AbstractSystem
	{
		private readonly List<IRunUpdateable> mUpdateables = new();
		private readonly HashSet<IRunUpdateable> mUpdateableSet = new();
		private readonly List<IRunFixedUpdateable> mFixedUpdateables = new();
		private readonly HashSet<IRunFixedUpdateable> mFixedUpdateableSet = new();
		private readonly List<IRunUpdateable> mPendingUpdateAdds = new();
		private readonly List<IRunUpdateable> mPendingUpdateRemoves = new();
		private readonly List<IRunFixedUpdateable> mPendingFixedAdds = new();
		private readonly List<IRunFixedUpdateable> mPendingFixedRemoves = new();
		private bool mIsTicking;
		private bool mClearPending;

		public void BeginRun()
		{
			RegisterUpdateable(this.GetSystem<InputSystem>());
			RegisterUpdateable(this.GetSystem<PlayerSystem>());
			RegisterUpdateable(this.GetSystem<RunSaveSystem>());
			if (this.GetSystem<DodgeSystem>().IsEquipped) RegisterFixedUpdateable(this.GetSystem<DodgeSystem>());
			RegisterFixedUpdateable(this.GetSystem<PlayerSystem>());
			RegisterFixedUpdateable(this.GetSystem<EnemySystem>());
			if (this.GetSystem<AttackSystem>().HasRuntimes) RegisterUpdateable(this.GetSystem<AttackSystem>());
		}

		public void EndRun()
		{
			if (mIsTicking)
			{
				mClearPending = true;
				return;
			}
			Clear();
		}

		public void TickFrame(float unscaledDeltaTime)
		{
			var timer = this.GetSystem<RunTimerSystem>();
			timer.Advance(unscaledDeltaTime);
			if (!timer.IsRunning()) return;
			TickUpdateables(this.GetModel<RunTimerModel>().DeltaTime);
		}

		public void TickFixed(float fixedDeltaTime)
		{
			var timer = this.GetSystem<RunTimerSystem>();
			timer.AdvanceFixed(fixedDeltaTime);
			if (!timer.IsRunning()) return;
			TickFixedUpdateables(this.GetModel<RunTimerModel>().FixedDeltaTime);
		}

		public void RegisterUpdateable(IRunUpdateable updateable)
		{
			if (updateable == null || !mUpdateableSet.Add(updateable)) return;
			if (mIsTicking) mPendingUpdateAdds.Add(updateable);
			else mUpdateables.Add(updateable);
		}

		public void UnregisterUpdateable(IRunUpdateable updateable)
		{
			if (updateable == null || !mUpdateableSet.Remove(updateable)) return;
			if (mIsTicking) mPendingUpdateRemoves.Add(updateable);
			else mUpdateables.Remove(updateable);
		}

		public void RegisterFixedUpdateable(IRunFixedUpdateable updateable)
		{
			if (updateable == null || !mFixedUpdateableSet.Add(updateable)) return;
			if (mIsTicking) mPendingFixedAdds.Add(updateable);
			else mFixedUpdateables.Add(updateable);
		}

		public void UnregisterFixedUpdateable(IRunFixedUpdateable updateable)
		{
			if (updateable == null || !mFixedUpdateableSet.Remove(updateable)) return;
			if (mIsTicking) mPendingFixedRemoves.Add(updateable);
			else mFixedUpdateables.Remove(updateable);
		}

		private void TickUpdateables(float deltaTime)
		{
			if (deltaTime <= 0f) return;
			mIsTicking = true;
			for (var index = 0; index < mUpdateables.Count; index++)
			{
				var updateable = mUpdateables[index];
				if (mUpdateableSet.Contains(updateable)) updateable.OnRunUpdate(deltaTime);
			}
			mIsTicking = false;
			ApplyPendingChanges();
		}

		private void TickFixedUpdateables(float deltaTime)
		{
			if (deltaTime <= 0f) return;
			mIsTicking = true;
			for (var index = 0; index < mFixedUpdateables.Count; index++)
			{
				var updateable = mFixedUpdateables[index];
				if (mFixedUpdateableSet.Contains(updateable)) updateable.OnRunFixedUpdate(deltaTime);
			}
			mIsTicking = false;
			ApplyPendingChanges();
		}

		private void ApplyPendingChanges()
		{
			if (mClearPending)
			{
				mClearPending = false;
				Clear();
				return;
			}
			ApplyPendingChanges(mUpdateables, mUpdateableSet, mPendingUpdateAdds, mPendingUpdateRemoves);
			ApplyPendingChanges(mFixedUpdateables, mFixedUpdateableSet, mPendingFixedAdds, mPendingFixedRemoves);
		}

		private static void ApplyPendingChanges<T>(List<T> items, HashSet<T> itemSet, List<T> adds, List<T> removes)
		{
			foreach (var updateable in removes) items.Remove(updateable);
			removes.Clear();
			foreach (var updateable in adds)
				if (itemSet.Contains(updateable)) items.Add(updateable);
			adds.Clear();
		}

		private void Clear()
		{
			mUpdateables.Clear();
			mUpdateableSet.Clear();
			mFixedUpdateables.Clear();
			mFixedUpdateableSet.Clear();
			mPendingUpdateAdds.Clear();
			mPendingUpdateRemoves.Clear();
			mPendingFixedAdds.Clear();
			mPendingFixedRemoves.Clear();
		}

		protected override void OnInit()
		{
		}
	}
}
