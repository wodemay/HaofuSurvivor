# 脚本索引

本文只说明脚本归属和单一职责；具体运行流程见 `Docs/GameLogic.zh-CN.md`，新增内容步骤见 `Docs/Guides/`。

## 架构注册与公共接口

- `Architecture/GameArchitecture.cs`：注册全部 Model、System、Utility。
- `Architecture/Commands/GameplayCommands.cs`：游戏状态变更命令。
- `Architecture/Events/GameplayEvents.cs`：跨模块事件定义。

## Run 与 Save

- `Architecture/Run/RunModel.cs`：保存 `RunPhase`。
- `Architecture/Run/RunSystem.cs`：开始、暂停、恢复、结束、退出和继续对局。
- `Architecture/Run/RunTimerModel.cs`：保存统一时间输出。
- `Architecture/Run/RunTimerSystem.cs`：推进逻辑时间和时间阶段。
- `Architecture/Run/GetRunTimeStateQuery.cs`、`GetRunTimerStateQuery.cs`：只读时间快照。
- `Architecture/Run/RunSettlementModel.cs`、`RunSettlementSystem.cs`：生成本局结算快照。
- `Architecture/Save/RunSaveData.cs`：存档 DTO。
- `Architecture/Save/RunSaveStorage.cs`：PlayerPrefs JSON 读写。
- `Architecture/Save/RunSaveSystem.cs`：自动保存、恢复和存档查询。

## Character 与 Player

- `Character/CharacterConfig.cs`、`CharacterCatalog.cs`：角色静态配置和目录。
- `Character/CharacterSelectionModel.cs`、`CharacterSelectionSystem.cs`、`CharacterSelectionStorage.cs`：角色选择与选择持久化。
- `Player/PlayerRootConfig.cs`、`PlayerSpawnSystem.cs`：PlayerRoot 和角色内容生成。
- `Player/PlayerModel.cs`、`PlayerStatModel.cs`、`PlayerSystem.cs`、`StatSystem.cs`：玩家状态和属性。
- `Player/DamageSystem.cs`：通用伤害路由。

## Enemy 与 Experience

- `Enemy/EnemyConfig.cs`、`EnemyCatalogConfig.cs`、`EnemyConfigCatalog.cs`：敌人静态数据和目录。
- `Enemy/EnemyRootConfig.cs`、`EnemyFactory.cs`：EnemyRoot 创建和对象池。
- `Enemy/EnemyModel.cs`、`EnemySystem.cs`、`EnemyHealthSystem.cs`：敌人生成、移动、生命和回收。
- `Experience/ExperienceDropConfig.cs`、`ExperienceProgressionConfig.cs`：经验静态配置。
- `Experience/ExperienceFactory.cs`、`ExperienceDropController.cs`：经验球池和 Unity 移动桥接。
- `Experience/ExperienceModel.cs`、`ExperienceSystem.cs`：经验、吸附和等级事件。

## Combat 与 Skill

- `Combat/AttackConfig.cs`、`AttackCatalog.cs`、`AttackCatalogConfig.cs`：Attack 配置目录。
- `Combat/AttackExecutorRegistry.cs`：Executor 注册和执行器查找。
- `Combat/AttackSystem.cs`、`CombatTargetSystem.cs`：攻击运行时和目标查询。
- `Combat/CombatFaction.cs`、`ProjectileAttackParameterConfig.cs`：阵营和投射物参数。
- `Skill/SkillGroupConfig.cs`、`SkillGroupCatalog.cs`：开局技能组。
- `Skill/WeaponConfig.cs`、`WeaponCatalog.cs`、`WeaponRuntimeData.cs`：Weapon 模板、目录和运行时容器。
- `Skill/PlayerLoadoutModel.cs`、`PlayerLoadoutSystem.cs`：按槽位装备 Weapon、Skill、Dodge，处理升级、替换、进化和可选能力降级。
- `Skill/WeaponEvolutionConfig.cs`、`WeaponEvolutionCatalog.cs`：Weapon 进化映射。
- `Dodge/DodgeConfig.cs`、`DodgeCatalogConfig.cs`、`DodgeCatalog.cs`：闪避静态配置与目录。
- `Dodge/DodgeModel.cs`、`DodgeRuntimeData.cs`、`DodgeSystem.cs`：闪避运行时状态、冲刺、冷却、无敌和升级。
- `LevelUp/LevelUpModel.cs`、`LevelUpSystem.cs`：升级选择队列和阶段控制。

## Unity 游戏桥接

- `Game/GameStart.cs`：初始化架构、驱动帧命令和打开面板；同文件内的 `CameraFollow` 跟随玩家。
- `Game/PlayerController.cs`：输入到玩家移动命令的桥接。
- `Game/CombatEntity.cs`：Unity 对象的 Combat 身份组件。
- `Game/CollisionAttackTrigger.cs`、`ProjectileAttackTrigger.cs`：攻击触发桥接。
- `Game/ProjectileController.cs`、`ProjectileFactory.cs`：投射物运行和对象池。

## UI 与 Editor

- `UI/UICharacterSelectPanel.cs`、`UICharacterSelectItem.cs`：角色选择。
- `UI/UIMainMenuPanel.cs`：开始、继续、设置和退出入口。
- `UI/UIGameHUDPanel.cs`：局内时间、暂停和返回。
- `UI/UILevelUpPanel.cs`、`UI/UILevelUpOptionItem.cs`：升级候选展示。
- `UI/UIGameOverPanel.cs`：失败后的重开和返回。
- `*.Designer.cs`：QFramework 自动生成 Bind，禁止手改。
- `Editor/ConfigurationCreatorWindow.cs`：创建 CharacterConfig、EnemyConfig 和内容模板。
