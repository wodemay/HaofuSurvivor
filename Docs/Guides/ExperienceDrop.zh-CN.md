# 经验掉落

## 配置

`EnemyConfig.ExperienceDrop` 直接引用经验掉落配置。`ExperienceDropConfig` 定义经验球 Prefab、基础经验和预留的 `DropTableId`；当前不实现掉落表查询。

玩家吸取范围属于 `PlayerStatModel.ExperienceAbsorbRadius`，不是经验球属性。吸附后经验球永久追踪玩家，速度由距离、加速度和最高速度共同决定。

## 运行链路

敌人死亡发送 `EnemyDiedEvent`，`ExperienceSystem` 通过 `ExperienceFactory` 从对象池创建经验球。经验球进入吸取范围后被捕获，随后使用统一游戏时间加速移动并在接触玩家时结算经验。

`ExperienceSystem` 只负责经验和等级阈值计算；升级候选和暂停由独立的 `LevelUpSystem` 处理。
