# 新增 Enemy

本文只说明敌人配置和内容 Prefab 接入；具体攻击接入见 `NewAttack.zh-CN.md`。

## 配置资产

1. 在 `Resources/Configs/Enemy/` 创建 EnemyConfig。
2. 分配未使用的数字 `Id`，填写内容 Prefab、生命、移动速度、Attack IDs 与经验掉落配置。
3. 将 EnemyConfig 加入 EnemyCatalog。
4. 在 RunTimelineConfig 的目标时间阶段填入该 Enemy ID，才会进入生成候选。

内容 Prefab 只负责表现。运行时由 EnemyRoot 承载 CombatEntity、Rigidbody2D、攻击桥接和对象池状态。

## 运行约束

`EnemySystem` 作为 `IRunFixedUpdateable`，由 `GameLoopSystem` 在 Active 期间统一执行生成和追击；`EnemyFactory` 按 ID 复用 EnemyRoot，`EnemyHealthSystem` 负责生命与死亡回收。不要在敌人内容 Prefab 新增独立 Update/FixedUpdate。
