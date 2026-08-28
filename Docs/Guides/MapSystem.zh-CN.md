# 地图系统

本文说明当前已实现的无限区块地图、障碍生成和导航链路；道具、地图事件和世界箭头仍是独立的后续模块。

## 配置

`Resources/Configs/Map/MapGrid.asset` 提供区块 Prefab、底图 Tile、主题、生成器版本、区块尺寸、加载/卸载半径、初始同步半径和每帧区块操作上限。当前区块尺寸为 `32×32`。

`MapThemeConfig` 定义主题底图、装饰 Tile 和障碍模板。`MapObstacleTemplateConfig` 定义占用格形状、权重、最小间距、是否允许四向旋转/镜像、是否阻挡移动和是否阻挡投射物。障碍物不可破坏、不可交互。

## 生成与加载

新对局由系统内部创建隐藏 `WorldSeed`；Seed、主题 ID 和生成器版本共同决定每个区块的确定性结果，不依赖 `UnityEngine.Random`。区块先铺底图，再按模板权重生成障碍候选；候选与已有障碍重叠或违反间距时拒绝，并以 Flood Fill 检查所有可行走格仍在同一连通区域。

玩家周围的 3×3 区块按距离优先加载。初始半径同步加载，外围区块按 `MaxChunkOperationsPerTick` 分帧生成；超出卸载半径的表现区块回收到 `MapChunkFactory`，逻辑 `MapChunkData` 仍由 `WorldMapModel` 保留，因此重新靠近时不会改变已生成布局。

## 碰撞与导航

障碍 Tilemap 通过 `MapColliderUtility` 配置碰撞。玩家、敌人和投射物都受移动/投射物阻挡规则约束；玩家和敌人使用 Rigidbody2D Cast 处理接触，并保留沿障碍切线的滑动分量，闪避也走同一碰撞路径。

`MapNavMeshSystem` 使用 NavMeshPlus 2D 组件异步重建活动区块的 NavMesh。区块加载、卸载或障碍变化只标记脏状态并批处理重建；敌人按需计算连续 A* 路径并缓存拐点，实际位移仍由 `EnemySystem` 的 Rigidbody2D 物理 Tick 完成。

## 表现层

`WorldRootLocator` 将动态对象挂到场景 WorldRoot 的九个角色层：`MapBackground`、`MapDecoration`、`GroundEffect`、`Pickup`、`Enemy`、`Player`、`Projectile`、`CombatEffect`、`WorldUI`。地图背景和装饰不会覆盖角色；Inferno FireBall 的地面火焰使用 `GroundEffect`。
