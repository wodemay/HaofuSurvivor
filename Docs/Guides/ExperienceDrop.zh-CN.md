# 经验掉落系统设计

## 目标

敌人死亡后生成经验结晶；玩家进入吸附范围时，结晶向玩家移动并被拾取。经验达到等级阈值后累计待升级次数，后续升级选择模块逐次消耗该次数。

```text
Enemy death -> Experience drop -> Magnet -> Collect -> Level up pending
```

## 静态配置

`ExperienceDropConfig` 是每种经验结晶的 ScriptableObject：

```text
Id
DisplayName
BaseExperience
Prefab
```

初期使用小、中、大三种结晶，例如 1、5、20 点经验。`EnemyConfig` 直接序列化引用对应的 `ExperienceDropConfig`。需要概率掉落或多种奖励时，再扩展 `DropTableId`，本阶段不提前实现。

`EnemyConfig` 预留 `DropTableId`，默认值为 `0`，表示当前不启用掉落表。现阶段运行时只读取 `ExperienceDropConfig`；不创建 `DropTableConfig`、Catalog、概率规则或掉落表逻辑。未来 `DropTableId > 0` 时，由掉落表接管结算，现有 `ExperienceDropConfig` 作为无掉落表时的默认经验掉落。

`ExperienceProgressionConfig` 保存每一级升到下一级所需经验。数值由配置表逐级定义，不在代码中写成长公式。

## 玩家吸取属性

经验吸取范围属于玩家运行时属性，而非经验结晶属性：

```text
CharacterConfig.BaseExperienceAbsorbRadius
-> PlayerStatModel.ExperienceAbsorbRadius
```

同一条玩家属性链还包含：

```text
CharacterConfig.BaseExperienceAbsorbAcceleration
-> PlayerStatModel.ExperienceAbsorbAcceleration

CharacterConfig.BaseExperienceAbsorbMaxSpeed
-> PlayerStatModel.ExperienceAbsorbMaxSpeed
```

角色配置提供基础值；`PlayerStatModel` 持有本局实际值；未来升级通过玩家属性修改该值，绝不修改 ScriptableObject。结晶距离玩家小于吸取范围时进入捕获状态；捕获后不再检查范围，始终追踪玩家当前位置，直到靠近到系统固定的极小收取距离并结算经验。

## QFramework 职责

- `ExperienceModel`：`Level`、`CurrentExperience`、`RequiredExperience`、`PendingLevelUpCount`。
- `ExperienceSystem`：生成、吸附、拾取、经验累加、升级判定、局内重置。
- `ExperienceFactory`：按结晶配置 ID 分池复用运行时对象。
- `ExperienceDropController`：Unity 桥接，仅保存 Transform 与当前实际经验值；不承载规则。
- `ExperienceProgressionCatalog`：加载等级需求配置。
- Commands：`SpawnExperienceDropCommand`、`CollectExperienceCommand`、`ResetExperienceCommand`。
- Events：`ExperienceCollectedEvent`、`PlayerLevelUpEvent`。

## 敌人死亡衔接

`EnemyDiedEvent` 必须携带不可变的 `ExperienceDropConfig` 与 `DeathPosition`。`ExperienceSystem` 监听事件后直接以该配置生成结晶。

禁止在事件回调中继续依赖 EnemyRoot：敌人死亡后会立即回收到对象池。

## 运行规则

- 吸附、拾取和结晶移动全部使用 `RunTimerModel.DeltaTime`；暂停时完全停止。
- 不以 Trigger 作为核心拾取判定；`ExperienceSystem` 按与玩家的距离推进结晶，减少 Collider 配置和对象池状态风险。
- 结晶不配置 `MagnetSpeed`。进入捕获状态后，当前速度从 `0` 开始按 `ExperienceAbsorbAcceleration × RunTimerModel.DeltaTime` 累加，并限制在 `ExperienceAbsorbMaxSpeed`；每帧以当前速度朝玩家实时位置移动。捕获状态不会因玩家高速移动而取消。
- 场上结晶达到配置上限时，新经验合并到最近的同类结晶，不丢失经验也不无限扩池。
- 重开或退出对局时，全部结晶回收到对象池，经验模型重置。

## 升级交接

一次获得大量经验可连续升级。`ExperienceSystem` 只更新等级并逐次发布 `PlayerLevelUpEvent`：

```text
ExperienceSystem -> PlayerLevelUpEvent
```

升级选择队列、候选项、确认和对局冻结由独立的 `LevelUpSystem` 处理，详见 `Docs/Guides/LevelUp.zh-CN.md`。经验模块不依赖武器、升级候选或升级 UI。

## 后续用户创建项

实现前由用户创建：

```text
Art/Prefabs/Drop/ExperienceOrb.prefab
MainScene/ExperienceDropContainer
UILevelUpPanel
```

其中 `UILevelUpPanel` 在经验与升级候选逻辑完成后再接入。
