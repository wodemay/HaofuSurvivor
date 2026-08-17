using QFramework;

namespace HaoFuSurvivor
{
	public class GameArchitecture : Architecture<GameArchitecture>
	{
		protected override void Init()
		{
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

			RegisterUtility(new CharacterCatalog());
			RegisterUtility(new CharacterSelectionStorage());
			RegisterUtility(new RunTimelineCatalog());
			RegisterUtility(new EnemyCatalog());
			RegisterUtility(new AttackCatalog());
			RegisterUtility(new AttackExecutorRegistry());
			RegisterUtility(new SkillGroupCatalog());
			RegisterUtility(new SkillCatalog());
			RegisterUtility(new WeaponCatalog());
			RegisterUtility(new WeaponEvolutionCatalog());
			RegisterUtility(new ExperienceProgressionCatalog());
			RegisterUtility(new RunSaveStorage());
			RegisterUtility(new DodgeCatalog());

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
			RegisterSystem(new BarrageProjectileSystem());
			RegisterSystem(new CombatTargetSystem());
			RegisterSystem(new PlayerLoadoutSystem());
			RegisterSystem(new StatSystem());
			RegisterSystem(new DamageSystem());
			RegisterSystem(new CharacterSelectionSystem());
			RegisterSystem(new ExperienceSystem());
			RegisterSystem(new LevelUpSystem());
			RegisterSystem(new RunSaveSystem());
			RegisterSystem(new DodgeSystem());
		}
	}
}
