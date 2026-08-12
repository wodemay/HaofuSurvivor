# 项目脚本说明

本文以当前 `Assets/Scripts/` 为准，说明每个脚本的职责与边界。业务链路必须遵循 QFramework：Controller/View → Command → System → Model/Utility；读取使用 Query，跨模块通知使用 Event。

## 方法级调用索引

本节按“函数 → 加载/调用对象 → 实际行为”定位实现。路径均相对 `Assets/Resources/`；所有命令都集中在 `Architecture/Commands/GameplayCommands.cs`，只把请求转给 System，不保存业务状态。

### 启动、角色选择、玩家创建

- `Game/GameStart.cs`
  - `Awake()`：调用 `ResKit.Init()` 与 `GameArchitecture.InitArchitecture()`；订阅 `RunStartedEvent` 后调用 `OpenGameHud()`，订阅 `LevelUpSelectionRequestedEvent` 后调用 `OpenLevelUpPanel()`。
  - `Start()`：通过 `UIKit.OpenPanel<UICharacterSelectPanel>(assetBundleName: "uicharacterselectpanel_prefab")` 打开角色选择界面。
  - `Update()`：依次发送 `TickRunTimerCommand`、`TickExperienceCommand`、`TickAttacksCommand`、`TickPlayerDamageInvulnerabilityCommand`；每一步后读取 `GetRunTimeStateQuery`，暂停/升级时立即中断后续模拟。
  - `FixedUpdate()`：发送 `TickRunPhysicsCommand(Time.fixedDeltaTime)`；运行中再发送 `TickEnemiesCommand`。这就是敌人固定帧追击的入口。
  - `OpenGameHud()` / `OpenLevelUpPanel()`：分别加载 `uigamehudpanel_prefab` 与 `uileveluppanel_prefab` 下的既有 QFramework 面板。
- `Architecture/Character/CharacterCatalog.cs`
  - 构造函数：`Resources.LoadAll<CharacterConfig>("Configs/Characters")` 加载角色配置，按 `SortOrder` 排序。
  - `Get(id)`：返回匹配 ID 的 CharacterConfig；未找到时临时回退到第一个角色。
- `Architecture/Character/CharacterSelectionSystem.cs`
  - `Select(characterId)`：通过 `CharacterCatalog.Contains()` 校验，写入 `CharacterSelectionModel.SelectedCharacterId` 并发送选择变化事件。
  - `ConfirmSelection()`：调用 `CharacterSelectionStorage.SaveSelectedCharacterId()` 写入 PlayerPrefs，然后发送开局命令。
- `UI/UICharacterSelectPanel.cs`
  - `BuildCharacterItems()`：遍历 `CharacterCatalog.All`，克隆模板，调用 `UICharacterSelectItem.Initialize()`。
  - `ConfirmSelection()`：发送确认选择命令；不直接创建玩家。
- `UI/UICharacterSelectItem.cs`
  - `Initialize()`：从 CharacterConfig 读取 Icon、DisplayName、SkillDescription，并用 `Transform.Find` 写入 `Image_Select`、`Image_Icon`、`GameObject/Text_Name`、`GameObject/Text_Skill`。
- `Architecture/Player/PlayerSpawnSystem.cs`
  - `SpawnSelectedCharacter()`：读取选择 ID 和 `Resources/Configs/PlayerRoot.asset`，实例化 PlayerRoot；把角色 Prefab 挂到 `CharacterRoot`，血条 Prefab 挂到 `HealthBarAnchor`。
  - `EnsurePlayerComponents()`：补齐 PlayerRoot 的 Rigidbody2D、PlayerController、CombatEntity，并将阵营设为 Player。
  - `BindCamera()`：查找 `MainCamera`，绑定 `CameraFollow`。
  - `DespawnCurrentCharacter()`：重置 Loadout、注销玩家状态并销毁当前 PlayerRoot。
- `Architecture/Run/RunSystem.cs`
  - `StartRun()`：进入 Active，调用计时、敌人、经验、升级模块各自的 Reset/Start，并发送 `RunStartedEvent`。

### 时间、暂停、敌人、经验

- `Architecture/Run/RunTimerSystem.cs`
  - `StartTimer()`：清空计时、阶段索引、敌人倍率和逻辑 DeltaTime，设置 `Time.timeScale = 1`。
  - `Advance(unscaledDeltaTime)`：只有 `IsRunning()` 才把传入值写为 `RunTimerModel.DeltaTime`，累加 ElapsedSeconds，随后调用 `ApplyReachedStages()`。
  - `AdvanceFixed(unscaledFixedDeltaTime)`：写入 FixedDeltaTime；暂停状态下写入 0，所以不会在恢复时补算暂停时间。
  - `ApplyReachedStages()`：读取 `RunTimelineCatalog.Config.Stages`，更新敌人生命/伤害/移速/生成倍率并发送 `RunTimelineStageReachedEvent`。
  - `Pause()` / `Resume()` / `Stop()`：控制 `IsPaused`、逻辑时间和 `Time.timeScale`。
- `Architecture/Run/RunSystem.cs`
  - `Pause()` / `Resume()`：仅允许 Active 与 Paused 间切换。
  - `BeginLevelUpSelection()` / `EndLevelUpSelection()`：把对局改为 `LevelUpSelection` 并调用计时暂停/恢复。
  - `ExitToCharacterSelection()`：停止计时，回收敌人和经验球、清空升级、销毁玩家；HUD 的临时返回按钮调用此流程。
- `Architecture/Enemy/EnemyConfigCatalog.cs`
  - 构造函数：`Resources.Load<EnemyCatalogConfig>("Configs/Enemies/EnemyCatalog")` 加载敌人目录；`Get(id)` 按敌人 ID 查找。
- `Architecture/Enemy/EnemySystem.cs`
  - `Tick()`：读取玩家状态和统一 FixedDeltaTime；调用 `Spawn()` 与 `Move()`。
  - `Spawn()`：根据 EnemyCatalog、时间、阶段 SpawnRateMultiplier、波次和存活上限，调用 `EnemyFactory.Create()` 在摄像机视野外生成。
  - `Move()`：对每个活跃 EnemyRoot 的 Rigidbody2D 调用 `MovePosition()`，追向玩家，速度乘阶段移速倍率。
  - `Reset()` / `Release()`：清空活跃列表，并将 EnemyRoot 归还 Factory 的对象池。
- `Architecture/Enemy/EnemyFactory.cs`
  - `Create(config, position)`：读取 `Resources.Load<EnemyRootConfig>("Configs/EnemyRoot")`；从按 EnemyConfig.Id 划分的池取 EnemyRoot，或实例化 EnemyRoot，并把 `config.Prefab` 放到 `CharacterRoot`。
  - `ConfigureRoot()`：添加/初始化 CombatEntity、向 EnemyHealthSystem 注册 `config.BaseHealth`、确保 Kinematic Rigidbody2D；遍历 `config.AttackIds`，按 AttackConfig 的 ExecutorId 配置攻击桥接组件。
  - `Release()` / `ReleaseAllActive()`：停用 EnemyRoot 并入池；`PruneDestroyedRoots()` 清除跨场景后的伪空引用。
- `Architecture/Enemy/EnemyHealthSystem.cs`
  - `Register(enemy, baseHealth)`：用当前 `EnemyHealthMultiplier` 得到实际初始生命。
  - `ApplyDamage()`：生命归零时发送带死亡位置和 ExperienceDropConfig 的 `EnemyDiedEvent`，再由 EnemySystem 回收对象。
- `Architecture/Experience/ExperienceSystem.cs`
  - `OnEnemyDied()`：读取事件中的 `ExperienceDropConfig`，调用 `ExperienceFactory.Create(config, deathPosition)`，再配置经验值并加入活动列表。
  - `Tick()`：读取玩家吸取范围、加速度、最高速度；首次进入范围将经验球标记为 captured，之后不再解除捕获，持续调用 `ExperienceDropController.MoveTowards()`。
  - `Collect()`：回收经验球并调用 `AddExperience()`。
  - `AddExperience()`：读取 `ExperienceProgressionCatalog.Config.GetRequiredExperience()`，可以连续升级；每升一级发送 `PlayerLevelUpEvent`。
- `Architecture/Experience/ExperienceFactory.cs`
  - `Create()`：要求场景存在 `ExperienceDropContainer`，按 ExperienceDropConfig.Id 从池取物体或实例化 `config.Prefab`，确保其有 ExperienceDropController。
  - `Release()`：停用经验球并返回对应 ID 的队列；`PruneDestroyedEntries()` 会跳过已销毁对象。

### 攻击、伤害、投射物

- `Game/CombatEntity.cs`
  - `Initialize(faction)`：设置 Player/Enemy 阵营。
  - `OnEnable()` / `OnDisable()`：向 CombatTargetSystem 注册或注销自己，所以对象池复用后会重新成为可选攻击目标。
- `Architecture/Combat/AttackCatalog.cs`
  - 构造函数：`Resources.Load<AttackCatalogConfig>("Configs/Combat/AttackCatalog")` 加载 Attack ID 总索引；`Get(id)` 返回配置。
- `Architecture/Combat/AttackExecutorRegistry.cs`
  - `ConfigureOwner()`：根据 `AttackConfig.ExecutorId` 找到执行器并要求它在 owner 上建立 Trigger。
  - `CollisionAttackExecutor.ConfigureOwner()`：复用/初始化 CollisionAttackTrigger；EnemyRoot 复用时重新注册旧组件，不会重复 AddComponent。
  - `ProjectileAttackExecutor.ConfigureOwner()`：初始化 ProjectileAttackTrigger。
  - `ProjectileAttackExecutor.Execute()`：读取 WeaponRuntime 的数量、伤害、速度、穿透修正，计算多发扇形方向并调用 ProjectileFactory。
  - `AttackTriggerUtility.Remove()`：按 WeaponRuntimeId 删除指定武器的 Trigger，替换武器时不会误删其他武器。
- `Architecture/Combat/AttackSystem.cs`
  - `Register()` / `Unregister()`：保存或移除 AttackRuntime（攻击 ID、拥有者、阵营、武器槽位、冷却）。
  - `TryExecute()`：拒绝同阵营、目标无效或冷却中攻击；其余情况调用 Registry.Execute()。
  - `Advance()`：用统一 DeltaTime 减少冷却，并读取 WeaponRuntime 的冷却修正。
- `Game/CollisionAttackTrigger.cs`
  - `Initialize()`：记录 Attack ID、阵营和 WeaponRuntimeId。
  - `OnTriggerStay2D()`：发现敌对 CombatEntity 后发送注册与尝试攻击命令。
  - `Unregister()` / `OnDisable()`：移除 AttackRuntime，避免对象池中残留攻击状态。
- `Game/ProjectileAttackTrigger.cs`
  - `Update()`：发送 `FindClosestCombatTargetQuery` 找最近敌对目标，找到后发送尝试攻击命令。
  - `Initialize()` / `Unregister()` / `OnDisable()`：与碰撞 Trigger 同样维护 AttackRuntime 生命周期。
- `Game/ProjectileFactory.cs`
  - `Spawn()`：在运行时 `ProjectileContainer` 下取出/创建投射物，随后调用 `ProjectileController.Launch()`。
  - `Get()`：按参数资产维护池，并调用 `PruneDestroyedProjectiles()` 清掉跨场景的已销毁引用。
- `Game/ProjectileController.cs`
  - `Launch()`：写入一发投射物的位置、方向、伤害、速度、寿命、射程、阵营和穿透次数。
  - `FixedUpdate()`：使用 RunTimerModel.FixedDeltaTime 移动。
  - `Update()`：使用 RunTimerModel.DeltaTime 判断寿命和射程。
  - `OnTriggerEnter2D()`：只处理敌对 CombatEntity；调用通用伤害命令、记录本次飞行已命中的目标，穿透耗尽后回池。
- `Architecture/Player/DamageSystem.cs`
  - `ApplyDamage(target, damage)`：根据 target 的 CombatFaction，转交 `PlayerSystem.ApplyDamage()` 或 `EnemyHealthSystem.ApplyDamage()`；攻击执行器不直接改血。

### 武器、升级、进化与升级 UI

- `Architecture/Skill/SkillGroupCatalog.cs`：构造函数用 `Resources.Load<SkillGroupCatalogConfig>("Configs/Skills/SkillGroupCatalog")` 加载技能组目录；`Get(id)` 返回角色 SkillGroupId 对应配置。
- `Architecture/Skill/WeaponCatalog.cs`：构造函数用 `Resources.Load<WeaponCatalogConfig>("Configs/Weapon/WeaponCatalog")` 加载武器目录；`Get(id)` 返回 WeaponConfig。
- `Architecture/Skill/WeaponEvolutionCatalog.cs`：构造函数用 `Resources.Load<WeaponEvolutionCatalogConfig>("Configs/Weapon/WeaponEvolutionCatalog")` 加载进化表；`Get(sourceWeaponId, level)` 查找目标武器。
- `Architecture/Skill/PlayerLoadoutSystem.cs`
  - `EquipInitialSkillGroup()`：调用 SkillGroupCatalog，按 StartingWeaponIds 装备武器，保存 StartingSkillIds 和 StartingDodgeId 预留数据。
  - `EquipWeapon()`：读取 WeaponConfig.InitialAttackIds，校验每个 AttackId 都能找到 AttackConfig 和 Executor；成功后创建 WeaponRuntimeData 并调用 `ConfigureAttacks()`。
  - `ConfigureAttacks()`：对当前武器 AttackIds 调用 AttackExecutorRegistry.ConfigureOwner()，把攻击组件配置到 PlayerRoot。
  - `UpgradeWeapon()`：读取 WeaponConfig.LevelUpgrades 中下一等级的数据，增删 AttackIds，调用 `WeaponRuntimeData.ApplyModifiers()`，最后发送升级事件。
  - `ReplaceWeaponAttacks()`：先按 WeaponRuntimeId 移除旧 Trigger，再写入新攻击列表并重新配置。
  - `TryEvolveWeapon()`：读取进化表；目标必须 `MaxLevel = 1` 且 `CanUpgrade = false`，然后调用 `ReplaceWeapon()`。
  - `ReplaceWeapon()`：保留同一 RuntimeId，清掉旧 Trigger 和旧修正，写入目标武器初始 AttackIds 后重新配置。
  - `Reset()`：移除所有武器 Trigger 后清空 PlayerLoadoutModel。
- `Architecture/Skill/WeaponRuntimeData.cs`
  - `ApplyModifiers()`：累计当前局每个 Attack ID 的数值修正。
  - `GetModifierValue()`：由 AttackSystem、ProjectileAttackExecutor 用修正键读取最终运行时数值。
- `Architecture/LevelUp/LevelUpSystem.cs`
  - `OnPlayerLevelUp()`：有候选时将等级加入队列并调用 `PresentNextSelection()`。
  - `GetLevelUpWeaponOptionsQuery.OnDo()`：遍历 PlayerLoadoutModel.Weapons；未满级生成 `LevelX->LevelX+1`，满足进化表则生成 `LevelX->Evolve`。
  - `CompleteWeaponUpgrade()`：根据 `isEvolution` 调用 UpgradeWeapon 或 TryEvolveWeapon；完成队首后继续弹下一次选择，或恢复对局。
  - `PresentNextSelection()`：切换到 `LevelUpSelection` 并发送打开 UI 的事件；没有待选项时恢复 Active。
- `UI/UILevelUpPanel.cs`
  - `RefreshOptions()`：发送升级候选 Query 并按既有模板创建/刷新 UI 条目。
  - `SelectWeapon()`：把 RuntimeId 与是否进化提交给升级确认命令。
- `UI/UILevelUpOptionItem.cs`
  - `Initialize()`：写入武器名称、说明、图标和 `LevelText`，并绑定透明选择按钮的回调。

### 失败结算、重开与返回

- `Architecture/Player/PlayerSystem.cs`
  - `ApplyDamage()`：生命归零后设置 `PlayerModel.IsDead = true`，发送 `PlayerDiedEvent`，再调用 `RunSystem.EndWithDefeat()`。
- `Architecture/Run/RunSystem.cs`
  - `EndWithDefeat()`：将阶段改为 `Defeat`，调用 `RunTimerSystem.Stop()` 冻结统一时间，并发送 `RunEndedEvent(Defeat)`。
  - `RestartSelectedCharacterRun()`：仅接受 Defeat 阶段；调用 `ClearRunObjects()` 清理上一局，再用当前 `CharacterSelectionModel.SelectedCharacterId` 通过 `PlayerSpawnSystem.SpawnSelectedCharacter()` 创建角色，成功后调用 `StartRun()`。
  - `ExitToCharacterSelection()`：将阶段改为 None，停止计时并调用 `ClearRunObjects()`。
  - `ClearRunObjects()`：依次重置 EnemySystem、ExperienceSystem、LevelUpSystem，调用 `ProjectileFactory.ReleaseAllActive()`，最后销毁当前 PlayerRoot。
- `Architecture/Commands/GameplayCommands.cs`
  - `RestartSelectedCharacterRunCommand.OnExecute()`：把重开请求转发给 `RunSystem.RestartSelectedCharacterRun()`。
  - `ExitRunToCharacterSelectionCommand.OnExecute()`：把返回请求转发给 `RunSystem.ExitToCharacterSelection()`。
- `Game/GameStart.cs`
  - `Awake()`：订阅 `RunEndedEvent`。
  - `OnRunEnded()`：仅处理 Defeat；关闭 HUD 和升级面板，通过 `UIKit.OpenPanel<UIGameOverPanel>()` 加载 `uigameoverpanel_prefab` 并打开失败结算界面。
- `UI/UIGameOverPanel.cs`
  - `OnInit()`：绑定用户创建的 `Button_Restart` 与 `Button_Return`。
  - `Restart()`：发送 `RestartSelectedCharacterRunCommand`，成功进入新局后关闭失败面板。
  - `ReturnToCharacterSelection()`：发送 `ExitRunToCharacterSelectionCommand`，关闭失败面板并打开 `uicharacterselectpanel_prefab`。
- `Game/ProjectileFactory.cs`
  - `ReleaseAllActive()`：遍历已登记投射物，只将当前激活对象重置并回收到 `ProjectileContainer` 对应对象池。

## Architecture/Character

- `CharacterConfig.cs`：角色静态数据，含数值、内容预制体与 `SkillGroupId`。
- `CharacterCatalog.cs`：加载角色配置，按排序和 ID 查询。
- `CharacterSelectionModel.cs`：保存当前选中的角色 ID。
- `CharacterSelectionStorage.cs`：用 PlayerPrefs 持久化角色选择。
- `CharacterSelectionSystem.cs`：校验、切换和确认选择，并发布选择事件。

## Architecture/Skill

- `SkillGroupConfig.cs`：定义初始武器、技能和闪避 ID。
- `SkillGroupCatalogConfig.cs` / `SkillGroupCatalog.cs`：技能组目录与按 ID 查询。
- `WeaponConfig.cs`：武器静态容器模板；`InitialAttackIds` 定义初始填充物，`MaxLevel`、`CanUpgrade` 和 `WeaponLevelUpgrade` 定义升级边界、每级 Attack 增删、显示文本与按 Attack ID 生效的运行时修正。
- `WeaponCatalogConfig.cs` / `WeaponCatalog.cs`：武器目录与按 ID 查询。
- `WeaponEvolutionConfig.cs`：定义来源武器、进化所需等级和目标武器；目标武器必须固定为 1 级且不可升级。
- `WeaponEvolutionCatalogConfig.cs` / `WeaponEvolutionCatalog.cs`：进化目录与运行时查询。
- `WeaponRuntimeData.cs`：单把角色武器的运行时容器，保存稳定槽位 ID、当前 WeaponId、等级、可升级状态、Attack 填充物与按 Attack ID 累加的升级修正；替换或进化时保留槽位 ID 并清空旧修正。
- `GetPlayerWeaponsQuery.cs`：只读返回当前 WeaponRuntime 快照。
- `PlayerLoadoutModel.cs`：保存本局 WeaponRuntime、技能、闪避和当前拥有者。
- `PlayerLoadoutSystem.cs`：装备、升级、替换 Attack、直接替换 Weapon、按目录进化和重置角色武器容器；校验 Attack 配置，并管理每个容器独立的 Attack Trigger 生命周期。

## Architecture/Combat

- `CombatFaction.cs`：玩家与敌人的阵营标识。
- `AttackConfig.cs`：通用攻击静态数据；`ExecutorId` 决定行为，含伤害、冷却和可选参数资产。
- `AttackCatalogConfig.cs` / `AttackCatalog.cs`：攻击目录与按 ID 查询。
- `ProjectileAttackParameterConfig.cs`：子弹预制体、速度、射程、寿命和对象池容量。
- `AttackExecutorRegistry.cs`：执行器注册表；含 `IAttackExecutor`、`CollisionAttackExecutor` 与 `ProjectileAttackExecutor`。投射物执行器读取 WeaponRuntime 修正以生成多发、伤害、速度与穿透不同的子弹。`IAttackTrigger` 以 WeaponRuntimeId 区分角色武器；Enemy 的运行时 ID 为 0，复用 EnemyRoot 时重新注册既有 Trigger，不新增组件。
- `AttackSystem.cs`：保存攻击运行时、冷却和同阵营拦截，调用对应执行器。
- `CombatTargetSystem.cs`：维护可攻击实体；`FindClosestCombatTargetQuery` 查询最近敌对目标。

## Architecture/Enemy

- `EnemyConfig.cs`：敌人静态数据，包含内容预制体、基础生命、移速和攻击 ID。
- `EnemyCatalogConfig.cs`：敌人列表及生成间隔、波次、上限等全局参数。
- `EnemyConfigCatalog.cs`：声明 `EnemyCatalog`，加载目录并按 ID 查询敌人。
- `EnemyRootConfig.cs`：绑定通用 `EnemyRoot` 预制体。
- `EnemyModel.cs`：保存当前存活敌人数。
- `EnemyFactory.cs`：创建、配置及按敌人 ID 池化 `EnemyRoot`；跳过跨场景后已销毁的池对象，挂载内容、战斗实体、生命和攻击桥接。
- `EnemySystem.cs`：按时间轴阶段生成敌人、追踪玩家、维护活动列表并回收死亡敌人；在 FixedUpdate 驱动下通过 Rigidbody2D.MovePosition 移动。
- `EnemyHealthSystem.cs`：保存活动敌人的运行时生命；初始化时应用时间轴生命倍率，处理受伤、死亡与回收。

## Architecture/Player 与 Input

- `InputModel.cs` / `InputSystem.cs`：保存并写入归一化移动输入。
- `PlayerModel.cs`：保存玩家注册、位置、角色 ID、生命、死亡和无敌帧状态。
- `PlayerStatModel.cs`：保存本局最大生命、移速、攻击力、无敌帧和经验吸取属性（范围、加速度、最高速度）。
- `PlayerRootConfig.cs`：绑定 `PlayerRoot` 与血条画布预制体。
- `PlayerSpawnSystem.cs`：创建 PlayerRoot，挂载选中角色、血条、CombatEntity，并绑定摄像机和初始技能组；技能组装配失败时注销玩家状态并销毁半初始化根节点。
- `PlayerSystem.cs`：处理注册、移动、受伤、死亡及无敌帧推进；注册状态绑定当前 PlayerRoot 实例，旧对象销毁时不能注销新玩家。
- `StatSystem.cs`：提供玩家最终属性读取接口。
- `DamageSystem.cs`：根据 CombatEntity 阵营，将通用伤害路由给玩家或敌人生命系统。

## Architecture/Run

- `GetRunTimeStateQuery.cs`：只读返回统一逻辑帧、物理帧增量和运行状态；Controller 获取局内时间时使用此 Query。
- `RunModel.cs`：保存局内阶段 `RunPhase`。
- `RunSystem.cs`：开始、暂停、恢复、胜利结束或失败结束一局，并协调统一时间状态。
- `RunTimelineConfig.cs`：定义时间点事件、阶段敌人 ID 与敌人/生成倍率。
- `RunTimerModel.cs`：保存累计时间、当前阶段/倍率，以及统一逻辑帧和物理帧增量。
- `RunTimerSystem.cs`：唯一的局内时间源，推进正计时、应用阶段、发布事件，并在暂停/结束时停止游戏时间。
- `GetRunTimerStateQuery.cs`：只读返回当前计时与倍率快照。

## Architecture/Experience 与 LevelUp

- `ExperienceModel.cs`：保存角色等级、当前经验和下一等级所需经验。
- `ExperienceSystem.cs`：处理经验掉落、捕获、吸取、经验结算并发布等级提升事件。
- `ExperienceDropConfig.cs` / `ExperienceProgressionConfig.cs`：定义经验球和等级经验表的静态数据。
- `ExperienceDropController.cs` / `ExperienceFactory.cs`：提供经验球 Unity 移动桥接和按配置 ID 的对象池复用。
- `LevelUpModel.cs`：保存待处理的升级等级队列。
- `LevelUpSystem.cs`：接收等级事件，管理普通升级与满级进化候选、确认、队列和 `LevelUpSelection` 对局冻结。

## Architecture/Commands、Events 与根架构

- `GameplayCommands.cs`：集中声明开局、计时、刷怪、攻击、伤害、目标注册、敌人生命、玩家移动、角色选择，以及武器升级、Attack 替换、Weapon 替换和进化命令。
- `GameplayEvents.cs`：声明局内、时间轴、角色选择、玩家/敌人受伤死亡，以及武器装备、升级、替换和进化事件。
- `GameArchitecture.cs`：唯一的 QFramework 注册点，注册所有 Model、System 和 Utility。

## Game

- `GameStart.cs`：初始化 ResKit 与 GameArchitecture，打开角色选择/HUD，并逐帧驱动计时、敌人、攻击和无敌帧命令。
- `GameStart.Designer.cs`：QFramework 自动生成的 Bind 文件，禁止手改。
- `CameraFollow`：定义在 `GameStart.cs`，于 LateUpdate 跟随已生成玩家。
- `CombatEntity.cs`：Unity 侧战斗身份桥接，注册/注销可被攻击目标并保存阵营。
- `CollisionAttackTrigger.cs`：碰撞攻击桥接，检测敌对实体并请求执行攻击；角色武器容器替换时按 WeaponRuntimeId 注销。
- `ProjectileAttackTrigger.cs`：远程攻击桥接，查询最近敌对实体并请求执行攻击；角色武器容器替换时按 WeaponRuntimeId 注销。
- `ProjectileController.cs`：子弹飞行、碰撞命中、超时、穿透目标计数与状态重置。
- `ProjectileFactory.cs`：在运行时 `ProjectileContainer` 下创建、获取和回收子弹对象池；获取时跳过跨场景后已销毁的池引用。
- `PlayerController.cs`：桌面移动输入与 Rigidbody2D 桥接，发送移动命令。
- `PlayerHealthBarView.cs`：监听玩家受伤事件并刷新血条 Slider。

## UI

- `UICharacterSelectPanel.cs`：角色选择面板逻辑，创建条目、记录选择并确认开局。
- `UICharacterSelectPanel.Designer.cs`：自动生成 Bind 文件，禁止手改。
- `UICharacterSelectItem.cs`：单个角色条目，通过既定子节点路径显示配置与处理点击。
- `UIGameHUDPanel.cs`：局内 HUD，显示正计时并处理返回操作。
- `UIGameHUDPanel.Designer.cs`：自动生成 Bind 文件，禁止手改。

## Editor

- `ConfigurationCreatorWindow.cs`：菜单 `ProjectSurvivor/Configuration Creator` 对应的编辑器窗口；创建角色或敌人的配置和内容预制体模板，分配下一个数字 ID，并自动把敌人加入 EnemyCatalog。

## 维护规则

- 新攻击添加新的 `IAttackExecutor` 与 `ExecutorId`，禁止恢复 AttackType 枚举或集中分支。
- 静态数值只放 ScriptableObject；局内升级和生命等可变状态只放 Model/System。
- UI 层级、Prefab 结构和 Bind 由用户维护；脚本只能接入既有对象。
- 新增或删除脚本后，同时更新本文和 `Assets/AGENTS.md`。
