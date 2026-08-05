# 玩家技能组

角色通过 `CharacterConfig.SkillGroupId` 读取初始技能组。技能组只定义开局持有内容；局内升级将来通过 `PlayerLoadoutSystem` 增加或强化相同内容，不修改角色或技能组资产。

## 配置链

```text
CharacterConfig.SkillGroupId
→ SkillGroupConfig
→ WeaponConfig.AttackIds
→ AttackConfig
```

`SkillGroupConfig` 字段：

- `StartingWeaponIds`：开局武器；当前唯一已执行的类别。
- `StartingSkillIds`：预留主动/被动技能 ID；当前只记录，不执行。
- `StartingDodgeId`：预留闪避 ID；当前只记录，不执行。

当前示例：幸存者的 `SkillGroupId = 1`，`SkillGroup_ProjectileStarter` 给予武器 ID `1`，`Weapon_Projectile` 给予攻击 ID `1002`（子弹）。

## 运行时职责

`PlayerLoadoutModel` 保存本局持有的武器、技能和闪避 ID。`PlayerLoadoutSystem` 在玩家生成时加载技能组，并为每个武器的 AttackId 调用既有攻击执行器。玩家销毁时模型重置。

未来升级系统调用 `PlayerLoadoutSystem.EquipWeapon`、`AddSkill`、`SetDodge`；升级逻辑不得直接修改 ScriptableObject。
