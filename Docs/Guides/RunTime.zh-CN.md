# 统一运行时间与 Tick 调度

本文只说明局内时间来源和调度规则；各玩法如何消费 Tick 由所属模块文档说明。

## 完整链路

```text
GameStart.Update/FixedUpdate
 -> TickGameLoopCommand/TickGamePhysicsCommand
 -> GameLoopSystem
 -> RunTimerSystem
 -> IRunUpdateable / IRunFixedUpdateable
```

`GameStart` 是唯一的业务 `Update`、`FixedUpdate` 宿主。`RunTimerSystem` 是唯一的玩法时间源，产生 `DeltaTime`、`FixedDeltaTime`、`ElapsedSeconds` 和时间线阶段倍率。业务代码禁止使用 Unity 的 `Time.fixedUnscaledDeltaTime`、协程或独立业务 `MonoBehaviour.Update` 推进玩法。

## 阶段与暂停

`RunPhase.Active` 才会推进逻辑时间并分发 Tick；`Paused`、`LevelUpSelection`、`Defeat`、`Victory` 不分发玩法 Tick。暂停和升级选择会把逻辑增量设为零并设置 `Time.timeScale = 0`，恢复后不会补算暂停期间的移动、冷却、投射物寿命或经验吸附。

`RunTimerSystem` 对单帧逻辑时间设置 0.05 秒上限，避免切后台或恢复焦点时出现长帧追赶。`Stop`、重开、返回菜单和结算都会清零时间增量并结束 Tick 注册。

## 动态注册

需要帧级逻辑的 System 实现 `IRunUpdateable`，需要物理步进的 System 实现 `IRunFixedUpdateable`，通过 `GameLoopSystem.RegisterUpdateable/RegisterFixedUpdateable` 注册，并在对象失效或对局结束时注销。当前注册者包括输入、玩家、敌人、攻击、地图、NavMesh、经验、存档、投射物、闪避和区域效果系统。
