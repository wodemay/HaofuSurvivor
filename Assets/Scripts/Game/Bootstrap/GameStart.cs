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
			this.RegisterEvent<LevelUpSelectionRequestedEvent>(_ => OpenLevelUpPanel())
				.UnRegisterWhenGameObjectDestroyed(gameObject);
			this.RegisterEvent<RunEndedEvent>(OnRunEnded)
				.UnRegisterWhenGameObjectDestroyed(gameObject);
		}

		private void Update()
		{
			GameArchitecture.Interface.SendCommand(new TickGameLoopCommand(Time.unscaledDeltaTime));
		}

		private void FixedUpdate()
		{
			GameArchitecture.Interface.SendCommand(new TickGamePhysicsCommand(Time.fixedDeltaTime));
		}

		private void OpenGameHud()
		{
			UIKit.OpenPanel<UIGameHUDPanel>(
				assetBundleName: "uigamehudpanel_prefab",
				prefabName: UIGameHUDPanel.Name);
		}

		private void OpenLevelUpPanel()
		{
			UIKit.OpenPanel<UILevelUpPanel>(
				assetBundleName: "uileveluppanel_prefab",
				prefabName: UILevelUpPanel.Name);
		}

		private void OnRunEnded(RunEndedEvent runEndedEvent)
		{
			if (runEndedEvent.Phase != RunPhase.Defeat) return;
			UIKit.ClosePanel<UIGameHUDPanel>();
			UIKit.ClosePanel<UILevelUpPanel>();
			UIKit.OpenPanel<UIGameOverPanel>(
				assetBundleName: "uigameoverpanel_prefab",
				prefabName: UIGameOverPanel.Name);
		}

		private void Start()
		{
			UIKit.OpenPanel<UIMainMenuPanel>(
				assetBundleName: "uimainmenupanel_prefab",
				prefabName: UIMainMenuPanel.Name);
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus) GameArchitecture.Interface.GetSystem<RunSaveSystem>().SaveCurrentRun();
		}

		private void OnApplicationQuit()
		{
			GameArchitecture.Interface.GetSystem<RunSaveSystem>().SaveCurrentRun();
		}

	}
}
