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
- Treat `CharacterConfig.PlayerPrefab` and `EnemyConfig.Prefab` as visual/content prefabs, not runtime roots.
- Spawn the player through `PlayerRootConfig`: root `PlayerRoot` owns movement/camera; selected content goes under `CharacterRoot`; health canvas goes under `HealthBarAnchor`.
- Spawn enemies through `EnemyRootConfig`: root `EnemyRoot` owns runtime behavior; selected content goes under `CharacterRoot`; roots are parented under scene `EnemyContainer` and reused by `EnemyFactory` pools keyed by enemy ID.
- Configure enemy attacks through `EnemyConfig.AttackIds` and `EnemyAttackCatalog`. The implemented contact attack uses ID 1001; its trigger bridge sends Commands, while EnemyAttackSystem owns cooldown and applies run-scaled damage through DamageSystem. Keep projectile and special attack work as new attack executors.

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
- `EnemyContactAttack` relies on PlayerRoot/HurtBox trigger overlap. EnemyRoot receives a runtime Kinematic Rigidbody2D from EnemyFactory so its pooled root can participate in this callback.
