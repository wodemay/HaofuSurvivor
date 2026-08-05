# 新 Character 接入指南

角色选择、PlayerRoot、属性注册和摄像机跟随均已由现有模块处理。新角色只需要准备内容预制体与 `CharacterConfig`。

## 1. 创建角色内容预制体

在 `Assets/Art/Prefabs/Characters/` 创建角色内容预制体。它是外观、动画和角色专属内容，不是玩家运行时根节点。

运行时由 `PlayerSpawnSystem` 创建：

```text
PlayerRoot
├─ CharacterRoot     ← 挂载角色内容预制体
├─ HealthBarAnchor   ← 挂载 HealthBarCanvas
├─ AttackRoot
├─ EffectRoot
└─ HurtBox
```

不要将 PlayerController、Rigidbody2D、CameraFollow 或血条逻辑放到角色内容预制体；它们由 PlayerRoot 统一注入和管理。

## 2. 创建角色配置

可通过 Unity 菜单 `ProjectSurvivor/Configuration Creator` 自动创建内容预制体模板和 CharacterConfig；也可按下列规则手动创建。

在 `Resources/Configs/Characters/` 创建 `CharacterConfig`：

- `Id`：唯一整数角色 ID。
- `DisplayName`、`SkillDescription`、`Icon`：选择界面显示数据。
- `SkillId`：未来技能组 ID；当前可填 `0`。
- `PlayerPrefab`：角色内容预制体。
- `SortOrder`：选择界面排序。
- `MaxHealth`、`MoveSpeed`、`AttackPower`：初始属性。

CharacterCatalog 会加载该目录下的配置并按 `SortOrder` 排序，无需额外手写注册代码。

## 3. 角色攻击扩展

未来角色技能组应使用 `SkillId → AttackIds` 映射，将攻击注册到通用 `AttackSystem`。不要重新创建玩家专属攻击系统；参考 [NewAttack.zh-CN.md](NewAttack.zh-CN.md)。

## 验证

- ID 不与其他角色重复。
- PlayerPrefab 已绑定且为角色内容预制体。
- 角色在选择界面显示正确。
- 确认选择后，PlayerRoot 的 CharacterRoot 下出现该角色内容，血条与摄像机仍正常。
