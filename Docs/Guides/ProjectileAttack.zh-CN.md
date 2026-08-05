# 子弹攻击接入说明

`Attack_Projectile` 是通用远程攻击，不绑定 Player 或 Enemy。攻击拥有者启用后，`ProjectileAttackTrigger` 通过 `FindClosestCombatTargetQuery` 获取射程内最近的敌对 `CombatEntity`；`AttackSystem` 校验冷却后由 `ProjectileAttackExecutor` 创建子弹。

## 当前配置

| 资产 | 作用 |
| --- | --- |
| `Resources/Configs/Combat/Attack_Projectile.asset` | 攻击 ID 1002，Executor ID 为 `projectile`。 |
| `Resources/Configs/Combat/ProjectileAttackParameter_Projectile.asset` | 子弹预制体、速度、生命期、射程、对象池参数。 |
| `Art/Prefabs/Attack/Bullet.prefab` | 用户创建的子弹外观与 2D 物理组件。 |

当前参数：速度 8、生命期 3 秒、射程 8、池容量 16。伤害与冷却属于 `Attack_Projectile`，不要在参数资产中重复填写。

## 挂载方式

将 `1002` 填入任意 `EnemyConfig.AttackIds`，该 Enemy 即会自动攻击射程内的 Player。Player 侧等待 Skill 模块将 Character 的 `SkillGroupId` 映射到 Attack ID 1002；不需要改动攻击资产或执行器。

## 命中与回收

子弹只会对不同阵营的 `CombatEntity` 生效，命中或超时后由 `ProjectileFactory` 回收到运行时 `ProjectileContainer`。当前 `DamageSystem` 已可结算 Player 受伤；Enemy 生命模块尚未实现，因此玩家子弹命中 Enemy 的扣血效果待该模块接入。

## 升级扩展点

`ProjectileAttackParameterConfig` 保存基础值。未来升级系统按 Attack ID 1002 生成运行时修正值，并在 `ProjectileAttackExecutor` 发射前合并速度、数量、穿透、范围或冷却修正；不要直接修改 ScriptableObject 资产。
