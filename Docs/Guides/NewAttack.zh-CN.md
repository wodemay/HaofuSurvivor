# 新增 Attack

## Unity 资源

1. 在 `Assets/Resources/Configs/Combat/` 创建 `Attack_<技能名称>.asset`。
2. 分配未使用的数字 `Id`。
3. 填写基础伤害、冷却和参数资源。
4. 将配置加入 `AttackCatalog.asset`。

Attack 配置不写角色、敌人或用途名称，因为同一攻击可被不同拥有者复用。

## 代码接入

在 `Assets/Scripts/Architecture/Combat/` 或 `Assets/Scripts/Game/` 创建 Executor/Trigger。Executor 实现 `IAttackExecutor`，使用唯一 `ExecutorId` 注册到 `AttackExecutorRegistry`；Trigger 只负责触发条件和运行时注册。

不要添加 AttackType 枚举、目标阵营字段或集中式攻击分支。攻击目标从运行时拥有者阵营推导。

## 验证

检查 AttackCatalog 引用、Executor 注册、Trigger 生命周期、对象池回收和 `dotnet build Assembly-CSharp.csproj --no-restore --disable-build-servers`。
