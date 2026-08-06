# 武器容器与进化

## 定义

`WeaponConfig` 是角色武器的静态容器模板，不是一次攻击。它提供 `InitialAttackIds`、`MaxLevel`、`CanUpgrade` 与每级可增删 Attack 的 `LevelUpgrades`。

`WeaponRuntimeData` 是本局实际持有的一把武器。每把武器有稳定的 `RuntimeId`、独立等级、可升级状态与当前 `AttackIds`。同一角色可以同时持有多把 Weapon；同一 Weapon 也可以容纳多个 Attack。

`AttackConfig` 仍只定义单一攻击行为，例如碰撞或投射物。Enemy 不使用 Weapon 容器，继续直接通过 `EnemyConfig.AttackIds` 配置攻击。

## 角色配置与出生

1. `CharacterConfig.SkillGroupId` 找到 `SkillGroupConfig`。
2. `StartingWeaponIds` 为角色创建多个 WeaponRuntime。
3. 每个 WeaponRuntime 从对应 `WeaponConfig.InitialAttackIds` 填充 Attack。
4. `PlayerLoadoutSystem` 为每个 Attack 配置独立 Trigger，并以 WeaponRuntimeId 区分不同武器容器。

当前 `Weapon_Projectile` 的初始 Attack 为 `1002`，最大等级为 5。

## 普通升级

升级系统将对目标 WeaponRuntime 发送 `UpgradeWeaponCommand`。`PlayerLoadoutSystem` 只允许满足以下条件的武器升级：

- `CanUpgrade = true`
- 当前等级低于 `MaxLevel`

到达下一级时，系统读取该等级的 `WeaponLevelUpgrade`：先移除 `RemoveAttackIds`，再加入 `AddAttackIds`。没有填写升级项时只增加等级，Attack 容器内容保持不变。

不要修改 `WeaponConfig` 或 `AttackConfig` 资产来表示本局升级。

## 替换武器与进化

`ReplaceWeaponCommand` 会在原 `RuntimeId` 槽位直接替换 Weapon：

1. 注销并移除旧 Weapon 容器的 Attack Trigger。
2. 将该 Runtime 的 `WeaponId` 改为目标 Weapon。
3. 等级重置为 1，并使用目标 `InitialAttackIds` 重新填充。
4. 注册新的 Attack Trigger，发布 `WeaponReplacedEvent`。

进化使用 `WeaponEvolutionCatalog.asset` 中的 `WeaponEvolutionConfig`：

- `SourceWeaponId`：待进化武器。
- `RequiredLevel`：达到此等级后可进化。
- `TargetWeaponId`：替换后的武器。

`EvolveWeaponCommand` 只接受 `MaxLevel = 1` 且 `CanUpgrade = false` 的目标 Weapon。成功后保持原槽位，但新 Weapon 固定为 1 级、不会再进入升级候选池，并发布 `WeaponEvolvedEvent`。

## 创建进化终态

1. 在 `Resources/Configs/Skills/` 创建新的 WeaponConfig，例如 `Weapon_ProjectileEvolved`。
2. 配置不同的 `InitialAttackIds`；可填入已有 Attack，也可先创建新的 AttackConfig 并加入 AttackCatalog。
3. 设置 `MaxLevel = 1`、`CanUpgrade = false`、`LevelUpgrades = []`。
4. 将新 Weapon 加入 `WeaponCatalog.asset`。
5. 创建 WeaponEvolutionConfig，填写来源武器、所需等级与目标武器。
6. 将该进化配置加入 `WeaponEvolutionCatalog.asset`。

## 读取与事件

- UI 或升级候选模块使用 `GetPlayerWeaponsQuery` 读取 WeaponRuntime 快照，不直接修改 Model。
- `WeaponEquippedEvent`、`WeaponUpgradedEvent`、`WeaponReplacedEvent`、`WeaponEvolvedEvent` 用于后续提示、特效、成就和升级候选刷新。
- 更换 Attack 填充物使用 `ReplaceWeaponAttacksCommand`，它会先注销旧 Trigger，再注册新的 Attack 容器内容。
