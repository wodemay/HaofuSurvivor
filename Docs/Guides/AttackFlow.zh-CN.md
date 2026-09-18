# Attack 运行流程

本文说明共享 Attack 的运行职责；新增攻击步骤见 `NewAttack.zh-CN.md`。

## 配置边界

所有 Attack 由 `Resources/Configs/Combat/Attack/AttackCatalog.asset` 以数字 ID 索引。`ExecutorId` 选择具体实现；目标阵营从运行时拥有者的 `CombatFaction` 推导，配置不写目标阵营，也不使用 AttackType 分支。

当前已在 `AttackExecutorRegistry` 注册的 Executor（全部实现在 `Architecture/Combat/AttackExecutorRegistry.cs`）：

| ExecutorId | 实现类 | 需要目标 | 注册方式 | 参考攻击 ID |
| --- | --- | --- | --- | --- |
| `collision` | `CollisionAttackExecutor` | 是 | `CollisionAttackTrigger` 发 Command | 1001 敌人接触伤害 |
| `projectile` | `ProjectileAttackExecutor` | 是 | `AttackSystem.RegisterAutomatic` | 1002 初始武器投射物、1005 火球 |
| `explosive-projectile` | `ExplosiveProjectileAttackExecutor` | 是 | `AttackSystem.RegisterAutomatic` | 1006 炼狱火球 |
| `barrage-projectile` | `BarrageProjectileAttackExecutor` | 否（手动） | `AttackSystem.RegisterManual` | 1004 幸存者环形弹幕 |

## 碰撞攻击

```text
CollisionAttackTrigger -> Register/TryExecuteAttackCommand -> AttackSystem -> CollisionAttackExecutor -> DamageSystem
```

`CollisionAttackTrigger` 只处理碰撞感知与注册。`AttackSystem` 保存运行时、校验阵营和冷却，然后调用 Executor；`DamageSystem` 是唯一伤害入口。对象池回收时，Trigger 注销运行时而不是重复添加组件。

## 自动攻击与投射物

```text
Executor.ConfigureOwner -> AttackSystem.RegisterAutomatic -> GameLoopSystem
-> AttackSystem.OnRunUpdate -> IAutomaticAttackExecutor.FindTarget -> Execute
```

自动攻击没有逐攻击的 Update Trigger。`AttackSystem` 仅在存在 AttackRuntime 时注册帧 Tick；冷却结束后，自动 Executor 查找目标并执行。当前 `projectile` 与 `explosive-projectile` 两个 Executor 采用此路径；投射物移动和生命周期由 `ProjectileSystem` 批量调度，爆炸范围与地面火焰由 `ExplosiveAreaSystem` 按 `GameLoopSystem` Tick 结算。

## 手动攻击与持续效果

```text
输入 Command -> AttackSystem.RegisterManual -> AttackSystem
-> IAttackExecutor.Execute -> 领域 System（如 BarrageProjectileSystem） -> GameLoopSystem Tick
```

`barrage-projectile` 的 `RequiresTarget` 为 `false`，由输入（`Space` → `RequestSkillCommand`）触发，冷却由 `AttackSystem` 统一持有。这类攻击把持续时间、发射节奏和伤害结算交给领域 System，由领域 System 注册 `IRunUpdateable` 在统一 Tick 中推进；表现 Prefab 只负责渲染。暂停、升级选择和结算不派发 GameLoop，因此未完成的轮次会冻结。

## 扩展约束

- Executor 实现具体攻击和必要的目标选择。
- 碰撞类攻击可以使用 Trigger；自动攻击实现 `IAutomaticAttackExecutor`。
- AttackSystem 统一拥有运行时、冷却和生命周期。
- DamageSystem 统一路由伤害。
