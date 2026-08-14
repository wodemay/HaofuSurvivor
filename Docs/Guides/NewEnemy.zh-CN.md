# 新增 Enemy

1. 在 `Assets/Resources/Configs/Enemies/` 创建 EnemyConfig。
2. 分配未使用的数字 `Id`。
3. 填写内容 Prefab、基础生命、移动速度、攻击 ID 和经验掉落配置。
4. 将 EnemyConfig 加入 EnemyCatalog。
5. Enemy 内容 Prefab 只负责表现；运行时由 EnemyRoot 承载并进入按 ID 划分的对象池。

时间阶段通过 `RunTimelineConfig` 指定哪些 Enemy ID 可以生成。移动、生命、攻击注册和回收由 EnemySystem、EnemyFactory 与 EnemyHealthSystem 负责。
