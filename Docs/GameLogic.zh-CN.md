# 当前游戏逻辑总览

本文描述当前已落地的运行时逻辑，而不是最终完整策划。项目是 2D 吸血鬼幸存者式单机游戏，业务逻辑统一按 QFramework 划分：Unity 组件和 UI 只负责输入、显示、物理回调；状态由 Model 保存；状态变更通过 Command 进入 System；只读数据通过 Query；模块通知通过强类型 Event。

## 功能点与脚本定位

下表用于从功能反查代码。所有全局命令集中在 `Assets/Scripts/Architecture/Commands/GameplayCommands.cs`，跨模块事件集中在 `Assets/Scripts/Architecture/Events/GameplayEvents.cs`。

| 功能点 | 核心脚本 | 作用 |
|---|---|---|
| 模块注册与依赖装配 | `Architecture/GameArchitecture.cs` | 注册所有 Model、System、Catalog/Registry Utility。 |
| 场景启动与逐帧驱动 | `Game/GameStart.cs` | 初始化架构、打开面板、分发 Update/FixedUpdate 命令；同文件的 `CameraFollow` 处理跟随。 |
| 对局阶段与退出 | `Run/RunModel.cs`、`Run/RunSystem.cs` | 保存并切换 None、Active、Paused、LevelUpSelection、Victory、Defeat。 |
| 统一游戏时间与时间阶段 | `Run/RunTimerModel.cs`、`Run/RunTimerSystem.cs`、`Run/RunTimelineConfig.cs` | 提供局内 DeltaTime、暂停门禁、已进行时间和阶段倍率。 |
| 计时状态读取 | `Run/GetRunTimeStateQuery.cs`、`Run/GetRunTimerStateQuery.cs` | 给 HUD 或其他模块读取运行状态和阶段数据。 |
| 角色静态配置 | `Character/CharacterConfig.cs`、`Character/CharacterCatalog.cs` | 定义并读取角色 ID、属性、Prefab、技能组。 |
| 角色选择与持久化 | `Character/CharacterSelectionModel.cs`、`Character/CharacterSelectionSystem.cs`、`Character/CharacterSelectionStorage.cs` | 保存当前选择，并用 PlayerPrefs 持久化角色 ID。 |
| 选择界面 | `UI/UICharacterSelectPanel.cs`、`UI/UICharacterSelectItem.cs` | 生成角色条目、显示信息、确认开局。 |
| 玩家根节点创建 | `Player/PlayerRootConfig.cs`、`Player/PlayerSpawnSystem.cs` | 创建 PlayerRoot，挂接角色内容、血条、相机和初始技能组。 |
| 玩家输入与移动 | `Input/InputModel.cs`、`Input/InputSystem.cs`、`Game/PlayerController.cs` | 输入状态与 Rigidbody2D 移动的 Unity 桥接。 |
| 玩家状态与属性 | `Player/PlayerModel.cs`、`Player/PlayerStatModel.cs`、`Player/PlayerSystem.cs`、`Player/StatSystem.cs` | 保存生命、位置、死亡、无敌帧和角色属性。 |
| 玩家血条 | `Game/PlayerHealthBarView.cs` | 订阅受伤事件并刷新用户创建的 Slider。 |
| 敌人配置与目录 | `Enemy/EnemyConfig.cs`、`Enemy/EnemyCatalogConfig.cs`、`Enemy/EnemyConfigCatalog.cs` | 定义并加载多个敌人类型。 |
| 敌人生成、追击、回收 | `Enemy/EnemySystem.cs`、`Enemy/EnemyModel.cs`、`Enemy/EnemyFactory.cs`、`Enemy/EnemyRootConfig.cs` | 场外生成、固定帧追击、EnemyRoot 对象池和场景容器管理。 |
| 敌人生命与死亡 | `Enemy/EnemyHealthSystem.cs` | 注册敌人生命、处理受伤死亡、发送死亡事件和回收根节点。 |
| 阵营与目标查询 | `Game/CombatEntity.cs`、`Combat/CombatFaction.cs`、`Combat/CombatTargetSystem.cs` | 标识 Player/Enemy 阵营，查询最近敌对目标。 |
| 攻击通用状态和执行器 | `Combat/AttackConfig.cs`、`Combat/AttackCatalog.cs`、`Combat/AttackSystem.cs`、`Combat/AttackExecutorRegistry.cs` | 加载攻击定义、管理冷却/运行时状态、按 ExecutorId 执行攻击。 |
| 碰撞攻击 | `Game/CollisionAttackTrigger.cs` | 在 Trigger 重叠时向 AttackSystem 请求攻击。 |
| 投射物攻击与参数 | `Game/ProjectileAttackTrigger.cs`、`Combat/ProjectileAttackParameterConfig.cs` | 自动选取目标并按配置请求发射。 |
| 投射物移动与对象池 | `Game/ProjectileController.cs`、`Game/ProjectileFactory.cs` | 飞行、命中、寿命/距离判定和回收。 |
| 通用伤害与玩家受伤 | `Player/DamageSystem.cs` | 根据目标阵营将伤害转发给玩家或敌人模块。 |
| 经验配置和经验球 | `Experience/ExperienceDropConfig.cs`、`Experience/ExperienceFactory.cs`、`Experience/ExperienceDropController.cs` | 定义经验值/Prefab，生成、移动和回收经验球。 |
| 经验累计与升级事件 | `Experience/ExperienceModel.cs`、`Experience/ExperienceProgressionConfig.cs`、`Experience/ExperienceSystem.cs` | 累计经验、计算升级、发送 PlayerLevelUpEvent。 |
| 升级选择流程 | `LevelUp/LevelUpModel.cs`、`LevelUp/LevelUpSystem.cs` | 管理待选择队列、生成候选、暂停和恢复对局。 |
| 升级界面 | `UI/UILevelUpPanel.cs`、`UI/UILevelUpOptionItem.cs` | 显示武器候选并发送确认命令。 |
| 技能组 | `Skill/SkillGroupConfig.cs`、`Skill/SkillGroupCatalog.cs` | 定义角色初始武器、技能 ID、闪避 ID。 |
| 武器运行时容器 | `Skill/WeaponConfig.cs`、`Skill/WeaponRuntimeData.cs`、`Skill/PlayerLoadoutModel.cs`、`Skill/PlayerLoadoutSystem.cs` | 装备、升级、替换、清理每把武器的攻击内容和属性修正。 |
| 武器目录与进化 | `Skill/WeaponCatalog.cs`、`Skill/WeaponEvolutionConfig.cs`、`Skill/WeaponEvolutionCatalog.cs` | 按 ID 获取武器，定义满级后替换到的目标武器。 |
| 新角色/敌人配置编辑器 | `Editor/ConfigurationCreatorWindow.cs` | Unity 菜单中创建基础 CharacterConfig、EnemyConfig 及内容 Prefab 模板。 |

`*.Designer.cs` 为 QFramework 自动生成的 UI Bind 文件，只读，不应手改。

## 一局游戏如何开始

`GameStart` 是场景启动入口。它初始化 ResKit 和 `GameArchitecture`，订阅开局、升级选择事件，然后打开 `UICharacterSelectPanel`。

角色选择面板从 `Resources/Configs/Characters/` 加载 `CharacterConfig`。每名角色有数值型 `Id`、显示信息、角色内容 Prefab、基础属性和 `SkillGroupId`。确认选择后，选择 ID 会写入 `PlayerPrefs`，并发送 `StartSelectedCharacterRunCommand`。

`PlayerSpawnSystem` 根据角色配置创建 `PlayerRoot`：

- `PlayerRoot` 是运行时根节点，负责 Rigidbody2D、移动、阵营身份和相机跟随目标。
- 角色的美术/内容 Prefab 被挂到其 `CharacterRoot` 子节点。
- 血条 Canvas 被挂到 `HealthBarAnchor`；`PlayerHealthBarView` 订阅受伤事件刷新 Slider。
- 若角色带有技能组，`PlayerLoadoutSystem` 装配其初始武器、预留技能 ID 与预留闪避 ID；装配失败会中止开局，避免出现半初始化角色。

玩家创建和技能组装配成功后，`RunSystem.StartRun` 将阶段切为 `Active`，重置敌人、经验球与升级队列，并发送 `RunStartedEvent` 以打开 `UIGameHUDPanel`。

## 游戏循环与统一时间

`RunTimerSystem` 是局内模拟时间的唯一来源。`RunTimerModel` 保存：已进行秒数、普通帧 DeltaTime、物理帧 FixedDeltaTime、暂停状态、当前时间阶段，以及敌人的生命/伤害/移速/生成倍率。

`GameStart.Update` 的顺序是：

1. 发送 `TickRunTimerCommand(Time.unscaledDeltaTime)` 更新游戏时间。
2. 只有 RunPhase 为 `Active` 且计时器未暂停时，才处理经验球。
3. 再次确认仍在运行后，处理攻击冷却与玩家受伤无敌帧。

`GameStart.FixedUpdate` 先发送 `TickRunPhysicsCommand(Time.fixedDeltaTime)`，再在运行状态下发送敌人移动/生成命令。这里使用固定步长而不是 `fixedUnscaledDeltaTime`，避免暂停很久后恢复时产生一次巨大的物理位移。

暂停会将 RunPhase 切为 `Paused`，使计时器输出的两个 DeltaTime 都为 0，并将 `Time.timeScale` 设为 0。恢复时重新进入 `Active`。升级选择使用独立的 `LevelUpSelection` 阶段，但同样暂停统一时间。因此玩家、敌人、投射物、攻击冷却、经验球吸附和无敌帧都不会在暂停或升级界面背后继续结算。

`RunTimelineConfig` 定义按“已进行秒数”触发的时间阶段。达到一个阶段时，`RunTimerSystem` 更新敌人倍率并发送 `RunTimelineStageReachedEvent`。敌人生成与生命系统通过读取当前倍率生效；后续 Boss、事件和刷怪规则也应订阅该事件或查询计时状态，而不应自行读取 Unity 时间。

## 玩家、移动与生命

`PlayerController` 是 PlayerRoot 上的 Unity 侧桥接组件，目前通过旧 Input Manager 的 `Horizontal`/`Vertical` 读取桌面输入，并经 QFramework 输入/玩家模块驱动 Rigidbody2D 移动。相机上的 `CameraFollow` 在 `LateUpdate` 仅同步玩家 XY，保持相机原有 Z。

`PlayerModel` 保存当前玩家对象、位置、存活状态、生命值和受伤无敌剩余时间；`PlayerStatModel` 保存角色基础数值与经验吸取属性。伤害统一进入 `DamageSystem`：它根据目标的 `CombatEntity` 阵营分派到玩家或敌人处理。玩家受伤后会发送 `PlayerDamagedEvent`，由血条视图刷新。

当前无敌帧机制已经存在，但默认 `DamageInvulnerabilityDuration` 为 0，因此现在仍是每次有效伤害立即扣血。后续升级可以提高该数值，`PlayerSystem` 会在无敌剩余时间大于 0 时拒绝新的伤害。

## 敌人生成、追击与对象池

`EnemyConfigCatalog` 从敌人配置目录加载多个 `EnemyConfig`。配置包含敌人 ID、内容 Prefab、基础生命、移动/生成参数、攻击 ID 列表和直接引用的 `ExperienceDropConfig`。

`EnemySystem` 只在玩家已注册且局内处于 Active 时运行：根据时间和当前时间阶段的生成倍率，在玩家摄像机可视范围外随机生成敌人；随着时间增加，生成间隔缩短、每波数量增加，同时受最大存活数量限制。敌人以 Rigidbody2D 的 `MovePosition` 在固定帧追向玩家，移速也会乘以时间阶段倍率。

`EnemyFactory` 是持久化的单例工厂。它按 EnemyConfig ID 维护 FIFO 对象池：创建或取出 `EnemyRoot`，把内容 Prefab 放在 `CharacterRoot` 下，再将根节点放到场景的 `EnemyContainer` 下。敌人死亡或本局退出时不销毁，而是注销运行时状态、停用并回收到对应池。

池会主动跳过场景切换后 Unity 已销毁但 C# 引用尚未清空的对象。复用 EnemyRoot 时，既有攻击触发器会重新注册，不能重复 AddComponent；否则会造成重叠伤害和组件持续累积。

## 通用战斗架构

玩家与敌人不各自维护一套攻击系统，而是共用：

- `CombatEntity`：挂在运行时根节点，保存阵营（Player 或 Enemy）。
- `AttackConfig`：静态攻击定义，存于 `Resources/Configs/Combat/AttackCatalog.asset` 的索引中。
- `AttackSystem`：保存攻击运行时状态和冷却，拒绝同阵营伤害，调用执行器。
- `AttackExecutorRegistry`：按 `ExecutorId` 找到攻击执行器。
- `CombatTargetSystem`：提供“最近的敌对 CombatEntity”等 Query。

攻击配置没有 `AttackType` 枚举，也没有固定 `TargetFaction` 字段。攻击目标由实际持有者阵营推导：玩家默认攻击敌人，敌人默认攻击玩家。新增攻击方式时应新增执行器和对应 Unity 触发桥，而不是向中心系统增加类型分支。

### 碰撞攻击

攻击 ID `1001` 的执行器为 `collision`。敌人配置将该 ID 放入 `AttackIds` 后，EnemyRoot 上的 `CollisionAttackTrigger` 在 `OnTriggerStay2D` 发现敌对 CombatEntity 时，发送注册、尝试攻击和注销命令。冷却由 `AttackSystem` 管理；实际伤害先应用当前时间阶段的敌人伤害倍率，再由 `DamageSystem` 处理。

### 投射物攻击

攻击 ID `1002` 的执行器为 `projectile`，参数资产为 `ProjectileAttackParameter_Projectile`，引用 `Art/Prefabs/Attack/Bullet.prefab`。`ProjectileAttackTrigger` 查询最近敌对单位并按冷却请求攻击；执行器向目标方向发射投射物。

`ProjectileFactory` 在运行时创建 `ProjectileContainer` 并对投射物进行对象池复用。`ProjectileController` 负责移动、寿命、距离、命中和回池；同样会在取池时清理跨场景销毁的伪空引用。投射物命中时使用通用伤害入口，且有每发投射物的命中记录，防止同一目标在一次飞行中重复被击中。

## 敌人生命、死亡与经验掉落

敌人生成时由 `EnemyHealthSystem` 注册生命数据。`EnemyConfig.BaseHealth` 是静态基础值，实际初始生命乘以当前时间阶段的敌人生命倍率。当前四个敌人基础生命均为 20；初始投射物伤害为 10，因此开局需要两发命中击杀。

敌人生命归零时，`EnemyHealthSystem` 发送 `EnemyDiedEvent`，事件携带死亡位置和该敌人的 `ExperienceDropConfig`，然后将 EnemyRoot 还给敌人池。

`ExperienceSystem` 监听死亡事件，用 `ExperienceFactory` 在 `ExperienceDropContainer` 下取出或创建 `ExperienceOrb`。当前敌人直接引用标准经验配置，每只掉落 1 点经验；`DropTableId` 已预留但值为 0，当前不参与任何概率表或随机逻辑。

经验球初始停在死亡点。玩家进入 `ExperienceAbsorbRadius` 后，经验球进入不可逆的“已捕获”状态：即使玩家高速移动并离开原吸取半径，也会持续追踪玩家当前坐标。它的速度从 0 开始，按玩家的吸取加速度提高，直到玩家的最大吸取速度；足够接近时回收到对象池并增加经验。吸取范围、加速度和最大速度属于角色属性，而不是经验球配置。

## 经验、等级与升级选择

`ExperienceModel` 保存当前等级、当前经验和升下一级所需经验。所需经验从 `ExperienceProgressionConfig` 读取。经验增加后允许在一次拾取中连升多级；每升一级都发送 `PlayerLevelUpEvent`。

经验模块到此为止，它不决定升级什么，也不直接操作 UI。`LevelUpSystem` 专门监听 `PlayerLevelUpEvent`，把待选择次数加入 `LevelUpModel` 队列。存在可用升级项时，它将局内切换到 `LevelUpSelection` 并发送 `LevelUpSelectionRequestedEvent`，由 `GameStart` 打开 `UILevelUpPanel`。

当前升级候选只来自玩家已拥有的武器：

- 未达到 MaxLevel 的可升级武器：显示“LevelX->LevelX+1”。
- 满足进化配置的满级武器：显示“LevelX->Evolve”。

UI 点击选项后发送升级确认命令。`LevelUpSystem` 调用 `PlayerLoadoutSystem` 升级或进化，完成当前队列项；如果仍有待选择次数，继续打开下一次选择，否则恢复 Active 阶段。当前尚未实现：新武器候选、新技能候选、闪避升级、随机权重、刷新/跳过和完整掉落表。

## 技能组、武器、攻击与进化

角色的 `SkillGroupId` 指向 `SkillGroupConfig`。技能组包含初始武器 ID 列表、初始技能 ID 列表和一个闪避 ID。当前只有“初始武器”真正接入战斗；技能与闪避只是已保留的数据接口。

武器不是具体攻击，而是攻击的运行时容器：

- `WeaponConfig` 是静态模板，定义初始 Attack ID、最大等级、是否可升级、升级规则和显示文本。
- `WeaponRuntimeData` 是单局运行时实例，保存稳定的 RuntimeId、当前 WeaponId、等级、当前 Attack ID 列表和攻击修正值。
- 一个玩家可以拥有多个 WeaponRuntimeData，因此未来可以同时持有多把武器。
- 敌人不需要 Weapon 容器，直接从 `EnemyConfig.AttackIds` 装配攻击即可。

`PlayerLoadoutSystem` 负责装配、升级、替换攻击和清理触发器。升级绝不修改 ScriptableObject；它只修改当前局的 WeaponRuntimeData。攻击触发器带有 WeaponRuntimeId，因此同一玩家的不同武器可以持有相同攻击而互不混淆，移除或替换时也能精确清理。

当前完整链路为 `Weapon_Projectile`：

| 等级 | 当前效果 |
|---|---|
| 1 | 初始投射物攻击 |
| 2 | 投射物数量 +1 |
| 3 | 冷却降低 20% |
| 4 | 伤害 +5、速度 +25% |
| 5 | 投射物数量 +1、穿透 +1 |
| 进化 | 替换为 Weapon 2，使用 Attack 1003，等级重置为 1，且不可升级 |

通用的 `WeaponAttackModifier` 存在 WeaponRuntimeData 中。AttackSystem 读取冷却修正；ProjectileAttackExecutor 读取数量、伤害、速度和穿透修正，并以扇形多发生成投射物。以后新武器只需在其配置中定义自身升级规则；若某种攻击需要新的修正字段，则由对应执行器读取，不应污染其他攻击逻辑。

进化由 `WeaponEvolutionCatalog` 定义“源武器 ID + 所需等级 -> 目标武器 ID”。进化时保持原 WeaponRuntimeId，先移除旧攻击触发器，再将该运行时槽位替换成目标武器的初始攻击。目标武器必须是 `MaxLevel = 1` 且 `CanUpgrade = false`，否则进化命令会拒绝执行。

## UI 当前职责

UI 的 GameObject 层级、Prefab 和 Bind 均由项目维护者创建。代码只接入既有绑定：

- `UICharacterSelectPanel`：创建角色条目、记录选择、确认开局。
- `UICharacterSelectItem`：用固定 `Transform.Find` 路径设置头像、名称、技能说明和选中状态。
- `UIGameHUDPanel`：显示运行时间；暂停按钮在 Active/Paused 间切换；返回按钮当前临时结束本局并返回角色选择。
- `UILevelUpPanel`：读取升级候选并发送升级确认命令。
- `UILevelUpOptionItem`：显示武器名、描述和 `LevelX->LevelX+1` 或 `LevelX->Evolve`。

返回角色选择只是临时流程：它会停止计时、回收敌人与经验球、清空升级队列、销毁当前 PlayerRoot、关闭 HUD、打开选择界面。后续正式主菜单/结算流程应替换这一入口。

## 当前配置位置与扩展边界

| 内容 | 位置 |
|---|---|
| 角色 | `Assets/Resources/Configs/Characters/` |
| 敌人和敌人目录 | `Assets/Resources/Configs/` 的 Enemy 配置与目录资产 |
| 时间阶段 | `Assets/Resources/Configs/RunTimeline.asset` |
| 攻击总索引 | `Assets/Resources/Configs/Combat/AttackCatalog.asset` |
| 武器与进化总索引 | `Assets/Resources/Configs/Weapon/` |
| 投射物武器相关资产 | `Assets/Resources/Configs/Weapon/Projectile/` |
| 经验与成长 | `Assets/Resources/Configs/ExperienceDrop_Standard.asset`、经验成长配置 |

后续新增角色、敌人、攻击、武器时，优先复用已有配置和执行器边界。新增攻击行为应创建 ExecutorId 对应执行器；新增角色/敌人应只提供内容 Prefab 和配置，不绕过 PlayerRoot、EnemyRoot、Factory、对象池和 QFramework 的命令链路。

## 当前未完成项

- 正式角色美术、多个可选角色的完整技能组。
- 主动技能、闪避、技能升级与新武器获取。
- 敌人远程攻击和特殊攻击执行器。
- Boss、事件、掉落概率表和时间阶段的具体内容。
- 死亡、胜利、结算、局外养成与正式返回主菜单流程。
- 自动化 Unity Test Framework 测试；目前以编译、MCP Console 和 Play Mode 做人工验证。
