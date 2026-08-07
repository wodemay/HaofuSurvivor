using UnityEngine;
using QFramework;

namespace HaoFuSurvivor
{
	public partial class GameStart : ViewController, IController
	{
		public IArchitecture GetArchitecture() => GameArchitecture.Interface;

		private void Awake()
		{
			ResKit.Init();
			GameArchitecture.InitArchitecture();
			this.RegisterEvent<RunStartedEvent>(_ => OpenGameHud())
				.UnRegisterWhenGameObjectDestroyed(gameObject);
		}

		private void Update()
		{
			GameArchitecture.Interface.SendCommand(new TickRunTimerCommand(Time.unscaledDeltaTime));
			GameArchitecture.Interface.SendCommand(new TickAttacksCommand());
			GameArchitecture.Interface.SendCommand(new TickPlayerDamageInvulnerabilityCommand());
		}

		private void FixedUpdate()
		{
			GameArchitecture.Interface.SendCommand(new TickRunPhysicsCommand(Time.fixedUnscaledDeltaTime));
			GameArchitecture.Interface.SendCommand(new TickEnemiesCommand());
		}

		private void OpenGameHud()
		{
			UIKit.OpenPanel<UIGameHUDPanel>(
				assetBundleName: "uigamehudpanel_prefab",
				prefabName: UIGameHUDPanel.Name);
		}

		private void Start()
		{
			UIKit.OpenPanel<UICharacterSelectPanel>(
				assetBundleName: "uicharacterselectpanel_prefab",
				prefabName: UICharacterSelectPanel.Name);
		}

	}

	public class CameraFollow : MonoBehaviour
	{
		private Transform mTarget;
		private float mZPosition;

		public void Bind(Transform target)
		{
			mTarget = target;
			mZPosition = transform.position.z;
		}

		private void LateUpdate()
		{
			if (mTarget == null) return;

			var position = mTarget.position;
			position.z = mZPosition;
			transform.position = position;
		}
	}
}
