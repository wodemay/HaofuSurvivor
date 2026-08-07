# 角色与敌人攻击流程

## 核心原则

攻击配置不区分角色和敌人。两者都使用 `AttackConfig`、`AttackSystem`、`CombatEntity`、`DamageSystem` 与 `IAttackExecutor`。差异只来自攻击拥有者的阵营和攻击来源：角色由技能组装备武器，敌人直接使用 `EnemyConfig.AttackIds`。

`AttackSystem` 只负责攻击运行时、冷却和阵营校验；具体行为由 `AttackConfig.ExecutorId` 找到的执行器完成。当前可用执行器为 `collision` 与 `projectile`。

## 角色攻击流程

### 1. 角色出生并取得初始武器

1. `PlayerSpawnSystem` 根据选中的 `CharacterConfig` 创建 PlayerRoot，并写入 `CombatFaction.Player`。
2. 它读取 `CharacterConfig.SkillGroupId`，交给 `PlayerLoadoutSystem.EquipInitialSkillGroup`。
3. `SkillGroupConfig.StartingWeaponIds` 找到对应 `WeaponConfig`。
4. `WeaponConfig.InitialAttackIds` 填充该 WeaponRuntime 的一个或多个 `AttackConfig`。
5. 每个攻击的执行器调用 `ConfigureOwner(PlayerRoot, attackConfig, Player, weaponRuntimeId)`，在 PlayerRoot 上挂接或复用对应 Trigger。

当前 survivor 的技能组 1 装备武器 1；武器 1 配置攻击 1002，即投射物攻击。

### 2. 角色发射投射物

1. `ProjectileAttackTrigger.Update` 使用 `FindClosestCombatTargetQuery` 查询攻击范围内最近的敌对 `CombatEntity`。
2. 找到敌人后发送 `TryExecuteAttackCommand`。
3. `AttackSystem` 检查：局内处于 Active、攻击已注册、目标不是同阵营、冷却已结束。
4. `ProjectileAttackExecutor` 读取 `ProjectileAttackParameterConfig`，按拥有者到目标的方向调用 `ProjectileFactory.Spawn`。
5. `ProjectileController` 移动子弹；碰到敌对 `CombatEntity` 时发送 `ApplyCombatDamageCommand`，随后回收到对象池。
6. `DamageSystem` 将敌人目标路由到 `EnemyHealthSystem`；敌人生命归零时发布死亡事件并由 `EnemySystem` 回收 EnemyRoot。

角色未来新增碰撞、范围或特殊攻击时，先填入 WeaponRuntime；初始内容来自 WeaponConfig 的 `InitialAttackIds`，局内替换通过 `ReplaceWeaponAttacksCommand`，不应在角色代码中添加攻击类型分支。

## 敌人攻击流程

### 1. 敌人生成并配置攻击

1. `EnemySystem` 按 `RunTimelineConfig` 当前阶段的 `EnemyIds` 选择敌人配置。
2. `EnemyFactory.Create` 获取或创建对应 ID 的 EnemyRoot，挂载 `EnemyConfig.Prefab` 到 `CharacterRoot`。
3. `EnemyFactory.ConfigureRoot` 给 EnemyRoot 初始化 `CombatFaction.Enemy`、Kinematic Rigidbody2D 和运行时生命。
4. 它遍历 `EnemyConfig.AttackIds`，按攻击的 `ExecutorId` 调用执行器 `ConfigureOwner(EnemyRoot, attackConfig, Enemy)`。

敌人不经过 `SkillGroupConfig` 或 `WeaponConfig`，因为它的攻击列表直接由自身配置决定。

### 2. 敌人碰撞攻击

1. `CollisionAttackTrigger.OnTriggerStay2D` 找到接触到的 `CombatEntity`。
2. 同阵营目标直接忽略；玩家目标发送 `TryExecuteAttackCommand`。
3. `AttackSystem` 执行通用状态与冷却校验。
4. `CollisionAttackExecutor` 将 `AttackConfig.Damage` 乘以当前 `RunTimerModel.EnemyDamageMultiplier`。
5. `DamageSystem` 将玩家目标路由到 `PlayerSystem.ApplyDamage`。
6. `PlayerSystem` 先检查无敌帧，随后扣血并发布 `PlayerDamagedEvent`；血量归零发布 `PlayerDiedEvent` 并结束本局。

### 3. 敌人投射物攻击

敌人也可以在 `EnemyConfig.AttackIds` 中配置攻击 1002 或未来的投射物攻击。`ProjectileAttackTrigger` 会以 Enemy 阵营查询最近玩家，后续步骤与角色投射物完全相同；唯一不同是伤害会乘以 `EnemyDamageMultiplier`，子弹只会命中 Player 阵营。

## 攻击运行时与回收

- Trigger 初始化时通过 `RegisterAttackCommand` 注册运行时 ID；禁用时通过 `UnregisterAttackCommand` 注销。
- `GameStart` 每帧发送 `TickAttacksCommand`，由 `AttackSystem.Advance` 递减冷却。
- EnemyRoot 死亡或局内重置时，`EnemySystem` 先注销敌人生命，再由 `EnemyFactory` 按敌人 ID 回收根节点；复用时重新注册既有 Trigger，不重复挂载攻击组件。
- 子弹命中或生命周期结束时，由 `ProjectileFactory` 回收到运行时 `ProjectileContainer`；两类对象池都会跳过跨场景后已销毁的池引用。

## 新攻击的接入位置

1. 在 `Resources/Configs/Combat/` 创建 `Attack_<技能名称>`，填写唯一 ID、伤害、冷却和 `ExecutorId`。
2. 新行为实现 `IAttackExecutor`，并在 `AttackExecutorRegistry` 注册；需要持续触发时实现对应 `IAttackTrigger`。
3. 角色攻击将 ID 放进 `WeaponConfig.InitialAttackIds` 或运行时 Weapon 容器；敌人攻击将 ID 放进 `EnemyConfig.AttackIds`。
4. 任何伤害都通过 `ApplyCombatDamageCommand` / `DamageSystem` 进入目标结算，禁止执行器直接修改角色或敌人生命。
