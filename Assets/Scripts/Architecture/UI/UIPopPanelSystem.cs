using System;
using System.Collections.Generic;
using QFramework;

namespace HaoFuSurvivor
{
	public sealed class UIPopPanelRequest
	{
		public string Title { get; }
		public string Content { get; }
		public string OkText { get; }
		public string CancelText { get; }
		public Action OnOk { get; }
		public Action OnCancel { get; }

		public UIPopPanelRequest(string title, string content, string okText, string cancelText,
			Action onOk = null, Action onCancel = null)
		{
			Title = title ?? string.Empty;
			Content = content ?? string.Empty;
			OkText = okText ?? string.Empty;
			CancelText = cancelText ?? string.Empty;
			OnOk = onOk;
			OnCancel = onCancel;
		}

		public bool HasButton => !string.IsNullOrEmpty(OkText) || !string.IsNullOrEmpty(CancelText);
	}

	public class UIPopPanelSystem : AbstractSystem
	{
		private readonly Queue<UIPopPanelRequest> mPendingRequests = new();
		private UIPopPanelRequest mCurrentRequest;
		private bool mSelectionMade;

		public UIPopPanelRequest CurrentRequest => mCurrentRequest;

		public void Enqueue(UIPopPanelRequest request)
		{
			if (request == null || !request.HasButton) return;
			mPendingRequests.Enqueue(request);
			TryOpenNext();
		}

		public void CompleteCurrent(bool confirmed)
		{
			if (mCurrentRequest == null || mSelectionMade) return;
			mSelectionMade = true;
			var callback = confirmed ? mCurrentRequest.OnOk : mCurrentRequest.OnCancel;
			try
			{
				callback?.Invoke();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public void AcknowledgeClosed()
		{
			if (mCurrentRequest == null) return;
			mCurrentRequest = null;
			mSelectionMade = false;
			TryOpenNext();
		}

		private void TryOpenNext()
		{
			if (mCurrentRequest != null || mPendingRequests.Count == 0 || !CanShowNow()) return;
			mCurrentRequest = mPendingRequests.Dequeue();
			this.SendEvent(new UIPopPanelRequestedEvent());
		}

		private bool CanShowNow()
		{
			var phase = this.GetModel<RunModel>().Phase;
			return phase != RunPhase.Active && phase != RunPhase.Paused && phase != RunPhase.LevelUpSelection;
		}

		protected override void OnInit()
		{
			this.RegisterEvent<RunEndedEvent>(_ => TryOpenNext());
			this.RegisterEvent<RunExitedEvent>(_ => TryOpenNext());
		}
	}
}
