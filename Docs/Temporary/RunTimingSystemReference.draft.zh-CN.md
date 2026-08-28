# ProjectSurvivor 对局时间系统参考

> 临时交接文档。描述当前已实现的运行时契约；不提交 GitHub、不进入 Docs 网页阅读器。

## 1. 一句话约束

所有会随**对局时间**变化的游戏逻辑，必须由 `GameLoopSystem` 分发的 `IRunUpdateable` 或 `IRunFixedUpdateable` 驱动；业务 System 和运行时 Prefab 不得自行使用 `Update`、`FixedUpdate`、`Time.deltaTime`、`Time.fixedDeltaTime`、协程等待或 `Invoke` 推进对局状态。

这样暂停、升级选择、失败和结算只需改变对局阶段，就能统一停止所有已接入的游戏时间逻辑。

## 2. 调用链

```text
Unity PlayerLoop
  GameStart.Update()
    -> TickGameLoopCommand(Time.deltaTime)
    -> GameLoopSystem.TickFrame(...)
    -> RunTimerSystem.Advance(...)
    -> IRunUpdateable.OnRunUpdate(RunTimerModel.DeltaTime)

  GameStart.FixedUpdate()
    -> TickGamePhysicsCommand(Time.fixedDeltaTime)
    -> GameLoopSystem.TickFixed(...)
    -> RunTimerSystem.AdvanceFixed(...)
    -> IRunFixedUpdateable.OnRunFixedUpdate(RunTimerModel.FixedDeltaTime)
```

唯一的 Unity `Update` / `FixedUpdate` 宿主是 `GameStart`。它只负责把 Unity 的时间值送进 Command，不包含任何玩法推进逻辑。

## 3. 核心职责与脚本位置

| 脚本 | 职责 |
| --- | --- |
| `Assets/Scripts/Game/Bootstrap/GameStart.cs` | 唯一的 Unity 帧循环入口。|
| `Assets/Scripts/Architecture/Commands/GameplayCommands.cs` | `TickGameLoopCommand`、`TickGamePhysicsCommand` 把时间输入交给系统。|
| `Assets/Scripts/Architecture/Run/GameLoopSystem.cs` | 保存 Update / Fixed 两类订阅者、在正确阶段分发 tick、处理 tick 中的安全增删订阅。|
| `Assets/Scripts/Architecture/Run/RunTimerSystem.cs` | 唯一的对局时间源；推进秒数、控制暂停、限制单帧最大游戏时间、触发时间线阶段。|
| `Assets/Scripts/Architecture/Run/RunTimerModel.cs` | 保存当前对局秒数、当帧 `DeltaTime`、物理帧 `FixedDeltaTime`、阶段倍率。|
| `Assets/Scripts/Architecture/Run/RunModel.cs` | 保存 `RunPhase`，决定对局是否处于可运行阶段。|
| `Assets/Scripts/Architecture/Run/RunSystem.cs` | 开局、暂停、恢复、升级选择、结束与继续游戏时切换 `RunPhase` 并调用计时器。|
| `Assets/Scripts/Architecture/Run/GetRunTimeStateQuery.cs` | UI 或只读逻辑读取时间、固定步长和运行状态的唯一 Query。|
| `Assets/Resources/Configs/Run/RunTimeline.asset` | 时间线配置；由 `RunTimerSystem` 依次应用敌人生命、伤害、速度、生成倍率。|

## 4. 对局阶段与时间行为

| `RunPhase` | `RunTimerSystem.IsRunning()` | 游戏时间与玩法 tick |
| --- | --- | --- |
| `None` | 否 | 不推进、不分发。|
| `Active` | 是 | 正常推进并分发。|
| `Paused` | 否 | `DeltaTime` / `FixedDeltaTime` 为 0，不分发，`Time.timeScale = 0`。|
| `LevelUpSelection` | 否 | 与暂停相同；背景玩法完全冻结。|
| `Defeat` | 否 | 停止。|
| `Victory` | 否 | 停止。|

`RunTimerSystem` 额外将每次帧时间限制为最多 `0.05` 秒。窗口失焦、恢复焦点或暂停切换后，不能把长时间的墙上时间一次性补算给敌人移动、自然回血、投射物等逻辑。

## 5. 时间数据规则

```csharp
RunTimerModel.ElapsedSeconds   // 对局累计秒数，仅 Active 时增加
RunTimerModel.DeltaTime       // 当前游戏帧步长，仅 IRunUpdateable 使用
RunTimerModel.FixedDeltaTime  // 当前游戏物理步长，仅 IRunFixedUpdateable 使用
```

- UI 显示累计时间：使用 `GetRunTimeStateQuery` 或订阅 `RunTimerUpdatedEvent`。
- 普通连续逻辑：使用 `OnRunUpdate(float deltaTime)`。
- Rigidbody2D 移动、物理 Cast、接触判定：使用 `OnRunFixedUpdate(float deltaTime)`。
- 不要在玩法代码中重新读取 Unity 的 `Time.*`；否则该逻辑会绕过暂停和单帧上限。

## 6. 如何接入一个新系统

### 6.1 只有帧时间逻辑

```csharp
public class ExampleSystem : AbstractSystem, IRunUpdateable
{
	public void OnRunUpdate(float deltaTime)
	{
		// Advance gameplay state with deltaTime.
	}

	protected override void OnInit()
	{
	}
}
```

在 `GameArchitecture.Init()` 注册 System。若它在整局都需要运行，在 `GameLoopSystem.BeginRun()` 中注册：

```csharp
RegisterUpdateable(this.GetSystem<ExampleSystem>());
```

### 6.2 只有物理时间逻辑

```csharp
public class ExampleSystem : AbstractSystem, IRunFixedUpdateable
{
	public void OnRunFixedUpdate(float deltaTime)
	{
		// Move Rigidbody2D or process physics state with deltaTime.
	}

	protected override void OnInit()
	{
	}
}
```

在 `BeginRun()` 中使用 `RegisterFixedUpdateable(...)`。

### 6.3 仅在存在运行时对象时工作

投射物、经验球、弹幕、地面火焰等应按需注册：

1. 第一个运行时对象出现时，调用 `GameLoopSystem.RegisterUpdateable` 或 `RegisterFixedUpdateable`。
2. 最后一个对象回收时，调用对应 `Unregister...`。
3. 对局结束、对象池回收和场景切换路径必须重复执行注销或由 `GameLoopSystem.EndRun()` 统一清空。

不要在 tick 遍历期间直接修改订阅列表。`GameLoopSystem` 已经延迟处理注册、注销和清空；必须继续使用其公开 API。

## 7. 当前注册关系

`GameLoopSystem.BeginRun()` 固定注册：

- 帧：`InputSystem`、`PlayerSystem`、`RunSaveSystem`、`MapSystem`、`MapNavMeshSystem`。
- 固定步：`PlayerSystem`、`EnemySystem`；装备闪避后追加 `DodgeSystem`。
- 攻击运行时已存在时追加 `AttackSystem`。

以下系统按对象/状态动态注册：

- `ProjectileSystem`：有活动投射物时注册帧与固定步。
- `ExperienceSystem`：有经验球时注册帧。
- `BarrageProjectileSystem`：有持续弹幕时注册帧。
- `ExplosiveAreaSystem`：有地面火焰或定时特效时注册帧。
- `PlayerRegenerationSystem`：玩家拥有自然回血属性时注册帧。
- `CharacterExclusivePerkSystem`：专属效果需要倒计时时注册帧。

## 8. 暂停与恢复

暂停入口必须使用 `PauseRunCommand` / `RunSystem.Pause()`，恢复必须使用 `ResumeRunCommand` / `RunSystem.Resume()`。

`RunSystem.Pause()` 依次：

1. 将阶段设为 `Paused`。
2. 调用 `RunTimerSystem.Pause()`，置零两种步长并设定 `Time.timeScale = 0`。
3. 清空输入。
4. 保存当前对局。
5. 发送 `RunPausedEvent`。

升级选择通过 `BeginLevelUpSelection()` 将阶段设为 `LevelUpSelection` 并暂停计时；选项确定后 `EndLevelUpSelection()` 恢复 `Active` 与计时。

即使 Unity 因边界时机仍触发了一次 `FixedUpdate`，`GameLoopSystem.TickFixed()` 也会先经过 `RunTimerSystem.IsRunning()`，因此不会向游戏 System 分发任何非 Active 的物理 tick。

## 9. 时间线

`RunTimerSystem.Advance()` 每次推进后读取 `RunTimeline.asset`。当累计时间跨过下一个配置节点时：

1. 更新 `CurrentStageIndex`。
2. 写入敌人生命、敌人伤害、敌人移动速度、刷怪频率倍率。
3. 发送 `RunTimelineStageReachedEvent`。

新刷怪、事件或阶段规则应订阅该 Event 或读取 `RunTimerModel`，而不是自行维护另一套计时器。继续游戏时，时间线根据已保存的 `ElapsedSeconds` 从头重放到正确阶段，以重建倍率。

## 10. 禁止项与排查清单

- 禁止在 `MonoBehaviour.Update` / `FixedUpdate` 推进玩法。
- 禁止在 Prefab 上挂独立的游戏循环脚本。
- 禁止用 `Time.unscaledDeltaTime`、`Time.fixedUnscaledDeltaTime` 推进对局玩法；它们会在暂停时继续流逝。
- 禁止用协程 `WaitForSecondsRealtime`、`Invoke` 或单独计时字段绕开 `RunTimerSystem`。
- 新增运行时状态时，必须设计开局重置、暂停、结束、对象池回收、存档恢复五条路径。
- UI 动画、菜单过渡等非对局表现可使用 Unity 自己的时间，但不得改变游戏数值或位置。

## 11. 常见问题

**暂停后恢复，敌人或投射物突然跳动**：检查是否误用了 `unscaled` 时间、直接调用了 Update，或未经过 `GameLoopSystem` 注册。

**新模块在暂停时仍运行**：检查它是否使用了协程、`Invoke`、动画事件或独立 MonoBehaviour 循环；全部改为 `IRunUpdateable` / `IRunFixedUpdateable`。

**新模块完全不运行**：确认它已在 `GameArchitecture` 注册，并在开局或第一个运行时对象出现时注册到 `GameLoopSystem`。

**对局结束后旧逻辑还执行**：确认 `RunSystem.ReleaseRunRuntime()` 的重置路径与对象池回收路径已注销运行时订阅；`EndRun()` 会清空 GameLoop 当前订阅，但模块自己的活动数据也必须重置。
