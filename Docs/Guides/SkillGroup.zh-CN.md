# 玩家技能组

角色通过 `CharacterConfig.SkillGroupId` 读取初始技能组。技能组只定义开局持有内容；局内升级将来通过 `PlayerLoadoutSystem` 增加或强化相同内容，不修改角色或技能组资产。

## 配置链

```text
CharacterConfig.SkillGroupId
→ SkillGroupConfig
→ WeaponConfig.InitialAttackIds
→ AttackConfig
```

`SkillGroupConfig` 字段：

- `StartingWeaponIds`：开局武器；当前唯一已执行的类别。
- `StartingSkillIds`：预留主动/被动技能 ID；当前只记录，不执行。
- `StartingDodgeId`：预留闪避 ID；当前只记录，不执行。

当前示例：幸存者的 `SkillGroupId = 1`，`SkillGroup_ProjectileStarter` 给予武器 ID `1`，`Weapon_Projectile` 的初始 Attack 为 `1002`（子弹）。

## 运行时职责

`PlayerLoadoutModel` 保存本局 WeaponRuntime、技能和闪避 ID。每个 WeaponRuntime 独立保存等级、可升级状态和当前 Attack 容器内容；`PlayerLoadoutSystem` 在玩家生成时加载技能组并注册攻击。玩家销毁时会注销对应 Trigger 后重置模型。

升级系统通过 `UpgradeWeaponCommand`、`ReplaceWeaponCommand`、`EvolveWeaponCommand` 或 `ReplaceWeaponAttacksCommand` 修改运行时武器；详情见 [WeaponSystem.zh-CN.md](WeaponSystem.zh-CN.md)。
