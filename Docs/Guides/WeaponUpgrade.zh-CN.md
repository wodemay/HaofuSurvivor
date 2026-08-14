# Weapon 升级

`WeaponConfig.LevelUpgrades` 为每个等级定义：显示文本、增加/移除的 Attack，以及按 Attack ID 记录的属性修正。

升级时：

1. `LevelUpSystem` 生成候选。
2. UI 发送确认 Command。
3. `PlayerLoadoutSystem` 修改 WeaponRuntime 的 Attack 列表和修正。
4. 旧 Trigger 按 WeaponRuntimeId 清理后重新配置。

进化由 `WeaponEvolutionCatalog` 查找目标。目标必须 `MaxLevel = 1` 且 `CanUpgrade = false`，进化后不可再次升级。
