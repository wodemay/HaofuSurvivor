# 升级系统

## 职责边界

`LevelUpSystem` 独立处理升级选择流程，不参与经验球生成、吸取或经验数值结算。

```text
PlayerLevelUpEvent
-> LevelUpModel 队列
-> LevelUpSelection
-> 升级候选
-> 确认选择
-> 恢复对局
```

## 运行时状态

`LevelUpModel` 保存待选择的角色等级队列：

- `CurrentLevel`：当前正在选择的升级等级。
- `PendingSelectionCount`：尚未完成的升级选择次数。

每局开始和退出角色选择时都调用 `LevelUpSystem.Reset` 清空队列，不能将上一局的升级选择带入下一局。

## 当前候选项

当前只实现武器升级候选：`GetLevelUpWeaponOptionsQuery` 从 `PlayerLoadoutModel` 获取仍可升级的武器，再由 `WeaponCatalog` 提供名称、描述与图标。

`PlayerLoadoutSystem` 只负责验证并执行 `UpgradeWeapon`，不保存升级队列、不控制暂停、也不直接打开 UI。

后续新增技能、闪避、属性或新武器候选时，在 `LevelUpSystem` 扩展候选生成与确认分发；不要改动 `ExperienceSystem`。

## UI 与对局

`LevelUpSelectionRequestedEvent` 是 UI 入口。`UILevelUpPanel` 通过 `GetLevelUpStateQuery` 读取当前等级和待选择数量，通过 `GetLevelUpWeaponOptionsQuery` 展示选项，点击后发送 `CompleteLevelUpWeaponCommand`。

`LevelUpSystem` 在存在可选项时调用 `RunSystem.BeginLevelUpSelection`，由统一游戏时钟冻结对局；所有选择完成后调用 `RunSystem.EndLevelUpSelection` 恢复对局。
