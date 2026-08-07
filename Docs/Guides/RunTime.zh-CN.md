# 局内统一时间

`RunTimerSystem` 是局内唯一的逻辑时间源。业务代码不得自行使用 `Time.deltaTime`、`Time.fixedDeltaTime` 或直接修改 `Time.timeScale`。

## 时间推进

`GameStart` 每帧发送 `TickRunTimerCommand(Time.unscaledDeltaTime)`，每个物理帧发送 `TickRunPhysicsCommand(Time.fixedUnscaledDeltaTime)`。

- 普通逻辑从 `RunTimerModel.DeltaTime` 读取帧时间，例如攻击冷却、投射物寿命、无敌帧。
- 物理移动从 `RunTimerModel.FixedDeltaTime` 读取物理时间，例如玩家、敌人和投射物的 `Rigidbody2D.MovePosition`。
- Controller 读取时间必须使用 `GetRunTimeStateQuery`；System 可读取 `RunTimerModel`。

## 暂停

暂停入口是 `PauseRunCommand`，恢复入口是 `ResumeRunCommand`。它们通过 `RunSystem` 改变 `RunPhase`，再由 `RunTimerSystem` 将逻辑增量清零并同步 Unity 的 `Time.timeScale`。

暂停期间 `RunTimerSystem.IsRunning()` 为 `false`。攻击执行、冷却、伤害无敌帧、敌人刷新/移动、玩家移动和投射物移动/寿命均不得推进。未来暂停 UI 只负责发送这两个 Command，不能自行冻结业务模块。
