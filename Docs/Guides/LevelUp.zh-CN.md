# 升级选择

本文只说明升级候选、确认和阶段控制；经验来源见 `ExperienceDrop.zh-CN.md`，Weapon 数值规则见 `WeaponUpgrade.zh-CN.md`。

`ExperienceSystem` 只发布 `PlayerLevelUpEvent`。`LevelUpSystem` 独立维护待选择队列、生成候选，并在有候选时把对局切换为 `RunPhase.LevelUpSelection`；统一 Tick 因此停止，确认后恢复 Active。

## 当前候选

候选包含可升级 Weapon、满足条件的 Weapon 进化，以及可升级 Dodge。普通升级文本为 `LevelX->LevelX+1`；进化为 `LevelX->Evolve`。

## 确认规则

UI 发送确认 Command 后，`PlayerLoadoutSystem` 升级或进化 Weapon，或 `DodgeSystem` 升级闪避。所有修改只写入运行时数据，绝不修改 ScriptableObject。没有待选项时恢复 Active。

新 Weapon、通用属性、Weapon 组合和专属 Skill 已接入候选生成；刷新、跳过、锁定和稀有度仍未实现。

## 专属 Skill 候选

`CharacterExclusiveSkillUpgradeSystem` 检查角色初始 SkillGroup 中指定的专属 Weapon 与 Dodge。两者满级后，或被配置中的最终形态替代后，下一次升级把该专属 Skill 的一次性升级固定放入第一个候选位。Skill 未升级前，玩家未选择该候选不会丢失资格。
