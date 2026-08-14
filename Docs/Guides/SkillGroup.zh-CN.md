# 技能组

本文只说明角色开局能力的配置与装配边界；局内成长见 `LevelUp.zh-CN.md`。

`CharacterConfig.SkillGroupId` 指向 `SkillGroupConfig`，后者定义 `StartingWeaponIds`、`StartingSkillIds` 与 `StartingDodgeId`。

## 装配规则

`PlayerLoadoutSystem.EquipInitialSkillGroup()` 按槽位返回结果。`RequireStartingWeapons` 默认是 `true`：必需 Weapon 无法装配会阻止开局。Skill 和 Dodge 是可选能力，失败时记录结果并禁用对应能力，不影响 PlayerRoot 创建。

Weapon 装配后通过 Executor 生成 AttackRuntime；Dodge 装配成功才会注册其固定 Tick。当前 Skill 仅保留 ID 接口，尚无运行逻辑。

局内新增能力必须进入对应成长模块，不能回写角色或技能组 ScriptableObject。
