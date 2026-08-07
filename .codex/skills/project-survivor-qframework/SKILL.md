---
name: project-survivor-qframework
description: Implement, refactor, diagnose, or document the ProjectSurvivor Unity 2D survivor game. Use for gameplay systems, QFramework architecture, ScriptableObject configuration, player/enemy roots, UI integration, validation, Git handoff, and persistent project-context updates in this repository.
---

# Project Survivor QFramework

Implement this Unity project through QFramework. Read `Assets/AGENTS.md` before acting; it is the durable source of project contracts, current state, known issues, and change history.

## Architecture Rules

- Keep dependencies `Controller/View -> Command -> System -> Model/Utility`.
- Put state in Models, behavior in Systems, mutations in Commands, reads in Queries, and cross-module changes in typed Events.
- Register modules in `GameArchitecture`; do not introduce unmanaged global gameplay state.
- Keep project code in `HaoFuSurvivor`. Use existing naming and formatting conventions.

## Asset and UI Boundary

- The user owns UI panels, prefabs, hierarchy changes, controls, and QFramework Bind generation. If a task needs a new or structurally changed UI object, stop and request that the user creates it first. Then implement only non-UI integration.
- Use ScriptableObject assets in `Resources/Configs/` for static game data. IDs are integers.
- Use Unity menu `ProjectSurvivor/Configuration Creator` to create CharacterConfig or EnemyConfig and optional empty content-prefab templates. The tool assigns the next numeric ID and adds EnemyConfig to EnemyCatalog; never use it to create PlayerRoot, EnemyRoot, UI, or final art.
- Treat `CharacterConfig.PlayerPrefab` and `EnemyConfig.Prefab` as visual/content prefabs, not runtime roots.
- Spawn the player through `PlayerRootConfig`: root `PlayerRoot` owns movement/camera; selected content goes under `CharacterRoot`; health canvas goes under `HealthBarAnchor`.
- Spawn enemies through `EnemyRootConfig`: root `EnemyRoot` owns runtime behavior; selected content goes under `CharacterRoot`; roots are parented under scene `EnemyContainer` and reused by `EnemyFactory` pools keyed by enemy ID. Pool retrieval must skip Unity destroyed-object references after scene changes; Enemy triggers with runtime ID 0 must be reused and re-registered rather than added again.
- Configure all actor attacks through integer attack IDs and `AttackCatalog` in `Resources/Configs/Combat/`. `AttackConfig.ExecutorId` selects a registered executor; never add an attack-type enum or central type branch. `AttackConfig` does not define a target faction: standard attacks derive the opposing faction from their runtime CombatEntity owner. The implemented collision attack uses ID 1001 and executor ID `collision`.
- Projectile attack ID 1002 uses executor ID `projectile`, `ProjectileAttackParameterConfig`, and the user-created `Art/Prefabs/Attack/Bullet.prefab`. It finds the nearest enemy through the CombatTargetSystem query and returns instances to ProjectileFactory's runtime pool, which must skip destroyed pooled references after scene changes. Keep base values in the parameter asset; future upgrades apply runtime modifiers by attack ID.
- CharacterConfig.SkillGroupId resolves through SkillGroupCatalog. A SkillGroupConfig defines starting weapon IDs, skill IDs, and one dodge ID. WeaponConfig is a static container template with InitialAttackIds and level rules; PlayerLoadoutSystem owns distinct WeaponRuntimeData containers for each player weapon. Upgrade, Attack replacement, and evolution mutate only runtime containers. EnemyConfig continues to reference AttackIds directly. Evolution replaces the original runtime slot with a target WeaponConfig that must be level 1 and non-upgradeable.

## Workflow

1. Inspect relevant scripts, assets, scene YAML, and `Assets/AGENTS.md`; preserve unrelated user work in a dirty tree.
2. Implement the smallest QFramework-consistent change. Use `apply_patch` for file edits.
3. Update all impacted configuration and prefab references. Do not silently create or restructure UI.
4. Validate with `dotnet build Assembly-CSharp.csproj --no-restore` when applicable. Report existing MCP/Unity reference warnings separately from new errors. Use Unity Play Mode verification when available.
5. Append a concise dated entry to `Assets/AGENTS.md` in the same task: change, rationale, paths/contracts, validation, and unresolved work.
6. For Git requests, inspect status and remote before staging. Commit focused changes with imperative scoped messages, push explicitly, and record the result in `AGENTS.md`.

## Current Gameplay Contracts

- Run time is elapsed time. `RunTimelineConfig` emits typed stages and holds enemy/spawn multipliers plus stage enemy IDs.
- Character selection persists the numeric selected ID before `StartSelectedCharacterRunCommand` spawns the player and starts the run.
- `EnemySystem` ticks only while the run is active and player is registered; resetting the run releases active roots back to their per-ID pool.
- `EnemyConfig.BaseHealth` is immutable static base health. EnemyFactory registers active EnemyRoots with EnemyHealthSystem, which applies the current timeline health multiplier and returns killed roots through EnemySystem's existing per-ID pool. Current enemy base health is 20; projectile attack 1002 deals 10, so opening enemies require two hits.
- `CollisionAttackTrigger` registers an AttackRuntime and uses generic CombatEntity faction overlap. EnemyRoot and PlayerRoot receive CombatEntity at runtime; EnemyRoot also receives a Kinematic Rigidbody2D for trigger callbacks. Player attack selection/input remains unimplemented, but it will use the same AttackSystem and executors.
- Player damage invulnerability is modeled as `PlayerStatModel.DamageInvulnerabilityDuration` and `PlayerModel.DamageInvulnerabilityRemaining`. The default duration is `0`; future upgrades may raise it, but should keep it short.
