# Attack 运行流程

Attack 配置位于 `Resources/Configs/Combat/AttackCatalog.asset`，`ExecutorId` 决定具体行为。攻击不绑定固定目标阵营，由运行时拥有者推导敌对阵营。

## 通用链路

```text
配置 -> Trigger -> RegisterAttackCommand -> AttackSystem -> Executor -> DamageSystem
```

Trigger 只负责感知触发条件；AttackSystem 负责运行时、冷却和阵营校验；Executor 负责具体攻击表现；DamageSystem 将伤害交给玩家或敌人生命系统。

当前 ID 1001 使用 `collision`，ID 1002 使用 `projectile`。新增攻击应新增 Executor 和对应 Trigger，不增加 AttackType 分支。
