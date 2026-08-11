# 武器升级链路

## 配置目录

`Resources/Configs/Weapon/` 保存全局 `WeaponCatalog` 与 `WeaponEvolutionCatalog`。单个投射物武器的 Weapon、进化、Attack 和投射物参数统一放在 `Resources/Configs/Weapon/Projectile/`；`Resources/Configs/Combat/AttackCatalog.asset` 只保留全局 Attack ID 索引。

## 运行时职责

`WeaponConfig.LevelUpgrades` 定义每一级的显示文本、Attack 增删和 `WeaponAttackModifier` 列表。`PlayerLoadoutSystem` 在确认升级时将修正累加到对应 `WeaponRuntimeData`；静态 ScriptableObject 不会被改写。

每个修正由 `AttackId`、`Key`、`Value` 组成。AttackSystem 读取 `Attack.CooldownMultiplier` 计算有效冷却；具体 Executor 读取自身键。投射物 Executor 当前支持：

- `Projectile.CountAdd`
- `Projectile.DamageAdd`
- `Projectile.SpeedMultiplier`
- `Projectile.PierceAdd`

新武器类型只需让自己的 Executor 解释新的键，不在升级系统增加武器类型分支。

## 首条配置路径

`Resources/Configs/Weapon/Projectile/Weapon_Projectile.asset`：

- 2 级：子弹数量 +1。
- 3 级：冷却乘以 0.8。
- 4 级：伤害 +5、速度乘以 1.25。
- 5 级：子弹数量 +1、穿透 +1。

该资产的 `DisplayName`、`Description` 与每级 `Description` 都是直接显示给玩家的中文文本；`AttackId`、`Key`、资源文件名和 `ExecutorId` 属于内部标识，不参与本地化。

`ProjectileController` 将穿透视为额外可命中目标数，并记录本次发射已命中的目标，避免多碰撞体重复受伤。

## 进化

`WeaponEvolution_Projectile.asset` 将武器 1 在 5 级时替换为 `Weapon_ProjectileEvolved`（ID 2）。新武器固定 1 级、`CanUpgrade = false`，使用 `Attack_ProjectileEvolved`（ID 1003）。

`LevelUpSystem` 在普通升级候选耗尽后查询进化目录，并通过现有 `EvolveWeaponCommand` 复用原 WeaponRuntime 槽位。升级面板显示 `Level5->Evolve`；无需修改 UI 层级或 Bind。
