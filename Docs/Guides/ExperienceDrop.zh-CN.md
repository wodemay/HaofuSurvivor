# 经验掉落

本文只说明经验配置、掉落和吸附边界；升级选择见 `LevelUp.zh-CN.md`。

## 配置

`EnemyConfig.ExperienceDrop` 直接引用 `ExperienceDropConfig`。该配置定义经验球 Prefab、基础经验和预留的 `DropTableId`；当前不实现掉落表查询。

吸取范围、吸附加速度和最高速度属于 `PlayerStatModel`，经验值和经验倍率使用浮点计算，显示层再格式化为小数点后一位。经验球一旦进入吸取范围即被捕获，之后持续追踪玩家，不会因玩家移动而脱离。

## 运行

敌人死亡发布 `EnemyDiedEvent`，`ExperienceSystem` 通过 `ExperienceFactory` 从对象池创建经验球。ExperienceSystem 只在存在活跃经验球时注册 `IRunUpdateable`；统一帧 Tick 中以玩家属性加速吸附，接触后结算经验并发布 `PlayerLevelUpEvent`。

`ExperienceSystem` 不生成升级候选，也不改变对局阶段。

## 预留接口

`DropTableId` 仅是后续掉落表的扩展字段，不参与当前运行时查询。
