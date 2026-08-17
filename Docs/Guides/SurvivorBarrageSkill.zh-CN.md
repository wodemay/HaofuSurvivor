# 幸存者环形弹幕技能

本文只说明幸存者首个专属技能的配置、装配与运行时职责；通用攻击契约见 `AttackFlow.zh-CN.md`，投射物对象池见 `Projectile.zh-CN.md`。

## 配置

`Resources/Configs/Combat/Skill/SkillCatalog.asset` 收录 `Skill_SurvivorBarrage`（技能 ID `1`）。该技能的 `InitialAttackIds` 指向攻击 ID `1004`。

`Resources/Configs/Combat/Skill/SurvivorBarrage/Attack_SurvivorBarrage.asset` 使用 Executor ID `barrage-projectile`，冷却为 15 秒，单发伤害为 5。其参数资产复用 `Art/Prefabs/Attack/Bullet.prefab`：持续 3 秒，每 0.025 秒发射一发（首发立即触发，约 121 发），速度为 16；发射圆以 720°/秒顺时针绕玩家旋转，轨道半径为 1.25，圆内随机偏移半径为 0.35。共享对象池会按实际并发数量自动扩容。

`SkillGroup_ProjectileStarter` 的 `StartingSkillIds` 包含技能 ID `1`，因此幸存者选择后自动获得该能力。

## 运行时

`PlayerLoadoutSystem.EquipSkill()` 从 `SkillCatalog` 创建 `SkillRuntimeData`，并以负运行时 ID 注册技能 Attack；该 ID 与 WeaponRuntime 的正 ID 隔离。若技能配置缺失或攻击 Executor 不存在，能力会被跳过，不阻断武器装配和对局启动。

`InputSystem` 检测 Space 并发送 `RequestSkillCommand`；`PlayerLoadoutSystem` 将请求交给对应技能的 AttackRuntime。`AttackSystem` 负责 ID `1004` 的 15 秒冷却。`BarrageProjectileAttackExecutor` 不需要目标，触发后交给 `BarrageProjectileSystem`：它随机初始环绕角度，让发射圆顺时针绕玩家旋转，并从圆内随机位置持续生成投射物。每发子弹沿玩家指向发射圆的径向向外飞行，如同沿悠悠球绳子的方向甩出。系统调用 `ProjectileFactory`，仍由 `ProjectileSystem` 推进和回收投射物。

暂停、升级选择不会派发 GameLoop，因此未完成的轮次会冻结；对局开始、退出或重开会调用 `BarrageProjectileSystem.Reset()` 清空未发射轮次。

## 扩展边界

技能升级尚未接入等级选择。后续升级只能修改 `SkillRuntimeData` 中的运行时修正，不能写回 ScriptableObject；可扩展轮数、单轮数量、伤害、冷却或附加 Attack。
