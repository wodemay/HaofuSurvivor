# 模块化与故障隔离审计

## 结论

当前项目具备 QFramework 的职责模块化，但还没有达到“单个模块编译或运行失败时，其他模块继续运行”的隔离级别。

闪避问题影响整个游戏的直接原因有两层：

1. Unity 当前没有项目级 `.asmdef` 拆分。`Scripts/` 下的代码默认编译到同一个 `Assembly-CSharp`。任意脚本出现语法错误、类型缺失或程序集编译失败，整个项目脚本程序集都会失效，不能进入正常 Play Mode。这一点无法仅靠业务代码消除。
2. 闪避装备曾被当成玩家生成的必需步骤。现在 `SetDodge()` 失败会清空 Dodge 并降级为禁用，玩家根不会因此销毁；仍会阻断启动的只有 PlayerRoot、角色预制体或必需 Weapon 等核心模块。

## 当前硬耦合

### 启动链

- `PlayerSpawnSystem`：只把核心玩家创建和必需 Weapon 装配结果作为玩家生成结果。
- `PlayerLoadoutSystem`：返回 `SkillGroupEquipResult`，Weapon、Skill、Dodge 分别记录结果；Dodge 失败只禁用该能力。
- `StartSelectedCharacterRunCommand`：只有 `SpawnSelectedCharacter()` 成功才调用 `RunSystem.StartRun()`。

因此，闪避不是“功能不可用”，而是被提升成了“玩家和对局不可创建”。

### 生命周期链

- `PlayerSystem.Unregister()` 调用 `PlayerLoadoutSystem.Reset()`。
- `PlayerLoadoutSystem.Reset()` 直接调用 `DodgeSystem.Reset()`。
- `RunSystem.StartRun()`、`ReleaseRunRuntime()` 也直接重置 Dodge。

玩家、对局和技能装配目前共享具体的 `DodgeModel`/`DodgeSystem` 类型，模块不能独立移除。

### 运行与成长链

- `PlayerSystem.Move()` 直接读取 `DodgeModel.Runtime.IsActive`。
- `PlayerSystem.ApplyDamage()` 直接读取 `PlayerModel.DodgeInvulnerabilityRemaining`。
- `LevelUpSystem` 直接查询和升级 Dodge。
- `RunSaveSystem.Restore()` 对无效 Dodge 或单个无效 Weapon 执行降级恢复；若存档中的 Weapon 全部无效则恢复失败，`RunSystem` 释放运行时并回到 `RunPhase.None`，原存档保留。
- `GameStart.FixedUpdate()` 每帧发送 `TickDodgeCommand`，闪避模块异常会持续暴露到主循环。

## 其他已发现隐患

- `DodgeCatalog.Get()` 的错误日志仍写着 `Configs/Skills`，会误导定位。
- `DodgeSystem.AdvanceFixed()` 找不到配置时直接返回，若运行时配置被破坏，闪避可能一直保持 `IsActive`。
- `SkillGroupConfig.StartingSkillIds` 当前只保存 ID，不校验技能是否存在，可能产生“技能组显示已装配、实际没有技能”的半初始化状态。
- `EnemyFactory.ConfigureRoot()` 对缺失 Attack 或 Executor 只跳过，不返回失败；玩家 Weapon 装配却严格失败，模块失败策略不一致。
- 所有 System、Model、Utility 在 `GameArchitecture.Init()` 中集中注册；类型编译隔离缺失，初始化错误也可能影响整套架构启动。
- `RunSaveSystem` 使用单个 PlayerPrefs JSON，没有版本号、校验和或分阶段提交；字段损坏时只能整体清除。

## 推荐隔离目标

### 编译级隔离

后续使用 Unity Assembly Definition 拆分为共享核心、Player、Combat、Enemy、Progression、Dodge、Save 和 UI Integration。依赖只能从上层指向核心契约，禁止循环引用。这样 Dodge 脚本编译失败时，至少不会阻断不依赖它的编辑器工具和独立模块。

### 运行级隔离

技能组装配已改为按槽位返回结果，而不是一个总 `bool`：

- Character/PlayerRoot 是启动必需项。
- Weapon、Skill、Dodge 分别报告装配结果。
- 当前 `SkillGroupConfig.RequireStartingWeapons` 默认为 `true`：初始 Weapon 属于核心能力，无法装配时阻止玩家启动。
- Dodge 配置无效时，玩家仍可生成并开始对局，只禁用闪避并记录一次警告。
- Skill 当前只保存 ID，属于可选能力，不会阻止玩家启动。

### 依赖反转

PlayerSystem 不应直接引用 Dodge 类型。普通移动只读取一个通用的“移动锁定/位移覆盖”能力；伤害系统只读取通用的“保护时间”服务。DodgeSystem 通过接口或事件提供这些能力，Dodge 被移除时核心 Player 仍有默认实现。

### 数据与恢复

恢复存档时，单个 Dodge 或 Weapon 无效会跳过该槽位并继续恢复可用内容；若所有 Weapon 都无效则恢复失败，运行时回滚到非运行状态，原存档保留。

## 本次未修改的内容

本次已完成第一阶段运行时隔离：技能组结果分槽位返回、Dodge 失败降级、存档单槽位恢复降级、恢复失败回滚；未修改 UI、Prefab、场景或配置资产。Assembly-CSharp 的编译级隔离仍未实施。
