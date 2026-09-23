using QFramework;
using UnityEngine;

namespace HaoFuSurvivor
{
	[RequireComponent(typeof(SpriteRenderer))]
	public sealed class ActorFrameAnimationView : MonoBehaviour, IController, IRunUpdateable
	{
		[SerializeField] private Sprite[] IdleFrames;
		[SerializeField] private Sprite[] MoveFrames;
		[SerializeField, Min(1f)] private float IdleFrameRate = 4f;
		[SerializeField, Min(1f)] private float MoveFrameRate = 8f;
		private SpriteRenderer mRenderer;
		private Vector3 mLastPosition;
		private float mFrameTime;
		private float mMovingRemaining;
		private bool mMoving;
		private IUnRegister mRunStarted;

		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		private void OnEnable()
		{
			mRenderer = GetComponent<SpriteRenderer>();
			ResetPlayback();
			this.SendCommand(new RegisterActorAnimationCommand(this, true));
			mRunStarted = this.RegisterEvent<RunStartedEvent>(_ =>
			{
				ResetPlayback();
				this.SendCommand(new RegisterActorAnimationCommand(this, true));
			});
		}

		private void OnDisable()
		{
			mRunStarted?.UnRegister();
			mRunStarted = null;
			this.SendCommand(new RegisterActorAnimationCommand(this, false));
		}

		private void ResetPlayback()
		{
			mLastPosition = transform.position;
			mMoving = false;
			mMovingRemaining = 0f;
			mFrameTime = 0f;
			if (IdleFrames != null && IdleFrames.Length > 0) mRenderer.sprite = IdleFrames[0];
		}

		public void OnRunUpdate(float deltaTime)
		{
			if (!isActiveAndEnabled || deltaTime <= 0f) return;
			var position = transform.position;
			if ((position - mLastPosition).sqrMagnitude > 0.000001f) mMovingRemaining = 0.1f;
			else mMovingRemaining = Mathf.Max(0f, mMovingRemaining - deltaTime);
			mLastPosition = position;
			var moving = mMovingRemaining > 0f;
			if (moving != mMoving)
			{
				mMoving = moving;
				mFrameTime = 0f;
			}
			var frames = mMoving ? MoveFrames : IdleFrames;
			if (frames == null || frames.Length == 0) return;
			var rate = Mathf.Max(1f, mMoving ? MoveFrameRate : IdleFrameRate);
			mFrameTime = (mFrameTime + deltaTime) % (frames.Length / rate);
			mRenderer.sprite = frames[Mathf.Min(frames.Length - 1, Mathf.FloorToInt(mFrameTime * rate))];
		}
	}

	public sealed class RegisterActorAnimationCommand : AbstractCommand
	{
		private readonly ActorFrameAnimationView mView;
		private readonly bool mRegister;

		public RegisterActorAnimationCommand(ActorFrameAnimationView view, bool register)
		{
			mView = view;
			mRegister = register;
		}

		protected override void OnExecute()
		{
			var loop = this.GetSystem<GameLoopSystem>();
			if (mRegister) loop.RegisterUpdateable(mView);
			else loop.UnregisterUpdateable(mView);
		}
	}
}
