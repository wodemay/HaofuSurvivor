# ProjectSurvivor 项目总纲

本文回答三个问题：游戏需要哪些功能、每项功能如何落地、模块之间如何关联。它是项目的功能地图和开发顺序，不替代：

- `DevelopmentPipeline.zh-CN.md`：规定如何开发和交付功能。
- `GameArchitecture.zh-CN.md`：规定 QFramework 代码边界。
- `DevelopmentRoadmap.zh-CN.md`：记录总纲确定后的未完成事项。
- `Docs/Guides/`：记录单个系统的详细接入方法。

## 一、最终游戏闭环

```text
主菜单
  -> 角色选择
  -> 创建/恢复对局
  -> 玩家移动、攻击、闪避
  -> 敌人生成、追击、攻击
  -> 击杀敌人
  -> 掉落经验和物品
  -> 经验升级选择
  -> 武器、技能、闪避、属性和角色专属升级
  -> 地图探索与事件
  -> 敌人阶段增强
  -> 死亡或达成胜利条件
  -> 结算
  -> 重开、返回主菜单或继续游戏
```

所有局内功能都必须受 `RunPhase` 和 `GameLoopSystem` 控制；所有需要恢复的局内状态必须能够序列化，所有表现对象都必须能够从数据重建。

## 二、模块总表

状态说明：

- 已接入：代码链路已经存在，后续主要是验证和扩展。
- 基础版：能跑通第一版流程，但还不是完整内容。
- 待开发：尚未形成可玩的实现。

| 模块 | 玩家可见功能 | 主要实现方式 | 当前状态 | 依赖 |
| --- | --- | --- | --- | --- |
| 启动与菜单 | 启动、开始、设置入口、退出 | `GameStart` 初始化架构并打开 `UIMainMenuPanel` | 基础版 | Run、UI |
| 角色选择 | 选择角色并开始对局 | `CharacterCatalog` 加载 `CharacterConfig`，Command 记录选择 | 已接入 | Character、Skill |
| 对局生命周期 | 开始、暂停、恢复、死亡、重开、返回、结算 | `RunSystem` 管理 `RunPhase`，Commands 作为外部入口 | 已接入，需回归 | Player、Enemy、Save、UI |
| 统一时间 | 所有移动、攻击、掉落、地图和回血使用同一逻辑时间 | `RunTimerSystem` + `GameLoopSystem` 分发 Tick | 已接入 | Run |
| 玩家运行时 | 移动、生命、受伤、死亡、闪避位移 | `PlayerSystem` 管理 PlayerRoot，组件只做桥接 | 已接入 | Run、Map、Progression |
| 角色内容 | 角色 Prefab、基础属性、专属配置 | `CharacterConfig` 引用内容 Prefab，运行时挂载到 `CharacterRoot` | 基础版 | Player、SkillGroup |
| Weapon | 多把武器、武器等级、Attack 替换、进化和合成 | `WeaponConfig` 是容器模板，`WeaponRuntimeData` 保存局内状态 | 基础版 | Combat、LevelUp、Progression |
| Attack | 碰撞、投射物、爆炸区域和手动技能 | `AttackConfig.ExecutorId` 选择独立 Executor | 基础版 | Combat、Run |
| Skill | 手动释放角色技能 | SkillRuntime 绑定 Attack，输入通过 Command 触发 | 基础版 | Input、Combat、LevelUp |
| Dodge | Shift 闪避、位移、冷却、无敌帧 | `DodgeSystem` 通过 `PlayerSystem.MoveBy` 移动 | 已接入 | Input、Run、Map、LevelUp |
| 敌人 | 生成、追击、寻路、攻击、受伤、死亡、回收 | `EnemyFactory` + `EnemySystem` + `EnemyHealthSystem` | 基础版 | Run、Map、Combat |
| 经验 | 敌人掉落、吸附、加速移动、收集 | `ExperienceSystem` 对象池和经验状态 | 已接入 | Enemy、Player、Run |
| 升级选择 | 三个候选、选择后继续对局 | `LevelUpSystem` 生成 `LevelUpOption`，UI 发送确认 Command | 基础版 | Experience、Weapon、Progression、UI |
| 通用属性 | 攻击力、冷却、移速、吸取范围、经验倍率、回血、恢复效率 | `PlayerStatUpgradeSystem` 保存等级并重算运行时倍率 | 基础版 | LevelUp、Player、Combat |
| 角色专属升级 | 角色固定的专属升级和联动效果 | 角色专属 Catalog + System，作为独立候选类型 | 基础版 | Character、LevelUp、Weapon、Dodge、Skill |
| 地图 | 无限区块、地面、障碍、碰撞、寻路和流式加载 | `MapSystem` 生成数据，`MapChunkView` 显示，NavMesh 提供寻路 | 基础版 | Run、Player、Enemy、Combat |
| 存档 | 继续游戏、玩家和地图状态恢复 | 根目录 JSON 快照，Model 数据优先，View 运行时重建 | 基础版，需增强 | Run、Player、Enemy、Map、Progression |
| 地图事件 | 击杀目标、区域事件、奖励和事件状态 | 事件配置 + 运行时状态 + 地图数据持久化 | 待开发 | Map、Enemy、Progression、Save |
| 道具与奖励 | 金币、恢复道具、宝箱和掉落物 | 配置化 Drop/Reward，运行时对象池和数据快照 | 待开发 | Enemy、Map、Player、Save |
| 局外成长 | 金币、解锁、永久升级、图鉴和成就 | 独立 Profile 存档，不混入局内快照 | 待开发 | RunSettlement、Save、UI |
| 表现层 | 九层世界渲染、血条、特效、伤害反馈 | Presentation 只负责渲染层和 View | 基础版 | 所有运行时模块 |
| 工具链 | 文档查看、静态检查、Unity 编译、Release | PowerShell 文档生成 + GitHub Actions | 已接入 | Git、Unity |

## 三、模块关联关系

### 1. 启动与对局创建

```text
GameStart
  -> GameArchitecture
  -> UIMainMenuPanel
  -> UICharacterSelectPanel
  -> StartSelectedCharacterRunCommand
  -> RunSystem.StartRun
  -> MapSystem.PrepareForRun
  -> PlayerSpawnSystem.SpawnSelectedCharacter
  -> PlayerLoadoutSystem.EquipInitialSkillGroup
  -> GameLoopSystem.BeginRun
```

玩家生成失败时，必须终止本次对局创建并释放已经创建的运行时对象；可选技能、可选表现和非核心内容失败时，只禁用对应能力并记录错误。

### 2. 玩家、技能组与战斗

```text
CharacterConfig
  -> SkillGroupConfig
  -> PlayerLoadoutSystem
      -> WeaponRuntimeData
      -> SkillRuntime
      -> DodgeRuntime
  -> AttackSystem
      -> AttackExecutorRegistry
      -> DamageSystem
      -> PlayerSystem / EnemyHealthSystem
```

Weapon 是可升级的 Attack 容器；Attack 是实际行为。Skill 和 Dodge 使用同一套运行时注册和冷却思想，但拥有各自的配置、升级和执行入口。

### 3. 敌人、地图与攻击

```text
RunTimerSystem
  -> EnemySpawnSystem
  -> EnemyFactory
  -> EnemySystem
      -> MapNavMeshSystem
      -> Rigidbody2D.MovePosition
  -> Enemy AttackSystem
  -> DamageSystem
```

地图系统提供可通行空间、障碍碰撞和 NavMesh；敌人系统只负责追击和移动决策，不把地图生成逻辑写进敌人模块。投射物和爆炸区域也必须检测地图阻挡。

### 4. 击杀、经验与升级

```text
EnemyHealthSystem
  -> EnemyDiedEvent
      -> ExperienceSystem
      -> Future Drop/RewardSystem
  -> ExperienceCollected
  -> ExperienceSystem
  -> PlayerLevelUpEvent
  -> LevelUpSystem
  -> UILevelUpPanel
  -> ConfirmLevelUpOptionCommand
  -> PlayerLoadoutSystem / PlayerStatUpgradeSystem / CharacterExclusiveSkillUpgradeSystem
```

升级系统只负责候选生成和选择阶段，不直接实现每种升级效果。具体升级由对应模块执行，失败时由 `LevelUpSystem` 重新生成候选，不能让对局卡在半初始化状态。

### 5. 存档与世界恢复

```text
RunSystem
  -> RunSaveSystem
      -> RunTimerModel
      -> PlayerModel / PlayerStatModel
      -> Loadout runtime data
      -> Enemy runtime data
      -> Experience/drop data
      -> Map WorldSeed / chunk data / event data
  -> JSON file under game root
```

恢复顺序固定为：

1. 读取并校验快照版本。
2. 恢复对局阶段、时间和地图身份。
3. 重新准备地图区块与障碍数据。
4. 按保存的角色和位置生成 PlayerRoot。
5. 恢复 Weapon、Skill、Dodge、属性和生命状态。
6. 恢复敌人、经验球、道具和事件运行时数据。
7. 重新注册 Attack、Tick、对象池和表现 View。
8. 校验恢复结果后才进入 Active。

表现对象、临时缓存和 NavMesh 运行时对象不直接写入快照，应由数据重新生成。

## 四、每个功能的实现规则

### 配置类功能

静态定义使用 `Resources/Configs/` 下的 ScriptableObject，使用整数 ID 和 Catalog 加载。配置不保存运行时等级、冷却、生命或位置。

### 运行时功能

状态进入 Model，行为进入 System，外部动作进入 Command，读取进入 Query，跨模块完成通知使用 Event。MonoBehaviour 只负责 Unity 输入、碰撞、生命周期和表现桥接。

### 时间相关功能

移动、攻击、投射物、经验吸附、自然回血、地图加载和敌人寻路必须接入 `GameLoopSystem`。暂停、升级选择、死亡和结算阶段不分发玩法 Tick。

### 对象池功能

池对象必须支持启用时重新注册、禁用时注销、场景切换时丢弃伪空引用，并清理上一次运行的攻击、伤害、目标和计时状态。

### 可选模块功能

可选模块的配置缺失或初始化失败不得阻止核心对局。模块必须提供清晰的“未启用”状态，不能返回半初始化对象。

## 五、开发里程碑

### 里程碑 M0：技术底座

完成 QFramework 架构、统一时间、模块生命周期、对象池、存档路径、CI 和 Release。

验收：能够稳定启动、暂停、重开、返回，并且没有模块重复注册或跨场景失效引用。

### 里程碑 M1：核心垂直切片

使用幸存者、基础投射物、环形弹幕、普通闪避和基础敌人完成一局完整流程。

验收：角色选择 → 战斗 → 经验 → 升级 → 死亡 → 结算 → 重开/返回全部跑通。

### 里程碑 M2：成长闭环

完成武器升级、属性升级、角色专属升级、武器进化和快照恢复。

验收：至少一个角色拥有一条完整成长路线，所有等级和组合状态可保存并恢复。

### 里程碑 M3：战斗内容扩展

完成火球术进化、范围攻击、持续伤害、精英敌人和 Boss 多 Attack。

验收：新 Attack 只需新增配置、参数和 Executor，不修改中央攻击分支。

### 里程碑 M4：地图闭环

完成无限地图、障碍规则、连通性、NavMesh、区块流式加载和地图状态恢复。

验收：玩家持续移动时无明显卡顿，敌人不会被障碍永久卡死，返回旧区域后状态一致。

### 里程碑 M5：世界内容

完成地图事件、道具、奖励、箭头指示和事件持久化。

验收：离开加载范围后，事件和道具状态不会被刷新或丢失。

### 里程碑 M6：局外成长与正式版本

完成结算资源、Profile、解锁、设置、本地化、性能采样和正式发行流程。

验收：局内存档与局外存档隔离，Windows/Mac 构建可以从 GitHub Release 下载并运行。

## 六、当前开发判断

当前不应直接继续堆叠新内容。优先顺序应为：

1. 完成当前存档重构的完整性、版本和恢复验证。
2. 以幸存者为样板完成 M1/M2 的回归闭环。
3. 修复地图初始加载、NavMesh、障碍碰撞和对象池边界问题。
4. 再扩展新的攻击、敌人和地图事件。

后续每提出一个新功能，先在本文确定它所属模块、依赖模块、保存字段、暂停行为和验收里程碑，再进入 `DevelopmentPipeline.zh-CN.md` 的生产流程。

## 七、功能完成定义

功能只有同时满足以下条件才算完成：

- 玩家行为、配置入口和模块边界已明确。
- 依赖的上游模块已经可用。
- 首次启动、暂停、重开、返回和销毁路径已评估。
- 运行时状态没有写回 ScriptableObject。
- 需要保存的数据已纳入快照或明确声明不保存。
- 可选能力失败不会阻断核心对局。
- 代码审查、C# 编译、静态检查和 Unity 手动验证结果已记录。
- 相关正式文档、Docs 查看器和 `Assets/AGENTS.md` 已同步。
