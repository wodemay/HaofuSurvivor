# 三角色战斗配置与验收

2026-09-29 已接入游戏，保持 Space 主动技能、Shift 闪避。基础数值见 `../CharacterVanguardScoutDesign.md`；实际伤害和冷却仍叠加现有局外、局内属性倍率。

| 角色 | 初始武器 | 主动技能 | 闪避 | 技能组 |
| --- | --- | --- | --- | --- |
| 幸存者1 | 原武器1 | 技能1：星环超载，Attack1011 | 普通闪避1 | 1 |
| 先锋2 | 武器5：裂阵剑术，Attack1007 | 技能2：断阵回旋，Attack1008 | 锋刃突进2 | 2 |
| 斥候3 | 武器6：贯穿弩，Attack1009 | 技能3：猎杀连射，Attack1010 | 掠影装填3 | 3 |

## 接入与边界

- `CharacterAttackExecutors.cs` 分别实现 sword、spin-sword、piercing-bolt、hunter-volley、star-overload；注册仍由 AttackExecutorRegistry 管理，没有按角色ID的战斗分支。
- `CharacterCombatModel` 保存阶段、伤害快照、减伤、装填与命中记录；`CharacterCombatSystem` 使用 GameLoop 局内 Tick 调度、回收表现、处理存档。
- `IPreparedAttackExecutor` 在扣冷却前验证施放条件并解析技能等级冷却。斥候无目标不消耗冷却；`ITransientAttackSequence` 标识不跨读档继续的短促普攻。
- 挥剑抬手结束锁方向，0.12秒挥击窗口对每个出生身份最多命中一次；普通敌人击退通过形状Cast截断，Boss免疫击退。剑为独立实体Sprite，在手部附近锚点摆动，身体沿用现有待机/跑步帧。
- 闪避最后一步按剩余时间裁剪，碰墙立即终止位移与无敌。路径伤害和装填效果由独立 DodgeEffectExecutor 实现；Reset/死亡取消不发放装填强化。
- 普通弩沿用全局投射物数量、速度加成。装填强化作用于下一次成功的弩齐射并消耗，额外命中数为2。新角色弹体使用扫掠碰撞，按距离先处理敌人或墙；单目标去重使用每次出生的 SpawnId。
- 一级星环在0.35秒展开后发射6轮、每轮4枚单目标弹，末尾收拢并触发一次终结范围伤害。终结发送 CombatFinisherEvent，CameraFollow 提供0.15秒、最大0.05单位轻震，可在 Inspector 关闭 EnableCombatShake。
- 音效为本轮原创短合成音，四声道上限；暂停和升级选择暂停音效，恢复继续，回菜单/重开停止并回收。未做人工听感验收。
- 角色专属技能升级沿用既有条件：专属武器、闪避满级后出现，技能最高3级。两种初始武器最高5级，新闪避最高3级。

## 资源

`Assets/Art/Sprites/Effects/CharacterCombat.png` 是内置 image_gen 生成的透明六格图集；最终文件来自 `exec-396e2715-0cd6-4b5b-a7ff-0856d2294662.png`，保留其alpha，未使用Python修图。初版来自 `exec-3d9c7bf0-3a07-41ef-a96e-7646ff437a1e.png`，随后通过生图编辑收紧背景。

七个预制体位于 `Art/Prefabs/Attack/`：VanguardSword、SwordSlash、OrbitDrone、CombatRing、ScoutBolt、DodgeThrust、DodgeAfterimage；音效预制体为 `Resources/Presentation/CharacterCombatAudio.prefab`。没有生成UI或改QFramework绑定。

`Editor/CharacterCombatSetup.cs` 提供一次性/重应用配置入口，通过 AssetDatabase 创建和绑定所有资产，ID按现存最大值加一。再次执行会恢复本方案的基础数值，不应在手动调优后无意运行。

## 存档

局内版本升为3。技能阶段、已发射轮数、减伤和装填有效期可恢复；表现对象重建。恢复时取消进行中的短促挥剑与闪避，保留冷却，避免重放路径伤害及闪避结束强化。旧空装备局补齐当前角色初始武器/技能/闪避；已有装备局保留原快照。幸存者旧技能冷却按技能运行时身份继承，旧弹幕对象允许自行结束。

## 验证

- `CharacterCombatVerification.Run`：19项；基础伤害与倍率、单次命中、减伤、序列JSON恢复、暂停、冲刺去重、装填消耗、连射数量、无目标、浮游炮轮数/终结及退出清理。
- `Boundaries`：14项；墙体截断、碰墙取消无敌、武器/闪避/技能升级资格和上限、满级连射与冷却、强化恢复。
- `ProjectilesAndEffects`：10项；高速弹命中3人、穿透耗尽、墙体阻断、残影回收、四声道上限、暂停/恢复/升级暂停和重置。
- 另外4项：原用户v2空装备快照的内存迁移、v3校验、非法数值拒绝、旧技能冷却继承。
- 终结震动2项：事件触发及关闭开关生效；累计49项检查通过。
- dotnet 编译0错误，2个既有程序集版本警告。Unity最终 Console查询无错误/警告。截图保存在 `Logs/CharacterReview/`；合成场景静帧不是长时间游戏验收。

待后续验收：长局平衡与人工听感。当前已实现独立剑的挥砍表现，未重绘人物全套身体攻击帧。原存档备份保留在 Logs/CharacterCombatSaveBackup，五个原文件均已恢复且SHA256校验一致。

## 生图提示词原文

初版：

```text
Create a single production-ready 2D game combat VFX sprite atlas, transparent alpha background, exactly 3 columns by 2 rows of equally sized square cells, 1536x1024 landscape. No text, no grid lines, no shadows/background. Each sprite centered entirely inside its cell with generous 15% transparent padding and no overlap. Polished hand painted stylized fantasy sci fi, crisp silhouette, restrained luminous edges, readable small scale. TOP LEFT: one physical silver long sword pointing horizontally RIGHT, brown gold grip on left, complete handle guard and blade, sharp point. TOP CENTER: gold white crescent sword slash arc, opening to LEFT, right-facing 120 degree arc, transparent interior. TOP RIGHT: compact cyan silver floating drone gun pointing RIGHT, complete isolated body, small cyan energy core. BOTTOM LEFT: circular cyan blue energy shockwave ring face on, transparent center and exterior, crisp double ring with restrained sparks. BOTTOM CENTER: narrow cyan green crossbow bolt pointing RIGHT, entire projectile visible with short tapered energy tail to left. BOTTOM RIGHT: golden white wedge shaped thrust streak pointing RIGHT with several small dust sparks, transparent space around it. All six sprites occupy centered roughly 70% of cell dimensions. Actual transparent background, not checkerboard.
```

编辑版：

```text
Edit this exact six-sprite atlas for game-engine import. Remove ALL black/colored backdrop and rectangular ambient glow; make background and the inside of the ring genuinely alpha transparent. Preserve the six objects' design. Fit EACH object fully within its own equal 512x512 cell in this 3x2 1536x1024 atlas with at least 40 pixels completely transparent on all cell edges. Sword top left must not cross cell right edge. Keep faint glow ONLY tight around solid edges, never spanning neighboring cells. TOP row sword pointing right, right crescent arc, floating gun. BOTTOM row ring, bolt pointing right, thrust streak. No opaque background, no checkerboard painted into image, no text, no labels. All atlas gutter regions fully transparent alpha.
```
