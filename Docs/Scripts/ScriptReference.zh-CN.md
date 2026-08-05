# 项目脚本说明

本文说明 `Assets/Scripts/` 中当前所有 C# 脚本的职责。业务代码遵循 QFramework：Controller/View → Command → System → Model/Utility，读取通过 Query，跨模块通知通过 Event。

## Architecture/Character

- `CharacterCatalog.cs`：加载并按排序提供角色配置。
- `CharacterConfig.cs`：角色静态数据，包含 ID、属性、技能 ID、图标与角色内容预制体。
- `CharacterSelectionModel.cs`：保存当前选中的角色 ID。
- `CharacterSelectionStorage.cs`：通过 PlayerPrefs 持久化角色选择。
- `CharacterSelectionSystem.cs`：校验、切换并确认角色选择，发布选择事件。

## Architecture/Combat

- `CombatFaction.cs`：定义玩家与敌人的阵营标识，用于攻击目标校验。
- `AttackConfig.cs`：通用攻击配置；`ExecutorId` 决定行为，不使用攻击类型枚举分支。
- `AttackCatalogConfig.cs`：攻击配置列表的 ScriptableObject 容器。
- `AttackCatalog.cs`：加载 `Resources/Configs/Combat/AttackCatalog` 并按 ID 查询攻击。
- `AttackExecutorRegistry.cs`：注册并查找攻击执行器；当前注册 `contact` 执行器。该执行器配置触碰触发器，并通过通用伤害入口结算。
- `AttackSystem.cs`：维护攻击运行时、冷却、目标阵营验证与执行器调用。

## Architecture/Commands 与 Events

- `GameplayCommands.cs`：集中声明开局、计时、敌人、攻击注册/释放、攻击尝试、玩家移动、受伤无敌帧与角色选择命令。
- `GameplayEvents.cs`：声明局开始/结束、计时更新、时间轴阶段、玩家受伤/死亡及角色选择事件。

## Architecture/Enemy

- `EnemyConfig.cs`：单个怪物静态数据，包含 ID、内容预制体、移速和攻击 ID 列表。
- `EnemyCatalogConfig.cs`：怪物列表与全局刷怪参数。
- `EnemyConfigCatalog.cs`：加载怪物目录并按 ID 查询怪物配置。
- `EnemyRootConfig.cs`：绑定通用 `EnemyRoot` 预制体。
- `EnemyFactory.cs`：按怪物 ID 管理 EnemyRoot 对象池，挂载角色内容、CombatEntity、刚体和攻击执行器。
- `EnemyModel.cs`：保存当前存活怪物数量。
- `EnemySystem.cs`：根据时间轴阶段刷怪、追踪玩家、维护存活列表和对象池回收。

## Architecture/Input

- `InputModel.cs`：保存归一化后的玩家移动输入。
- `InputSystem.cs`：写入移动输入状态。

## Architecture/Player

- `PlayerModel.cs`：保存玩家注册状态、位置、血量、死亡状态和受伤无敌帧剩余时间。
- `PlayerStatModel.cs`：保存生命、移速、攻击和无敌帧时长等运行时属性；无敌帧默认值为 0。
- `PlayerRootConfig.cs`：绑定 PlayerRoot 与血条画布预制体。
- `PlayerSpawnSystem.cs`：创建 PlayerRoot，注入 CombatEntity、Rigidbody2D、PlayerController，挂载角色内容和血条并绑定摄像机。
- `PlayerSystem.cs`：注册玩家、移动、受伤、死亡和无敌帧计时。
- `StatSystem.cs`：提供玩家最终属性读取接口。
- `DamageSystem.cs`：按目标阵营分发通用伤害；当前已实现对玩家的伤害结算。

## Architecture/Run

- `RunModel.cs`：保存局内阶段（未开始、进行、胜利、失败）。
- `RunSystem.cs`：开始或结束一局游戏，并重置计时和敌人。
- `RunTimelineConfig.cs`：定义时间点事件、怪物 ID 列表和敌人/刷怪倍率。
- `RunTimerModel.cs`：保存累计时间、当前阶段和当前倍率。
- `RunTimerSystem.cs`：推进正向计时，应用已到达时间轴阶段并发布事件。
- `GetRunTimerStateQuery.cs`：只读返回当前计时和倍率状态。

## Architecture 根节点

- `GameArchitecture.cs`：QFramework 架构注册点，注册所有 Model、System 和 Utility。

## Game

- `GameStart.cs`：启动 ResKit 与 GameArchitecture，打开角色选择/HUD，并逐帧驱动计时、敌人、攻击和玩家无敌帧命令。文件内的 `CameraFollow` 在 LateUpdate 跟随已生成玩家。
- `GameStart.Designer.cs`：QFramework 为 GameStart 生成的绑定代码，不手动修改。
- `PlayerController.cs`：Unity 输入与 Rigidbody2D 桥接；读取桌面移动轴并发送玩家移动命令。
- `PlayerHealthBarView.cs`：监听玩家受伤事件，按当前血量刷新世界空间 Slider。
- `CombatEntity.cs`：为运行时对象标识阵营，使玩家与敌人可走同一套攻击目标判定。
- `ContactAttackTrigger.cs`：Unity 触碰桥接；检测不同阵营碰撞体并向 AttackSystem 请求释放攻击。

## Editor

- `ConfigurationCreatorWindow.cs`：Unity 编辑器窗口。可创建角色或怪物内容预制体模板和对应配置，自动分配数字 ID；Enemy 创建时自动加入 EnemyCatalog。

## UI

- `UICharacterSelectPanel.cs`：角色选择面板逻辑，构建角色条目、记录选中项并确认开局。
- `UICharacterSelectPanel.Designer.cs`：角色选择面板的自动生成 Bind 字段，不手动修改。
- `UICharacterSelectItem.cs`：单个角色条目控制器，通过既定子节点路径填充配置并处理点击。
- `UIGameHUDPanel.cs`：局内 HUD，显示累计计时并处理返回按钮。
- `UIGameHUDPanel.Designer.cs`：局内 HUD 的自动生成 Bind 字段，不手动修改。

## 维护规则

- 新增攻击时创建新的 `IAttackExecutor` 并注册 `ExecutorId`；不要恢复 `AttackType` 枚举或集中分支。
- 新增脚本后同步更新本文和 `Assets/AGENTS.md`。
- UI 层级、Prefab 与 Bind 仅由用户创建或调整；脚本只做后续接入。
