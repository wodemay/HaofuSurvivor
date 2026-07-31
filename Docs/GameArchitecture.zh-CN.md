# 2D 吸血鬼幸存类游戏架构

## 产品定义

面向 PC 和移动端的单机 2D 吸血鬼幸存类游戏。玩家直接控制移动、一把主武器、闪避和后续可解锁的大招；副武器自动释放。单局内通过经验升级、武器、被动与特性形成临时构筑；局内掉落、结算和成就获得的金币，用于局外永久成长。

## QFramework 规则

`GameArchitecture : Architecture<GameArchitecture>` 是唯一业务入口，负责注册全部 Model、System 与 Utility。

- `Model`：只保存运行时状态、持久化状态和可观察数据。
- `System`：实现领域行为和生命周期，可访问 Model 与 Utility。
- `Command`：执行改变状态的意图，如 `StartRunCommand`、`ApplyDamageCommand`、`PurchaseMetaUpgradeCommand`。
- `Query`：执行只读计算，如 `GetFinalStatQuery`、`GetUpgradeCandidatesQuery`。
- `Event`：传递强类型跨模块事实，不承载业务决策。
- `Controller`：仅作为 Unity 场景桥接层，发送 Command、读取 Query，并以安全生命周期订阅事件。

依赖方向固定为 Controller -> Command -> System -> Model/Utility。Model 禁止依赖 System；UI 禁止直接修改 Model。

## 局内运行模块

| 模块 | QFramework 角色 | 职责 | 核心输出 |
| --- | --- | --- | --- |
| 对局 | `RunModel`、`RunSystem` | 管理准备、进行、暂停、胜利、失败、结算等对局阶段。 | `RunStartedEvent`、`RunEndedEvent` |
| 输入 | `InputSystem` | 将键鼠、手柄、触控统一转换为移动、瞄准、闪避、主武器、大招意图。 | 输入快照/命令 |
| 玩家 | `PlayerModel`、`PlayerSystem` | 管理角色生成、生命、无敌、死亡和位置注册。 | `PlayerDamagedEvent`、`PlayerDiedEvent` |
| 属性 | `StatSystem` | 汇总基础属性、局外升级、局内升级、装备和 Buff。 | 最终属性 Query 结果 |
| 主武器 | `PrimaryWeaponSystem` | 管理手动瞄准、攻击频率、投射物/近战执行及武器资源。 | `PrimaryWeaponFiredEvent` |
| 自动武器 | `AutoWeaponSystem` | 管理副武器装备、自动索敌与自动攻击。 | `AutoWeaponFiredEvent` |
| 大招 | `UltimateSystem` | 管理解锁状态、充能/冷却、释放和大招效果。 | `UltimateUnlockedEvent`、`UltimateCastEvent` |
| 闪避 | `DodgeSystem` | 管理闪避次数、位移、无敌帧及所装备的闪避特性。 | `DodgeStartedEvent`、`DodgeEndedEvent` |
| 伤害与状态 | `DamageSystem`、`StatusEffectSystem` | 统一结算伤害、暴击、护甲、击退、治疗和持续状态。 | `DamageAppliedEvent`、`EntityKilledEvent` |
| 敌人 | `EnemySystem` | 管理敌人运行时状态、移动/攻击、精英词缀、死亡及对象池钩子。 | `EnemySpawnedEvent`、`EnemyKilledEvent` |
| 生成与关卡 | `SpawnSystem`、`StageSystem` | 管理关卡时钟、刷怪表、波次、精英/Boss 时机、地图规则、难度和胜利条件。 | `WaveStartedEvent`、`BossSpawnedEvent` |
| 掉落 | `DropSystem` | 创建与拾取经验、金币、宝箱、生命等局内掉落物。 | `DropCollectedEvent` |
| 经验与等级 | `ExperienceSystem`、`LevelSystem` | 管理经验获得、等级阈值、升级暂停和升级项选择。 | `PlayerLeveledUpEvent` |
| 局内构筑 | `BuildModel`、`BuildSystem` | 保存本局武器、被动、特性、进化、稀有度、层数与上限。 | `BuildChangedEvent` |
| 局内经济 | `RunRewardSystem` | 统计本局金币和结算奖励，不直接写入玩家永久档案。 | `RunGoldChangedEvent`、`RunRewardsCalculatedEvent` |

## 局外与内容模块

| 模块 | QFramework 角色 | 职责 |
| --- | --- | --- |
| 档案与存档 | `ProfileModel`、`SaveSystem`、`SaveUtility` | 加载、校验、迁移并原子化保存玩家永久档案。 |
| 局外成长 | `MetaProgressionModel`、`MetaProgressionSystem` | 消耗金币永久升级生命、攻击、暴击、经验倍率/范围、护甲、移速、复活及后续特殊能力。 |
| 背包 | `InventoryModel`、`InventorySystem` | 管理货币、拥有的消耗品、装备和待领取奖励。 |
| 解锁 | `UnlockModel`、`UnlockSystem` | 解锁角色、主武器、大招、闪避特性、自动武器、地图与难度。 |
| 成就 | `AchievementModel`、`AchievementSystem` | 统计进度、完成成就，并确保配置奖励只领取一次。 |
| 图鉴 | `CodexModel`、`CodexSystem` | 记录已发现的角色、武器、敌人、特性、关卡和 Boss。 |
| 商店 | `ShopSystem` | 通过 Command 提供购买入口；扣款委托给背包，解锁结果委托给解锁/局外成长模块。 |
| 内容 | `ContentUtility` | 读取角色、武器、技能、敌人、关卡、升级、成就与奖励等不可变 ID 配置。 |

## 共用服务

- `PoolSystem`：对象池化敌人、投射物、掉落物和表现对象，不拥有游戏规则。
- `AudioSystem`、`VfxSystem`：仅响应已发生的玩法事件。
- `LocalizationUtility`：通过文本 Key 解析玩家可见文本。
- `RandomUtility`：提供可设种子的局内随机；种子保存在 `RunModel`。
- `AnalyticsSystem`：可选本地事件记录，不允许影响游戏状态。

## 数据归属

| 数据 | 所属模块 | 生命周期 |
| --- | --- | --- |
| 输入快照、实体、波次时钟、局内构筑、局内奖励 | 运行时 Model | 单局 |
| 角色/武器/敌人/关卡定义 | Content Utility | 不可变资源数据 |
| 金币、永久升级、拥有物品、成就、解锁、图鉴 | Profile 相关 Model | 永久存档 |

所有定义使用稳定字符串 ID 引用。运行时实体只保存定义 ID 和可变状态。存档只保存 ID 与数值，禁止保存 Unity 场景对象引用。

## Command、Query 与 Event 契约

写操作示例：`StartRunCommand`、`MovePlayerCommand`、`FirePrimaryWeaponCommand`、`TryDodgeCommand`、`CastUltimateCommand`、`KillEnemyCommand`、`CollectDropCommand`、`ChooseRunUpgradeCommand`、`EndRunCommand`、`PurchaseMetaUpgradeCommand`、`ClaimAchievementRewardCommand`。

只读查询示例：`GetFinalStatQuery`、`GetAvailableDodgeQuery`、`GetUpgradeCandidatesQuery`、`GetRunRewardQuery`、`GetMetaUpgradeCostQuery`、`CanUnlockContentQuery`。

Event 只表达已经完成的事实。需要获得响应时，必须发送 Command 或 Query，不允许假设存在 Event 订阅者。

## UI 边界

UI View 通过 Model、Query 和 Event 获取展示数据。所有 UI 面板、预制体、层级改动、控件和自动生成绑定均由用户独立创建。任务一旦需要这些改动，必须暂停，等待用户确认 UI 已创建及绑定名称可用后，才接入非 UI 逻辑。

## 实施顺序

1. 建立 `GameArchitecture`、内容配置、档案/存档、属性计算和双端输入。
2. 实现玩家、对局、主武器、自动武器、伤害、敌人、生成/关卡、掉落、经验/等级和局内构筑。
3. 加入大招、闪避特性、精英/Boss、进化和对局结算。
4. 加入背包、局外成长、解锁、成就、图鉴和商店。
5. 对接用户创建的 UI，再完成音效、特效、本地化、数值平衡和平台专项优化。

## 验证

- 尽可能用无场景单元测试覆盖 Command、Query 和 System。
- 验证属性叠加顺序、暴击/护甲伤害、闪避无敌帧、主/自动武器冷却和大招解锁/释放规则。
- 验证升级候选生成、重复项限制、局内奖励结算、局外升级扣费、成就幂等领奖和存档迁移。
- 使用 Play Mode 测试验证波次推进、掉落拾取、胜负结算、对象池及 PC/触控模拟输入一致性。
