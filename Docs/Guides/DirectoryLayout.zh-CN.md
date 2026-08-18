# 目录结构

本文只说明当前脚本和运行时 Config 的归类，不重复模块职责。

## 脚本

`Assets/Scripts/Architecture/` 按 QFramework 业务模块划分：Character、Player、Enemy、Combat、Skill、Dodge、Experience、LevelUp、Run、Save、Input，以及跨模块的 Commands 和 Events。

`Assets/Scripts/Game/` 仅放 Unity 桥接：`Bootstrap/` 是启动入口，`Camera/` 是表现跟随，`Combat/` 是 CombatEntity、碰撞和投射物桥接，`Player/` 是角色内容兼容标记与血条视图，`Presentation/` 是世界渲染层级等纯表现规则。`UI/` 和 `Editor/` 保持独立。

## Config

`Assets/Resources/Configs/` 与运行模块同名：

- `Character/`：角色静态数据。
- `Enemy/`：敌人目录、敌人数据与 EnemyRoot。
- `Player/`：PlayerRoot。
- `Combat/Attack/`：全局 Attack 目录和通用攻击。
- `Combat/Weapon/`、`Combat/Skill/`、`Combat/Dodge/`：三类玩家战斗能力及其目录。
- `Progression/Experience/`：经验掉落和等级需求。
- `Run/`：时间线阶段配置。

所有 Resources 加载路径必须与此结构同步；移动 ScriptableObject 时保留其 `.meta`，以保持 Catalog、Prefab 和场景引用的 GUID 不变。
