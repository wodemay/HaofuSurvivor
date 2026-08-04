using QFramework;

namespace HaoFuSurvivor
{
	public class GameArchitecture : Architecture<GameArchitecture>
	{
		protected override void Init()
		{
			RegisterModel(new RunModel());
			RegisterModel(new RunTimerModel());
			RegisterModel(new PlayerModel());
			RegisterModel(new EnemyModel());
			RegisterModel(new PlayerStatModel());
			RegisterModel(new InputModel());
			RegisterModel(new CharacterSelectionModel());

			RegisterUtility(new CharacterCatalog());
			RegisterUtility(new CharacterSelectionStorage());
			RegisterUtility(new RunTimelineCatalog());
			RegisterUtility(new EnemyCatalog());
			RegisterUtility(new AttackCatalog());
			RegisterUtility(new AttackExecutorRegistry());

			RegisterSystem(new RunSystem());
			RegisterSystem(new RunTimerSystem());
			RegisterSystem(new InputSystem());
			RegisterSystem(new PlayerSystem());
			RegisterSystem(new PlayerSpawnSystem());
			RegisterSystem(new EnemySystem());
			RegisterSystem(new AttackSystem());
			RegisterSystem(new StatSystem());
			RegisterSystem(new DamageSystem());
			RegisterSystem(new CharacterSelectionSystem());
		}
	}
}
