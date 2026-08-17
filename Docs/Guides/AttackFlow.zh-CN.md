# Attack 运行流程

本文说明共享 Attack 的运行职责；新增攻击步骤见 `NewAttack.zh-CN.md`。

## 配置边界

所有 Attack 由 `Resources/Configs/Combat/Attack/AttackCatalog.asset` 以数字 ID 索引。`ExecutorId` 选择具体实现；目标阵营从运行时拥有者的 `CombatFaction` 推导，配置不写目标阵营，也不使用 AttackType 分支。

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

自动攻击没有逐攻击的 Update Trigger。`AttackSystem` 仅在存在 AttackRuntime 时注册帧 Tick；冷却结束后，自动 Executor 查找目标并执行。当前 `projectile` Executor 采用此路径；投射物移动和生命周期由 `ProjectileSystem` 批量调度。

## 扩展约束

- Executor 实现具体攻击和必要的目标选择。
- 碰撞类攻击可以使用 Trigger；自动攻击实现 `IAutomaticAttackExecutor`。
- AttackSystem 统一拥有运行时、冷却和生命周期。
- DamageSystem 统一路由伤害。
