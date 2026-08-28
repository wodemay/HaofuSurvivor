using QFramework;

namespace HaoFuSurvivor
{
	public class GameArchitecture : Architecture<GameArchitecture>
	{
		protected override void Init()
		{
			RegisterUtility(new GameStoragePath());
			RegisterModel(new RunModel());
			RegisterModel(new RunTimerModel());
			RegisterModel(new RunSettlementModel());
			RegisterModel(new PlayerModel());
			RegisterModel(new EnemyModel());
			RegisterModel(new PlayerStatModel());
			RegisterModel(new InputModel());
			RegisterModel(new CharacterSelectionModel());
			RegisterModel(new PlayerLoadoutModel());
			RegisterModel(new ExperienceModel());
			RegisterModel(new LevelUpModel());
			RegisterModel(new DodgeModel());
			RegisterModel(new PlayerStatUpgradeModel());
			RegisterModel(new CharacterExclusivePerkModel());
			RegisterModel(new MapModel());
			RegisterModel(new WorldMapModel());
			RegisterModel(new ProfileModel());
			RegisterModel(new RunEconomyModel());

			RegisterUtility(new CharacterCatalog());
			RegisterUtility(new CharacterSelectionStorage());
			RegisterUtility(new RunTimelineCatalog());
			RegisterUtility(new EnemyCatalog());
			RegisterUtility(new AttackCatalog());
			RegisterUtility(new AttackExecutorRegistry());
			RegisterUtility(new SkillGroupCatalog());
			RegisterUtility(new SkillCatalog());
			RegisterUtility(new WeaponCatalog());
			RegisterUtility(new WeaponCombinationCatalog());
			RegisterUtility(new ExperienceProgressionCatalog());
			RegisterUtility(new RunSaveStorage());
			RegisterUtility(new SaveFileStorage());
			RegisterUtility(new DodgeCatalog());
			RegisterUtility(new StatUpgradeCatalog());
			RegisterUtility(new CharacterExclusiveSkillUpgradeCatalog());
			RegisterUtility(new CharacterExclusivePerkCatalog());
			RegisterUtility(new MapGridCatalog());
			RegisterUtility(new BreakableObjectCatalog());
			RegisterUtility(new MapEventCatalog());
			RegisterUtility(new ProfileStorage());
			RegisterUtility(new CoinEconomyCatalog());
			RegisterUtility(new DropTableCatalog());

			RegisterSystem(new GameLogSystem());
			RegisterSystem(new RunSystem());
			RegisterSystem(new GameLoopSystem());
			RegisterSystem(new RunTimerSystem());
			RegisterSystem(new RunSettlementSystem());
			RegisterSystem(new InputSystem());
			RegisterSystem(new PlayerSystem());
			RegisterSystem(new PlayerSpawnSystem());
			RegisterSystem(new EnemySystem());
			RegisterSystem(new EnemyHealthSystem());
			RegisterSystem(new AttackSystem());
			RegisterSystem(new ProjectileSystem());
			RegisterSystem(new ExplosiveAreaSystem());
			RegisterSystem(new BarrageProjectileSystem());
			RegisterSystem(new CombatTargetSystem());
			RegisterSystem(new PlayerLoadoutSystem());
			RegisterSystem(new WeaponCombinationSystem());
			RegisterSystem(new StatSystem());
			RegisterSystem(new DamageSystem());
			RegisterSystem(new CharacterSelectionSystem());
			RegisterSystem(new ExperienceSystem());
			RegisterSystem(new LevelUpSystem());
			RegisterSystem(new RunSaveSystem());
			RegisterSystem(new DodgeSystem());
			RegisterSystem(new PlayerStatUpgradeSystem());
			RegisterSystem(new CharacterExclusivePerkSystem());
			RegisterSystem(new PlayerRegenerationSystem());
			RegisterSystem(new CharacterExclusiveSkillUpgradeSystem());
			RegisterSystem(new MapSystem());
			RegisterSystem(new BreakableObjectSystem());
			RegisterSystem(new MapEventSystem());
			RegisterSystem(new MapNavMeshSystem());
			RegisterSystem(new WorldGuideSystem());
			RegisterSystem(new UIPopPanelSystem());
			RegisterSystem(new ProfileSystem());
			RegisterSystem(new RunEconomySystem());
			RegisterSystem(new PickupSystem());
		}
	}
}
