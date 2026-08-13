# ProjectSurvivor 架构边界

本文只定义架构边界，不重复具体运行流程和资源创建步骤。

## QFramework 分层

```text
Controller/View -> Command -> System -> Model/Utility
                         \-> Query/Event
```

- Model 保存可变状态，不调用 System。
- System 负责领域行为和生命周期。
- Command 表达改变状态的请求。
- Query 只返回查询结果。
- Event 只通知已经发生的事实。
- Controller 和 UI 只负责 Unity 桥接。

## 当前模块职责

| 模块 | 唯一职责 |
| --- | --- |
| Run | 对局阶段、统一时间、结算边界和存档时机 |
| Player | PlayerRoot、移动、生命、死亡 |
| Enemy | 敌人配置、生成、移动、生命和对象池 |
| Combat | Attack 配置、Executor、目标和伤害路由 |
| Skill | 技能组、Weapon 容器和运行时 Attack 内容 |
| Experience | 经验掉落、吸附和经验计算 |
| LevelUp | 升级队列、候选项和选择阶段 |
| Save | 局内快照序列化与恢复 |
| UI | 显示和输入；层级、Prefab、Bind 由用户维护 |

## 数据边界

- ScriptableObject 只保存不可变配置。
- Model 只保存运行时可变状态。
- 存档禁止保存 Unity 场景对象引用。
- 当前存档使用一个 PlayerPrefs JSON 槽位。

新增业务必须进入对应 System，并通过 Command、Query 或 Event 暴露；禁止增加无架构约束的全局管理器。
