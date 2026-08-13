# 新增 Character

1. 在 `Assets/Resources/Configs/Characters/` 创建 CharacterConfig。
2. 分配未使用的数字 `Id`。
3. 填写显示信息、基础属性、`SkillGroupId` 和 `PlayerPrefab`。
4. PlayerPrefab 只负责角色内容；必须由 PlayerRoot 在运行时承载。
5. 如需开局武器，在 `Resources/Configs/Skills/` 配置 SkillGroupConfig，并确保 Weapon/Attack ID 有效。

不要修改 PlayerRoot、HealthBarAnchor、CharacterRoot 或 UI 层级。角色生成由 `PlayerSpawnSystem` 统一完成。
