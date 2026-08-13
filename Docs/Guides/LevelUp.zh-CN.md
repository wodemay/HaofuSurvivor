# 升级选择

`LevelUpSystem` 独立于 `ExperienceSystem`。经验系统只发布 `PlayerLevelUpEvent`，升级系统负责排队、生成候选、暂停对局和执行选择。

当前候选只包含可升级 Weapon。显示文本使用 `LevelX->LevelX+1`；满足进化表时显示 `LevelX->Evolve`。

确认后由 `PlayerLoadoutSystem` 执行升级或进化。所有修改只写入 `WeaponRuntimeData`，不修改 ScriptableObject。没有待处理等级时恢复 `RunPhase.Active`。
