# ProjectSurvivor 架构边界

本文定义模块职责；运行顺序见 `GameLogic.zh-CN.md`，接入步骤见 `Guides/`。

## QFramework 分层

`GameArchitecture` 是业务模块的唯一注册点：

```text
Controller/View -> Command -> System -> Model/Utility
                         \-> Query/Event
```

Model 保存可变状态，System 负责行为和生命周期，Command 修改状态，Query 提供只读结果，Event 发布已完成事实。Unity 组件只负责输入、对象、物理和表现桥接。

## 当前模块

| 模块 | 职责 |
| --- | --- |
| Run | 对局阶段、逻辑时间、Tick 调度、结算边界 |
| Player | PlayerRoot、移动、生命、死亡和属性入口 |
| Enemy | 配置、生成、追击、生命、对象池 |
| Combat | Attack、Executor、目标查询、伤害、投射物和区域效果 |
| Skill | SkillGroup、Weapon 容器、Skill 运行时 |
| Dodge | 闪避配置、冷却、位移、无敌帧和升级 |
| Experience | 经验掉落、对象池、吸附和升级事件 |
| LevelUp | 候选生成、三槽选择、阶段切换 |
| Progression | 通用属性、自然回血、角色专属升级和联动效果 |
| Map | Seed、区块生成、障碍验证、流式加载、碰撞和 NavMesh |
| Save | 快照文件、迁移、恢复和游戏日志 |
| Presentation | WorldRoot 定位、九层 Sorting Layer、动态对象表现桥接 |
| UI | 显示与输入绑定；层级和 Bind 仍由用户维护 |

## 运行约束

`GameStart` 是唯一的游戏 `Update` / `FixedUpdate` 宿主。需要时间的 System 实现 `IRunUpdateable` 或 `IRunFixedUpdateable`，只在生命周期有效时向 `GameLoopSystem` 注册。禁止新增分散的业务 `MonoBehaviour.Update`、`FixedUpdate`、协程计时或直接使用 Unity 未缩放时间推进玩法。

ScriptableObject 只保存静态定义，Model 只保存局内状态，存档只保存 ID、数值和可重建世界数据。攻击行为通过 `ExecutorId` 扩展，禁止 AttackType 枚举和无架构约束的全局管理器。表现对象与对象池实例不进入存档。
