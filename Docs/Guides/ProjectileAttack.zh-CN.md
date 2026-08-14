# Projectile 攻击接入

本文只说明投射物 Attack 的配置和运行边界；通用攻击链见 `AttackFlow.zh-CN.md`。

## 配置

创建 `Resources/Configs/Combat/Attack_<技能名称>.asset`，在 AttackCatalog 注册唯一数字 ID，并将 `ExecutorId` 设为 `projectile`。参数资产引用子弹 Prefab，提供基础速度、生命周期、射程、穿透和池容量；这些基础值不可在升级时修改。

## 运行方式

`ProjectileAttackExecutor` 同时实现 `IAttackExecutor` 与 `IAutomaticAttackExecutor`。装配时注册自动 AttackRuntime；`AttackSystem.OnRunUpdate()` 在冷却结束时查找最近敌对目标，并调用 Executor 创建投射物。

`ProjectileFactory` 从对象池获取实例并注册到 `ProjectileSystem`。`ProjectileSystem` 集中调用 `ProjectileController.Advance()` 处理寿命，并在物理 Tick 调用 `AdvanceFixed()` 通过 Rigidbody2D 移动。`ProjectileController` 没有 Unity Update/FixedUpdate；命中后只发送通用伤害 Command 并按穿透次数回收。

## 升级边界

投射物数量、伤害、速度、穿透与冷却修正都写入 WeaponRuntime 的 Attack 修正数据，不修改 Attack 或参数 ScriptableObject。详情见 `WeaponUpgrade.zh-CN.md`。
