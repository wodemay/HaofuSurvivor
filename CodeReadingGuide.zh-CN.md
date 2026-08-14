# ProjectSurvivor 代码阅读顺序（辅助）

> 个人阅读用速查，不属于正式项目文档；按入口到具体功能逐层查看。

## 1. 先看架构入口

1. `Assets/Scripts/Architecture/GameArchitecture.cs`：模块注册、Model/System/Utility 的组合。
2. `Assets/Scripts/Game/GameStart.cs`：Unity 生命周期如何驱动 QFramework Command。
3. `Assets/Scripts/Architecture/Commands/GameplayCommands.cs`：所有主要操作入口。
4. `Assets/Scripts/Architecture/Events/GameplayEvents.cs`：跨模块通知的数据结构。

## 2. 再看一局游戏的生命周期

1. `RunSystem.cs`：开始、暂停、恢复、结算、重开、返回。
2. `RunTimerSystem.cs`：统一游戏时间和时间阶段事件。
3. `RunSettlementSystem.cs`：死亡或退出时生成结算快照。
4. `RunSaveSystem.cs`：周期快照保存与继续游戏读取。

## 3. 查看角色生成与玩家状态

1. `CharacterCatalog.cs`、`CharacterConfig.cs`：角色配置如何加载。
2. `PlayerSpawnSystem.cs`：PlayerRoot、角色内容、血条的组装。
3. `PlayerSystem.cs`、`PlayerModel.cs`、`PlayerStatModel.cs`：移动、生命、伤害和属性。
4. `PlayerController.cs`：Unity 输入与移动请求的桥接。

## 4. 查看技能与攻击链

1. `SkillGroupCatalog.cs`、`SkillGroupConfig.cs`：角色初始技能组。
2. `PlayerLoadoutSystem.cs`：玩家 Weapon 容器、Attack 装配、升级和进化。
3. `AttackSystem.cs`、`AttackExecutorRegistry.cs`：通用攻击运行时和 Executor 查找。
4. `CollisionAttackExecutor.cs`、`ProjectileAttackExecutor.cs`：具体攻击实现。
5. `WeaponRuntimeData.cs`：运行时等级、修正值和 Attack 替换。

## 5. 最后查看敌人、成长和可选能力

1. `EnemySystem.cs`、`EnemyFactory.cs`、`EnemyHealthSystem.cs`：生成、追踪、对象池和死亡。
2. `ExperienceSystem.cs`、`ExperienceFactory.cs`：经验球掉落、吸附和收集。
3. `LevelUpSystem.cs`：升级选项、暂停选择和确认。
4. `DodgeSystem.cs`、`DodgeCatalog.cs`：可选闪避能力；异常不应阻断基础开局。

## 阅读时的判断方法

- 看到 `MonoBehaviour`：先确认它只是 Unity 桥接，不要把核心状态写进去。
- 看到 `Command`：追踪它调用的 System；看到 `Event`：追踪订阅者。
- 看到 `ScriptableObject`：只看静态配置，不应被运行时升级直接修改。
- 看到 `Factory`：检查对象池、场景切换和销毁对象引用。
