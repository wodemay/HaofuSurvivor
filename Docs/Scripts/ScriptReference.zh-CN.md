# 脚本索引

本文按单一职责定位脚本；完整运行顺序见 `../GameLogic.zh-CN.md`，资源接入步骤见 `../Guides/`。

## 架构、运行与存档

- `Architecture/GameArchitecture.cs`：注册全部 Model、System、Utility。
- `Architecture/Commands/GameplayCommands.cs`、`Architecture/Events/GameplayEvents.cs`：跨模块命令与事件。
- `Architecture/Run/RunSystem.cs`：开始、暂停、结束、重开、返回和继续对局。
- `Architecture/Run/RunTimerSystem.cs`：唯一逻辑时间源、时间线阶段和单帧 0.05 秒上限。
- `Architecture/Run/GameLoopSystem.cs`：维护 `IRunUpdateable` / `IRunFixedUpdateable` 注册表并分发 Tick。
- `Architecture/Run/RunSettlementModel.cs`、`RunSettlementSystem.cs`：本局结算快照。
- `Architecture/Save/GameStoragePath.cs`：解析游戏根目录和相对存储路径。
- `Architecture/Save/RunSaveData.cs`、`RunSaveStorage.cs`、`RunSaveSystem.cs`：将快照写入 `SaveData/active-run.json`，按逻辑时间保存并恢复对局，不使用 PlayerPrefs。
- `Architecture/Character/CharacterSelectionStorage.cs`：将角色选择写入 `SaveData/selected-character.json`。
- `Architecture/Save/GameLogSystem.cs`：将 Unity 日志追加写入 `Logs/game.log`，文件不可用时只降级为 Unity Warning。

## 角色、玩家与敌人

- `Architecture/Character/CharacterConfig.cs`、`CharacterCatalog.cs`：角色静态配置和目录。
- `Architecture/Character/CharacterSelectionModel.cs`、`CharacterSelectionSystem.cs`、`CharacterSelectionStorage.cs`：选角状态与持久化。
- `Architecture/Player/PlayerRootConfig.cs`、`PlayerSpawnSystem.cs`：创建 PlayerRoot、角色内容、碰撞体和血条。
- `Architecture/Player/PlayerModel.cs`、`PlayerStatModel.cs`、`PlayerSystem.cs`、`StatSystem.cs`：玩家状态、属性、无敌帧、移动和地图碰撞滑动。
- `Architecture/Player/DamageSystem.cs`：通用伤害路由。
- `Architecture/Enemy/EnemyConfig.cs`、`EnemyCatalogConfig.cs`、`EnemyConfigCatalog.cs`：敌人静态数据与目录。
- `Architecture/Enemy/EnemyRootConfig.cs`、`EnemyFactory.cs`：EnemyRoot 创建、场景容器挂载和按 ID 对象池。
- `Architecture/Enemy/EnemyModel.cs`、`EnemySystem.cs`、`EnemyHealthSystem.cs`：敌人生成、追击、NavMesh 路径、生命和回收。

## 战斗、技能、成长与闪避

- `Architecture/Combat/AttackConfig.cs`、`AttackCatalog.cs`、`AttackCatalogConfig.cs`：Attack 配置目录。
- `Architecture/Combat/AttackExecutorRegistry.cs`、`AttackSystem.cs`：Executor 注册、AttackRuntime、冷却、自动目标和执行。
- `Architecture/Combat/CombatTargetSystem.cs`、`CombatFaction.cs`：阵营、最近目标和范围目标查询。
- `Architecture/Combat/ProjectileSystem.cs`、`Game/Combat/ProjectileController.cs`、`ProjectileFactory.cs`：投射物批量推进、参数隔离对象池和命中回收。
- `Architecture/Combat/BarrageProjectileAttackParameterConfig.cs`、`BarrageProjectileSystem.cs`：幸存者环形弹幕的旋转、随机发射和持续时间。
- `Architecture/Combat/ExplosiveProjectileAttackParameterConfig.cs`、`ExplosiveAreaSystem.cs`、`Game/Combat/ExplosiveProjectileController.cs`、`AreaEffectFactory.cs`：爆炸范围、地面火焰和灼烧 Tick。
- `Architecture/Skill/SkillGroupConfig.cs`、`SkillGroupCatalog.cs`：开局技能组。
- `Architecture/Skill/SkillConfig.cs`、`SkillCatalogConfig.cs`、`SkillCatalog.cs`、`SkillRuntimeData.cs`：Skill 静态配置、目录和运行时容器。
- `Architecture/Skill/WeaponConfig.cs`、`WeaponCatalog.cs`、`WeaponRuntimeData.cs`、`PlayerLoadoutModel.cs`、`PlayerLoadoutSystem.cs`：Weapon 容器、装配、升级、Attack 替换、退役和组合。
- `Architecture/Skill/WeaponCombinationConfig.cs`、`WeaponCombinationCatalogConfig.cs`、`WeaponCombinationCatalog.cs`、`WeaponCombinationSystem.cs`：最终形态配方和组合执行。
- `Architecture/Dodge/DodgeConfig.cs`、`DodgeCatalogConfig.cs`、`DodgeCatalog.cs`、`DodgeModel.cs`、`DodgeRuntimeData.cs`、`DodgeSystem.cs`：Shift 闪避、冷却、位移、无敌帧和升级。
- `Architecture/Experience/ExperienceDropConfig.cs`、`ExperienceFactory.cs`、`ExperienceDropController.cs`、`ExperienceModel.cs`、`ExperienceSystem.cs`：经验掉落、对象池、加速度吸附和等级事件。
- `Architecture/LevelUp/LevelUpModel.cs`、`LevelUpSystem.cs`：三候选槽、选择确认、失败重生成和升级阶段。
- `Architecture/Progression/PlayerStatUpgradeModel.cs`、`PlayerStatUpgradeSystem.cs`、`StatUpgradeCatalogConfig.cs`：通用属性等级与运行时倍率。
- `Architecture/Progression/PlayerRegenerationSystem.cs`：按统一逻辑时间的一秒自然回血 Tick。
- `Architecture/Progression/CharacterExclusivePerkConfig.cs`、`CharacterExclusivePerkCatalog.cs`、`CharacterExclusivePerkSystem.cs`：角色专属升级和闪避/技能触发的临时联动。
- `Architecture/Progression/CharacterExclusiveSkillUpgradeConfig.cs`、`CharacterExclusiveSkillUpgradeCatalog.cs`、`CharacterExclusiveSkillUpgradeSystem.cs`：专属 Weapon/Dodge 前置与一次性 Skill 升级。

## 地图与导航

- `Architecture/Map/MapGridConfig.cs`、`MapGridCatalog.cs`：区块尺寸、加载半径、主题、初始加载和每帧操作上限。
- `Architecture/Map/MapThemeConfig.cs`、`MapObstacleTemplateConfig.cs`：底图、装饰和可旋转/镜像的障碍模板。
- `Architecture/Map/MapWorldData.cs`、`WorldMapModel.cs`、`MapModel.cs`：世界 Seed、主题/生成版本、区块数据和运行时加载状态。
- `Architecture/Map/MapSystem.cs`：32×32 区块确定性生成、障碍不重叠、Flood Fill 防封闭、3×3 优先加载和远端卸载。
- `Architecture/Map/MapNavMeshSystem.cs`：活动区块的 2D NavMesh 异步重建与按需路径查询。
- `Game/Map/MapChunkFactory.cs`、`MapChunkView.cs`、`MapColliderUtility.cs`：区块表现对象池、Tilemap 填充和碰撞配置。

## Unity 桥接、表现与编辑器

- `Game/Bootstrap/GameStart.cs`：初始化架构、发送两个根 Tick Command、打开面板和应用生命周期存档。
- `Game/Camera/CameraFollow.cs`：表现层 `LateUpdate` 跟随。
- `Game/Presentation/WorldRootLocator.cs`：解析 WorldRoot 九个直接子节点，供工厂挂载对象。
- `ProjectSettings/TagManager.asset`、`Game/Presentation/WorldRootLocator.cs`：维护九层 Sorting Layer 名称，并按 `WorldRootSlot` 将运行时对象挂到对应层级。
- `Game/Combat/CombatEntity.cs`、`CollisionAttackTrigger.cs`：Combat 身份和碰撞 Attack 桥接。
- `Game/Player/PlayerController.cs`、`PlayerHealthBarView.cs`：角色内容兼容标记和血条视图。
- `UI/*.cs`：面板行为；`*.Designer.cs` 是 QFramework 生成 Bind，禁止手改。
- `Editor/ConfigurationCreatorWindow.cs`、`Editor/ProjectBuild.cs`：创建配置资产和 Unity 构建入口。
