# 技能组

本文只说明角色开局能力的配置与装配边界；局内成长见 `LevelUp.zh-CN.md`。

`CharacterConfig.SkillGroupId` 指向 `SkillGroupConfig`，后者定义 `StartingWeaponIds`、`StartingSkillIds` 与 `StartingDodgeId`。

## 装配规则

`PlayerLoadoutSystem.EquipInitialSkillGroup()` 按槽位返回结果。`RequireStartingWeapons` 默认是 `true`：必需 Weapon 无法装配会阻止开局。Skill 和 Dodge 是可选能力，失败时记录结果并禁用对应能力，不影响 PlayerRoot 创建。

Weapon 和 Skill 装配后都会通过 Executor 生成 AttackRuntime；Dodge 装配成功才会注册其固定 Tick。Skill 是 `SkillConfig -> SkillRuntimeData -> Attack` 的独立能力，不属于 Weapon 容器。幸存者环形弹幕的配置和运行规则见 `SurvivorBarrageSkill.zh-CN.md`。

局内新增能力必须进入对应成长模块，不能回写角色或技能组 ScriptableObject。
