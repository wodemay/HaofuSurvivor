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
			this.RegisterEvent<UIPopPanelRequestedEvent>(_ => OpenPopPanel())
				.UnRegisterWhenGameObjectDestroyed(gameObject);
			this.RegisterEvent<ProfileLoadCompletedEvent>(OnProfileLoadCompleted)
				.UnRegisterWhenGameObjectDestroyed(gameObject);
			GameArchitecture.Interface.GetSystem<ProfileSystem>().Initialize();
		}

		private void Update()
		{
			GameArchitecture.Interface.GetSystem<ProfileSystem>().FlushPendingSave();
			GameArchitecture.Interface.SendCommand(new TickGameLoopCommand(Time.deltaTime));
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

		private void OpenPopPanel()
		{
			var panel = UIKit.OpenPanel<UIPopPanel>(
				assetBundleName: "uipoppanel_prefab",
				prefabName: UIPopPanel.Name);
			if (panel == null)
			{
				GameArchitecture.Interface.GetSystem<UIPopPanelSystem>().AcknowledgeClosed();
			}
		}

		private void OnProfileLoadCompleted(ProfileLoadCompletedEvent profileEvent)
		{
			if (!profileEvent.RequiresNotice || string.IsNullOrEmpty(profileEvent.Message)) return;
			GameArchitecture.Interface.SendCommand(new RequestUIPopPanelCommand(
				new UIPopPanelRequest("存档提示", profileEvent.Message, "确认", string.Empty)));
		}

		private void OnRunEnded(RunEndedEvent runEndedEvent)
		{
			UIKit.ClosePanel<UIGameHUDPanel>();
			UIKit.ClosePanel<UILevelUpPanel>();
			UIKit.OpenPanel<UIRunSettlementPanel>(
				assetBundleName: "uirunsettlementpanel_prefab",
				prefabName: UIRunSettlementPanel.Name);
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
