# 新攻击接入指南

本文说明如何在当前项目中接入一项攻击。攻击对玩家和 Enemy 通用：触发来源不同，但都通过 `AttackSystem`、`AttackConfig` 和 `IAttackExecutor` 执行。不要恢复 `AttackType` 枚举，也不要在 `AttackSystem` 中增加攻击类型分支。

## 先理解现有文件

| 文件或目录 | 用途 |
| --- | --- |
| `Assets/Resources/Configs/Combat/AttackCatalog.asset` | 全部攻击配置的总列表；新攻击必须加进这里。 |
| `Assets/Resources/Configs/Combat/Attack_Contact.asset` | 当前碰撞攻击示例，ID 为 1001。 |
| `Assets/Scripts/Architecture/Combat/AttackConfig.cs` | 所有攻击共用的基础配置字段。 |
| `Assets/Scripts/Architecture/Combat/AttackExecutorRegistry.cs` | 注册 `ExecutorId` 与执行器的唯一位置。 |
| `Assets/Scripts/Architecture/Combat/AttackSystem.cs` | 统一处理攻击注册、冷却、目标阵营校验和执行。通常不需要修改。 |
| `Assets/Scripts/Game/ContactAttackTrigger.cs` | 碰撞攻击的 Unity 触发桥接示例。 |
| `Assets/Scripts/Architecture/Enemy/EnemyConfig.cs` | Enemy 的 `AttackIds` 列表。 |

当前 `ProjectSurvivor/Configuration Creator` 只创建 Character 和 Enemy，不创建 AttackConfig；攻击资产需要按以下步骤在 Unity Editor 中创建。

## 创建纯配置攻击

适用于复用已有执行器的攻击。例如不同数值的碰撞攻击不需要写新代码。

### 1. 在 Editor 创建攻击资产

1. 在 Project 窗口进入 `Assets/Resources/Configs/Combat/`。
2. 右键空白处，选择 `Create → ProjectSurvivor → Combat → Attack Config`。
3. 命名为 `Attack_<技能名称>`，例如 `Attack_Slash`、`Attack_Fireball`、`Attack_Contact`。攻击配置不绑定角色或 Enemy，名称只表达技能本身。
4. 选中新资产，在 Inspector 填写：

| 字段 | 填写规则 |
| --- | --- |
| `Id` | 全局唯一正整数。当前 1001 已被普通碰撞攻击使用。 |
| `Executor Id` | 已注册执行器的名称。当前只有 `contact`。 |
| `Target Faction` | 攻击可命中的阵营；Enemy 攻击玩家填 `Player`。 |
| `Damage` | 单次基础伤害。Enemy 会自动乘时间轴敌人伤害倍率。 |
| `Cooldown` | 同一个攻击运行时两次成功释放的最短间隔，建议大于 0。 |

5. 选中 `Assets/Resources/Configs/Combat/AttackCatalog.asset`。
6. 在 Inspector 的 `Attacks` 列表点击 `+`，将刚创建的 `Attack_<技能名称>.asset` 拖入新元素。

没有加入 `AttackCatalog.asset` 的攻击 ID 无法运行。

### 2. 赋给 Enemy

1. 打开 `Assets/Resources/Configs/Enemies/` 下目标 EnemyConfig。
2. 在 `Attack Ids` 列表点击 `+`。
3. 填入攻击的 `Id`，例如 `1002`。
4. 保存资产并进入 Play Mode 验证。

EnemyFactory 会读取这个 ID，找到 AttackConfig 的 ExecutorId，并让该执行器为 EnemyRoot 配置所需的触发器。

## 创建全新行为攻击

适用于子弹、冲撞、范围爆炸、激光、召唤等尚无执行器的攻击。以下以 `projectile` 为示例。

### 1. 新建执行器代码

在 `Assets/Scripts/Architecture/Combat/` 新建 `ProjectileAttackExecutor.cs`，实现 `IAttackExecutor`：

```csharp
public class ProjectileAttackExecutor : IAttackExecutor
{
	public string Id => "projectile";

	public void ConfigureOwner(GameObject owner, AttackConfig config, CombatFaction ownerFaction)
	{
		// Add or configure the projectile cast trigger.
	}

	public void Execute(AttackExecutionContext context, DamageSystem damageSystem)
	{
		// Spawn/reuse projectile and pass context data to it.
	}
}
```

要求：

- `Id` 必须与 AttackConfig 的 `Executor Id` 完全一致。
- `ConfigureOwner` 只负责给攻击者装配 Unity 桥接组件或发射点。
- `Execute` 只负责行为效果；命中结算必须走 `DamageSystem`，不能直接写 PlayerModel 或未来的 EnemyModel。
- 执行器自身不要保存某个 Enemy 或玩家的专属状态；运行时状态应在 AttackSystem、投射物对象或独立运行时组件中。

### 2. 注册执行器

打开 `Assets/Scripts/Architecture/Combat/AttackExecutorRegistry.cs`，在构造函数中增加：

```csharp
Register(new ProjectileAttackExecutor());
```

这是唯一需要登记 ExecutorId 的位置。不要修改 `AttackSystem` 添加 `if (ExecutorId == ...)`。

### 3. 创建触发器

执行器不会决定“什么时候释放”。为新行为创建相应触发器：

| 使用场景 | 推荐脚本位置 | 触发条件 |
| --- | --- | --- |
| Enemy 远程攻击 | `Assets/Scripts/Game/DistanceAttackTrigger.cs` | 玩家进入射程且 AI 允许攻击。 |
| 玩家主动攻击 | `Assets/Scripts/Game/PlayerAttackTrigger.cs` | 输入、自动施法或技能系统请求。 |
| Boss 特殊攻击 | `Assets/Scripts/Game/BossAttackTrigger.cs` | 时间轴、血量阈值或行为状态。 |

触发器最终只发送：

```csharp
this.SendCommand(new TryExecuteAttackCommand(runtimeId, targetFaction));
```

攻击 ID 的注册仍通过：

```csharp
this.SendCommand(new RegisterAttackCommand(runtimeId, attackId, ownerFaction));
```

对象池停用、攻击者死亡或销毁时必须发送 `UnregisterAttackCommand`，避免残留冷却运行时。

### 4. 处理专属参数

当前 `AttackConfig` 只有通用字段：ID、ExecutorId、目标阵营、伤害和冷却。子弹速度、预制体、生命周期、范围、数量等不能硬塞进它。

首次新增有专属参数的执行器时：

1. 在 `Assets/Scripts/Architecture/Combat/` 新建该执行器的参数 ScriptableObject，例如 `ProjectileAttackParameterConfig.cs`。
2. 在 `AttackConfig.cs` 增加一个通用 `ScriptableObject` 参数引用字段，例如 `ExecutorParameterConfig`。
3. 在 `Assets/Resources/Configs/Combat/` 创建 `ProjectileAttackParameter_<技能名称>.asset`，填写投射物预制体、速度、生命周期和对象池容量。
4. 在 `ProjectileAttackExecutor` 中读取并校验该参数配置类型；错误类型要输出明确日志。
5. 将参数资产拖入对应 AttackConfig 的 `Executor Parameter Config` 字段。

这样每个执行器拥有自己的参数结构，不会污染通用攻击配置。

## 当前碰撞攻击作为参考

`Attack_Contact.asset`：

```text
Id: 1001
Executor Id: contact
Target Faction: Player
Damage: 10
Cooldown: 1
```

`contact` 执行器会给 EnemyRoot 配置 `ContactAttackTrigger`。该组件检测不同 `CombatEntity` 阵营的碰撞体，发送通用攻击命令；AttackSystem 校验冷却后调用执行器，再由 DamageSystem 结算伤害。

## 完整验证清单

1. AttackConfig 的 ID 未重复。
2. AttackConfig 已加入 `AttackCatalog.asset`。
3. ExecutorId 已在 `AttackExecutorRegistry` 注册。
4. 攻击拥有者的 AttackIds 或技能组映射包含该 ID。
5. 触发器在创建/复用时注册，在回收/销毁时注销。
6. `dotnet build Assembly-CSharp.csproj --no-restore` 无新增错误。
7. Unity Play Mode 验证释放时机、冷却、阵营过滤、伤害、对象池回收及多目标边界情况。
