# Projectile 攻击接入

创建 `Attack_<技能名称>` 配置，并在 AttackCatalog 注册唯一数字 ID。将 `ExecutorId` 设置为 `projectile`，参数资源引用子弹 Prefab。

投射物由 `ProjectileFactory` 池化，`ProjectileController` 使用统一逻辑帧和物理帧移动，命中敌对 CombatEntity 后发送通用伤害命令。参数资源保存基础速度、寿命、射程、穿透和池容量；升级修正写入 WeaponRuntime，不改资源。
