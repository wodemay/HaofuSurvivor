# 升级选择

本文只说明升级候选、确认和阶段控制；经验来源见 `ExperienceDrop.zh-CN.md`，Weapon 数值规则见 `WeaponUpgrade.zh-CN.md`。

`ExperienceSystem` 只发布 `PlayerLevelUpEvent`。`LevelUpSystem` 独立维护待选择队列、生成候选，并在有候选时把对局切换为 `RunPhase.LevelUpSelection`；统一 Tick 因此停止，确认后恢复 Active。

## 当前候选

候选包含可升级 Weapon、满足条件的 Weapon 进化，以及可升级 Dodge。普通升级文本为 `LevelX->LevelX+1`；进化为 `LevelX->Evolve`。

## 确认规则

UI 发送确认 Command 后，`PlayerLoadoutSystem` 升级或进化 Weapon，或 `DodgeSystem` 升级闪避。所有修改只写入运行时数据，绝不修改 ScriptableObject。没有待选项时恢复 Active。

新 Weapon、Skill 候选、权重、刷新、跳过和稀有度仍未实现。
