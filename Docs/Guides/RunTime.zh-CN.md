# 统一运行时间

`RunTimerSystem` 是所有局内时间的唯一来源。

## 输出

- `DeltaTime`：逻辑帧增量。
- `FixedDeltaTime`：物理帧增量。
- `ElapsedSeconds`：对局累计时间。
- 当前阶段倍率：敌人生命、伤害、移动速度和生成速率。

## 使用规则

玩家、敌人、投射物、攻击冷却、经验吸附和无敌帧只能读取 `RunTimerModel` 的增量。禁止在业务逻辑中直接使用 `Time.deltaTime` 或 `Time.fixedUnscaledDeltaTime`。

暂停和升级选择会让两个逻辑增量归零，并同步 `Time.timeScale = 0`。恢复后重新输出增量。
