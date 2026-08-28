# 成长与属性系统

本文说明当前已实现的通用属性、自然回血和角色专属升级；候选生成与确认见 `LevelUp.zh-CN.md`。

## 通用属性

`StatUpgradeCatalog` 提供有等级上限的百分比升级。当前包括攻击伤害、冷却缩减、经验倍率、经验吸收范围、移动速度、自然回血和道具恢复效率。`PlayerStatUpgradeSystem` 只修改 `PlayerStatModel` 的运行时倍率，不写回配置资产；恢复效率仅作用于后续外部治疗，并且所有治疗都会限制在最大生命值以内。

自然回血由 `PlayerRegenerationSystem` 按统一逻辑时间累计，每满一秒执行一次，暂停、升级选择、死亡和结算期间不会追赶或补算。

## 角色专属升级

`CharacterExclusivePerkCatalog` 为角色定义固定的专属升级项目和等级上限。`CharacterExclusivePerkSystem` 负责等级、快照和临时效果：幸存者当前包含闪避结束后短时间增加武器子弹数量，以及技能使用后短时间降低武器冷却的联动效果。

`CharacterExclusiveSkillUpgradeSystem` 负责专属 Skill 的一次性升级资格。角色指定 Weapon 与 Dodge 达到前置条件后，专属 Skill 会进入候选第一槽；升级后通过 `SkillRuntimeData.Level` 保存，并在继续游戏时恢复。

专属升级不占 Weapon 或通用属性槽位，所有运行时状态都通过 QFramework Model/System 管理，并由 `GameLoopSystem` 统一推进临时持续时间。
