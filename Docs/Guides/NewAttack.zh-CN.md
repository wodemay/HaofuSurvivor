# 新增 Attack

本文是新 Attack 的资源、代码和验证清单；运行职责见 `AttackFlow.zh-CN.md`。

## 配置资产

1. 在 `Resources/Configs/Combat/Attack/` 创建 `Attack_<技能名称>.asset`。
2. 分配未使用的数字 `Id`，填写伤害、冷却和参数资产。
3. 使用唯一 `ExecutorId`，并把资产加入 `AttackCatalog.asset`。
4. 将 ID 配给 Enemy 的 `AttackIds`，或作为 Weapon 的 Attack 内容。

Attack 不写角色、敌人、用途或目标阵营；同一配置可由不同拥有者复用。

## 代码接入

在 `Architecture/Combat/` 实现 `IAttackExecutor` 并注册到 `AttackExecutorRegistry`：

- 碰撞型攻击：Executor 在 `ConfigureOwner()` 创建或复用 `CollisionAttackTrigger` 一类的感知桥接，并由该桥接发送注册和执行 Command。
- 自动攻击：Executor 同时实现 `IAutomaticAttackExecutor`。在 `FindTarget()` 返回目标；`AttackSystem` 会按冷却在统一帧 Tick 中调用它，禁止为此新增 Update Trigger。

需要投射物时复用 `ProjectileFactory` 与 `ProjectileSystem`，不要自行创建逐实例 Update。

## 验证

核对 Catalog 引用、Executor 注册、拥有者卸载/对象池回收时的注销、自动攻击 Tick 注册与 `dotnet build Assembly-CSharp.csproj --no-restore --disable-build-servers`。不改 UI、Prefab 或场景层级。
