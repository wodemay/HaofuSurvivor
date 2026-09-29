using UnityEngine;
using QFramework;

namespace HaoFuSurvivor
{
	[DefaultExecutionOrder(-100)]
	public class CameraFollow : MonoBehaviour, IController
	{
		public bool EnableCombatShake = true;
		private Transform mTarget;
		private float mZPosition;
		private float mShakeRemaining;
		public IArchitecture GetArchitecture() => GameArchitecture.Interface;
		private void Awake()
		{
			this.RegisterEvent<CombatFinisherEvent>(_ => { if (EnableCombatShake) mShakeRemaining = 0.15f; })
				.UnRegisterWhenGameObjectDestroyed(gameObject);
			this.RegisterEvent<RunEndedEvent>(_ => mShakeRemaining = 0).UnRegisterWhenGameObjectDestroyed(gameObject);
			this.RegisterEvent<RunExitedEvent>(_ => mShakeRemaining = 0).UnRegisterWhenGameObjectDestroyed(gameObject);
		}

		public void Bind(Transform target)
		{
			mTarget = target;
			mZPosition = transform.position.z;
			mShakeRemaining = 0;
		}

		private void LateUpdate()
		{
			if (mTarget == null) return;

			var position = mTarget.position;
			position.z = mZPosition;
			if (EnableCombatShake && mShakeRemaining > 0)
			{
				if (this.SendQuery(new GetRunTimeStateQuery()).IsRunning) mShakeRemaining = Mathf.Max(0, mShakeRemaining - Time.deltaTime);
				var phase = mShakeRemaining * 120;
				position += new Vector3(Mathf.Sin(phase), Mathf.Cos(phase * 1.3f), 0) * (0.05f * mShakeRemaining / 0.15f);
			}
			transform.position = position;
		}
	}
}
