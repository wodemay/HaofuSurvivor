# 脚本索引

本文按单一职责定位脚本；完整运行顺序见 `../GameLogic.zh-CN.md`，资源接入步骤见 `../Guides/`。

## 架构与运行

- `Architecture/GameArchitecture.cs`：注册全部 Model、System、Utility。
- `Architecture/Commands/GameplayCommands.cs`：状态变更 Command；`TickGameLoopCommand`、`TickGamePhysicsCommand` 是 Unity 帧到架构的两个入口。
- `Architecture/Events/GameplayEvents.cs`：跨模块事件。
- `Run/RunModel.cs`、`RunSystem.cs`：对局阶段、开始、暂停、结束、重开、返回与继续。
- `Run/RunTimerModel.cs`、`RunTimerSystem.cs`：统一逻辑时间和时间线阶段。
- `Run/GameLoopSystem.cs`：`IRunUpdateable` / `IRunFixedUpdateable` 接口及按需 Tick 注册表。
- `Run/RunSettlementModel.cs`、`RunSettlementSystem.cs`：本局结算快照。
- `Save/RunSaveData.cs`、`RunSaveStorage.cs`、`RunSaveSystem.cs`：角色、Weapon、Skill、Dodge 快照 DTO，PlayerPrefs 读写、自动保存与恢复。

## 角色、玩家与敌人

- `Character/CharacterConfig.cs`、`CharacterCatalog.cs`：角色静态配置和目录。
- `Character/CharacterSelectionModel.cs`、`CharacterSelectionSystem.cs`、`CharacterSelectionStorage.cs`：选角状态与持久化。
- `Player/PlayerRootConfig.cs`、`PlayerSpawnSystem.cs`：PlayerRoot 和角色内容生成。
- `Player/PlayerModel.cs`、`PlayerStatModel.cs`、`PlayerSystem.cs`、`StatSystem.cs`：玩家状态、属性、帧无敌和物理移动。
- `Player/DamageSystem.cs`：通用伤害路由。
- `Enemy/EnemyConfig.cs`、`EnemyCatalogConfig.cs`、`EnemyConfigCatalog.cs`：敌人静态数据与目录。
- `Enemy/EnemyRootConfig.cs`、`EnemyFactory.cs`：EnemyRoot 创建和对象池。
- `Enemy/EnemyModel.cs`、`EnemySystem.cs`、`EnemyHealthSystem.cs`：敌人生成、追击、生命和回收。

## 战斗、成长与闪避

- `Combat/AttackConfig.cs`、`AttackCatalog.cs`、`AttackCatalogConfig.cs`：Attack 配置目录。
- `Combat/AttackExecutorRegistry.cs`：Executor 注册；含碰撞、定向投射物、爆炸投射物与手动无目标环形弹幕 Executor。
- `Combat/AttackSystem.cs`：AttackRuntime、冷却、自动目标选择和执行。
- `Combat/ProjectileSystem.cs`：活跃投射物的帧/物理批量推进。
- `Combat/BarrageProjectileAttackParameterConfig.cs`、`BarrageProjectileSystem.cs`：环绕发射圆的随机投射物参数与持续调度。
- `Combat/CombatTargetSystem.cs`、`CombatFaction.cs`：最近目标与范围目标查询、阵营。
- `Combat/ExplosiveProjectileAttackParameterConfig.cs`、`ExplosiveAreaSystem.cs`：爆炸半径、地面领域参数，以及统一时钟驱动的爆炸/灼烧结算。
- `Skill/SkillGroupConfig.cs`、`SkillGroupCatalog.cs`：开局技能组。
- `Skill/SkillConfig.cs`、`SkillCatalogConfig.cs`、`SkillCatalog.cs`、`SkillRuntimeData.cs`：技能静态配置、目录和独立运行时容器。
- `Skill/WeaponConfig.cs`、`WeaponCatalog.cs`、`WeaponRuntimeData.cs`、`PlayerLoadoutModel.cs`、`PlayerLoadoutSystem.cs`：Weapon 容器及其装配、升级、替换、组合和来源 Weapon 退役管理，以及 Skill 装配。
- `Skill/WeaponCombinationConfig.cs`、`WeaponCombinationCatalogConfig.cs`、`WeaponCombinationCatalog.cs`、`WeaponCombinationSystem.cs`：最终形态配方、目录、资格判定与单/双 Weapon 组合执行。
- `Dodge/DodgeConfig.cs`、`DodgeCatalogConfig.cs`、`DodgeCatalog.cs`、`DodgeModel.cs`、`DodgeRuntimeData.cs`、`DodgeSystem.cs`：闪避配置与运行时。
- `Experience/ExperienceDropConfig.cs`、`ExperienceProgressionConfig.cs`、`ExperienceFactory.cs`、`ExperienceDropController.cs`、`ExperienceModel.cs`、`ExperienceSystem.cs`：经验掉落、吸附和等级事件。
- `LevelUp/LevelUpModel.cs`、`LevelUpSystem.cs`：升级选择队列与阶段控制。
- `Progression/CharacterExclusiveSkillUpgradeConfig.cs`、`CharacterExclusiveSkillUpgradeCatalog.cs`、`CharacterExclusiveSkillUpgradeSystem.cs`：角色初始 Weapon/Dodge 满级前置、专属 Skill 首位候选和一次性升级执行。

## Unity 桥接、UI 与编辑器

- `Game/GameStart.cs`：初始化架构、发送两个根 Tick Command、打开面板和应用生命周期存档。
- `Game/CameraFollow.cs`：纯表现层 LateUpdate 跟随。
- Game/Presentation/WorldRootLocator.cs：解析 MainScene 的 WorldRoot 九个直接子节点，供运行时工厂挂载对象。- `Game/PlayerController.cs`：保留给既有角色内容 Prefab 的空标记。
- `Game/CombatEntity.cs`：Unity 对象的 Combat 身份组件。
- `Game/CollisionAttackTrigger.cs`：碰撞 Attack 的感知与 Command 桥接。
- `Game/ProjectileController.cs`、`ProjectileFactory.cs`、`ExplosiveProjectileController.cs`：投射物实例状态、按参数隔离的对象池和命中爆炸桥接。
- `Game/AreaEffectFactory.cs`：爆炸与地面火焰表现对象池。
- `UI/*.cs`：面板行为；`*.Designer.cs` 是 QFramework 生成 Bind，禁止手改。
- `Editor/ConfigurationCreatorWindow.cs`：创建 CharacterConfig、EnemyConfig 和内容模板。
