# 项目脚本说明

本文以当前 `Assets/Scripts/` 为准，说明每个脚本的职责与边界。业务链路必须遵循 QFramework：Controller/View → Command → System → Model/Utility；读取使用 Query，跨模块通知使用 Event。

## Architecture/Character

- `CharacterConfig.cs`：角色静态数据，含数值、内容预制体与 `SkillGroupId`。
- `CharacterCatalog.cs`：加载角色配置，按排序和 ID 查询。
- `CharacterSelectionModel.cs`：保存当前选中的角色 ID。
- `CharacterSelectionStorage.cs`：用 PlayerPrefs 持久化角色选择。
- `CharacterSelectionSystem.cs`：校验、切换和确认选择，并发布选择事件。

## Architecture/Skill

- `SkillGroupConfig.cs`：定义初始武器、技能和闪避 ID。
- `SkillGroupCatalogConfig.cs` / `SkillGroupCatalog.cs`：技能组目录与按 ID 查询。
- `WeaponConfig.cs`：武器静态容器模板，定义初始 Attack、等级上限、可升级状态和每级 Attack 增删项。
- `WeaponCatalogConfig.cs` / `WeaponCatalog.cs`：武器目录与按 ID 查询。
- `WeaponEvolutionConfig.cs`：定义来源武器、进化所需等级和目标武器。
- `WeaponEvolutionCatalogConfig.cs` / `WeaponEvolutionCatalog.cs`：进化目录与运行时查询。
- `WeaponRuntimeData.cs`：单把角色武器的运行时容器，保存稳定槽位 ID、等级、可升级状态和当前 Attack 内容。
- `GetPlayerWeaponsQuery.cs`：只读返回当前 WeaponRuntime 快照。
- `PlayerLoadoutModel.cs`：保存本局 WeaponRuntime、技能、闪避和当前拥有者。
- `PlayerLoadoutSystem.cs`：装备、升级、替换、进化和重置角色武器容器，并管理对应 Attack Trigger。

## Architecture/Combat

- `CombatFaction.cs`：玩家与敌人的阵营标识。
- `AttackConfig.cs`：通用攻击静态数据；`ExecutorId` 决定行为，含伤害、冷却和可选参数资产。
- `AttackCatalogConfig.cs` / `AttackCatalog.cs`：攻击目录与按 ID 查询。
- `ProjectileAttackParameterConfig.cs`：子弹预制体、速度、射程、寿命和对象池容量。
- `AttackExecutorRegistry.cs`：执行器注册表；含 `IAttackExecutor`、`CollisionAttackExecutor` 与 `ProjectileAttackExecutor`。
- `AttackSystem.cs`：保存攻击运行时、冷却和同阵营拦截，调用对应执行器。
- `CombatTargetSystem.cs`：维护可攻击实体；`FindClosestCombatTargetQuery` 查询最近敌对目标。

## Architecture/Enemy

- `EnemyConfig.cs`：敌人静态数据，包含内容预制体、基础生命、移速和攻击 ID。
- `EnemyCatalogConfig.cs`：敌人列表及生成间隔、波次、上限等全局参数。
- `EnemyConfigCatalog.cs`：声明 `EnemyCatalog`，加载目录并按 ID 查询敌人。
- `EnemyRootConfig.cs`：绑定通用 `EnemyRoot` 预制体。
- `EnemyModel.cs`：保存当前存活敌人数。
- `EnemyFactory.cs`：创建、配置及按敌人 ID 池化 `EnemyRoot`；挂载内容、战斗实体、生命和攻击桥接。
- `EnemySystem.cs`：按时间轴阶段生成敌人、追踪玩家、维护活动列表并回收死亡敌人。
- `EnemyHealthSystem.cs`：保存活动敌人的运行时生命；初始化时应用时间轴生命倍率，处理受伤、死亡与回收。

## Architecture/Player 与 Input

- `InputModel.cs` / `InputSystem.cs`：保存并写入归一化移动输入。
- `PlayerModel.cs`：保存玩家注册、位置、角色 ID、生命、死亡和无敌帧状态。
- `PlayerStatModel.cs`：保存本局最大生命、移速、攻击力和无敌帧时长。
- `PlayerRootConfig.cs`：绑定 `PlayerRoot` 与血条画布预制体。
- `PlayerSpawnSystem.cs`：创建 PlayerRoot，挂载选中角色、血条、CombatEntity，并绑定摄像机和初始技能组。
- `PlayerSystem.cs`：处理注册、移动、受伤、死亡及无敌帧推进。
- `StatSystem.cs`：提供玩家最终属性读取接口。
- `DamageSystem.cs`：根据 CombatEntity 阵营，将通用伤害路由给玩家或敌人生命系统。

## Architecture/Run

- `RunModel.cs`：保存局内阶段 `RunPhase`。
- `RunSystem.cs`：开始、胜利结束或失败结束一局，并重置计时和敌人。
- `RunTimelineConfig.cs`：定义时间点事件、阶段敌人 ID 与敌人/生成倍率。
- `RunTimerModel.cs`：保存累计时间、当前阶段和当前倍率。
- `RunTimerSystem.cs`：推进正计时、应用已到达阶段并发布事件。
- `GetRunTimerStateQuery.cs`：只读返回当前计时与倍率快照。

## Architecture/Commands、Events 与根架构

- `GameplayCommands.cs`：集中声明开局、计时、刷怪、攻击、伤害、目标注册、敌人生命、玩家移动和角色选择命令。
- `GameplayEvents.cs`：声明局内、时间轴、角色选择、玩家受伤/死亡、敌人受伤/死亡事件。
- `GameArchitecture.cs`：唯一的 QFramework 注册点，注册所有 Model、System 和 Utility。

## Game

- `GameStart.cs`：初始化 ResKit 与 GameArchitecture，打开角色选择/HUD，并逐帧驱动计时、敌人、攻击和无敌帧命令。
- `GameStart.Designer.cs`：QFramework 自动生成的 Bind 文件，禁止手改。
- `CameraFollow`：定义在 `GameStart.cs`，于 LateUpdate 跟随已生成玩家。
- `CombatEntity.cs`：Unity 侧战斗身份桥接，注册/注销可被攻击目标并保存阵营。
- `CollisionAttackTrigger.cs`：碰撞攻击桥接，检测敌对实体并请求执行攻击。
- `ProjectileAttackTrigger.cs`：远程攻击桥接，查询最近敌对实体并请求执行攻击。
- `ProjectileController.cs`：子弹飞行、碰撞命中、超时和状态重置。
- `ProjectileFactory.cs`：在运行时 `ProjectileContainer` 下创建、获取和回收子弹对象池。
- `PlayerController.cs`：桌面移动输入与 Rigidbody2D 桥接，发送移动命令。
- `PlayerHealthBarView.cs`：监听玩家受伤事件并刷新血条 Slider。

## UI

- `UICharacterSelectPanel.cs`：角色选择面板逻辑，创建条目、记录选择并确认开局。
- `UICharacterSelectPanel.Designer.cs`：自动生成 Bind 文件，禁止手改。
- `UICharacterSelectItem.cs`：单个角色条目，通过既定子节点路径显示配置与处理点击。
- `UIGameHUDPanel.cs`：局内 HUD，显示正计时并处理返回操作。
- `UIGameHUDPanel.Designer.cs`：自动生成 Bind 文件，禁止手改。

## Editor

- `ConfigurationCreatorWindow.cs`：菜单 `ProjectSurvivor/Configuration Creator` 对应的编辑器窗口；创建角色或敌人的配置和内容预制体模板，分配下一个数字 ID，并自动把敌人加入 EnemyCatalog。

## 维护规则

- 新攻击添加新的 `IAttackExecutor` 与 `ExecutorId`，禁止恢复 AttackType 枚举或集中分支。
- 静态数值只放 ScriptableObject；局内升级和生命等可变状态只放 Model/System。
- UI 层级、Prefab 结构和 Bind 由用户维护；脚本只能接入既有对象。
- 新增或删除脚本后，同时更新本文和 `Assets/AGENTS.md`。
