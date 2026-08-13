# 闪避系统

当前闪避是独立于 Weapon、Attack 的技能模块。它使用 ScriptableObject 保存静态参数，使用 QFramework Model 保存局内状态，由 System 执行行为，Command 负责输入和升级请求。

## 配置资产

目录：`Assets/Resources/Configs/Dodge/`

- `Dodge_Basic.asset`：普通闪避，`Id = 1`。当前角色幸存者的 `StartingDodgeId` 为 1。
- `DodgeCatalog.asset`：`DodgeCatalogConfig`，通过 `Dodges` 列表维护所有闪避配置。

`DodgeConfig` 的主要字段：

- `Id`、`DisplayName`、`Description`、`Icon`：识别和显示数据。
- `ExecutorId`：预留执行器标识，当前普通闪避由 `DodgeSystem` 直接执行，尚未拆分多个执行器。
- `Cooldown`：再次使用前的冷却时间。
- `Duration`：冲刺持续时间。
- `Distance`：一次闪避的位移距离。
- `InvulnerabilityDuration`：闪避无敌时间。
- `MaxLevel`、`CanUpgrade`、`LevelUpgrades`：闪避升级边界和逐级数值增量。

## 装备流程

`CharacterConfig.SkillGroupId` 指向 `SkillGroupConfig`。玩家生成成功后，`PlayerLoadoutSystem.EquipInitialSkillGroup()` 依次装备 Weapon、Skill 和 Dodge；`SetDodge()` 调用 `DodgeSystem.Equip()`，按 ID 从 `DodgeCatalog` 取得配置并创建 `DodgeRuntimeData`。

配置缺失时装备失败，玩家生成流程会中止，不会启动半初始化对局。

## 输入与执行链

`PlayerController.Update()` 读取 `Space`，发送 `RequestDodgeCommand`。Command 转发到 `DodgeSystem.TryStart()`，系统检查：对局必须处于 Active、玩家已注册且存活、闪避不在持续或冷却状态。

系统使用当前移动输入；没有输入时依次使用最近移动方向，最后使用右方向。成功后记录方向、持续时间、冷却时间和玩家闪避无敌时间，并发布 `DodgeStartedEvent`。

## 时间推进与移动

`GameStart.FixedUpdate()` 发送 `TickDodgeCommand`。`DodgeSystem.AdvanceFixed()` 只消费 `RunTimerModel.FixedDeltaTime`：

1. 冷却递减。
2. 闪避期间按固定速度沿记录方向移动玩家。
3. 持续时间结束后清除激活状态并发布 `DodgeEndedEvent`。

普通移动由 `PlayerSystem.Move()` 在闪避激活期间跳过，因此不会叠加两种位移。暂停、选项暂停、退出和重开均通过 `DodgeSystem.Reset()` 清除运行时状态。

## 伤害与无敌

`PlayerSystem.ApplyDamage()` 同时检查普通受伤无敌和闪避无敌。`AdvanceDamageInvulnerability()` 使用统一游戏时间递减两种计时器。普通闪避的无敌时间来自 `DodgeConfig.InvulnerabilityDuration`，当前为 0.15 秒；玩家属性中的普通受伤无敌默认仍为 0。

## 升级与存档

`LevelUpSystem.GetLevelUpWeaponOptionsQuery()` 会把可升级闪避加入局内升级选项。选择后由 `CompleteLevelUpWeaponCommand` 以 `isDodge = true` 调用 `DodgeSystem.Upgrade()`，只提升运行时等级，不修改资产。

`RunSaveSystem` 保存 `DodgeId` 和 `DodgeLevel`；恢复时先重新装备闪避，再恢复等级。闪避的具体升级效果由 `LevelUpUpgrades` 按目标等级读取，未配置的等级不会产生增量。

## 扩展边界

后续角色专属闪避只需新增 `DodgeConfig`、加入 `DodgeCatalog`，并在执行层按 `ExecutorId` 注册独立行为；升级效果继续写入 `DodgeLevelUpgrade`。路径伤害、残留火焰、特殊位移等逻辑不应写入 `PlayerController`，应拆为闪避执行器或独立 System，并继续通过 Command、Event 和统一游戏时钟接入。
