# 新 Enemy 接入指南

新 Enemy 只提供数据与内容预制体；移动、攻击、对象池、阵营和根节点逻辑由现有 Enemy/Combat 模块统一处理。

## 1. 创建内容预制体

在 `Assets/Art/Prefabs/Enemies/` 创建怪物内容预制体。它是视觉与内容对象，不是运行时根节点；不要添加 EnemyFactory、对象池或全局逻辑。

运行时结构固定为：

```text
EnemyRoot
├─ CharacterRoot     ← 挂载该内容预制体
├─ AttackRoot
├─ EffectRoot
├─ HurtBox
└─ DropRoot
```

保持 `EnemyRoot` 的这些节点名称不变。EnemyFactory 会将生成实例放到 MainScene 的 `EnemyContainer`，并按 Enemy ID 回收复用。

## 2. 创建配置

可通过 Unity 菜单 `ProjectSurvivor/Configuration Creator` 自动创建内容预制体模板、EnemyConfig 并加入 EnemyCatalog；也可按下列规则手动创建。

在 `Resources/Configs/Enemies/` 创建 `EnemyConfig`：

- `Id`：唯一整数。
- `Prefab`：怪物内容预制体。
- `MoveSpeed`：基础移动速度。
- `AttackIds`：该怪物拥有的攻击 ID 列表，例如 `[1001]`。

将配置加入 `EnemyCatalog.asset`。只有被 Catalog 引用的 Enemy 才能被时间轴查询到。

## 3. 配置生成阶段

在 `Resources/Configs/RunTimeline.asset` 的对应阶段填写 `EnemyIds`。00:00 阶段需要至少一个有效 ID，确保开局即可刷怪。

## 4. 攻击与碰撞

攻击 ID 必须已在 `Resources/Configs/Combat/AttackCatalog.asset` 中定义。EnemyFactory 根据攻击对应的 Executor 配置触发器；例如 `contact` 会使用 EnemyRoot 的 HurtBox 参与碰撞攻击。

## 验证

- EnemyConfig ID 与时间轴 EnemyIds 一致。
- 内容预制体引用有效。
- EnemyRoot 仍有 `CharacterRoot`。
- 运行后 EnemyContainer 下出现 EnemyRoot，重开一局后根节点被停用并进入对应 ID 的对象池。
