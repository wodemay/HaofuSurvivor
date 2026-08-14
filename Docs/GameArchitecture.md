# ProjectSurvivor Architecture

## Scope

This document defines ownership boundaries. Runtime call order is documented in `GameLogic.zh-CN.md`; authoring steps live in `Guides/`.

## QFramework Boundary

`GameArchitecture` is the only registration point for business modules.

```text
Controller/View -> Command -> System -> Model/Utility
                         \-> Query/Event
```

- Models hold mutable state; Systems own behavior and lifecycle.
- Commands change state, Queries return read-only data, and Events publish completed facts.
- Unity components only bridge objects, input, rendering, or physics to QFramework.

## Implemented Module Boundaries

| Module | Owns |
| --- | --- |
| Run | phase, logical time, tick dispatch, settlement boundary |
| Player | PlayerRoot registration, movement, health, death |
| Enemy | configuration, spawning, movement, health, pooling |
| Combat | Attack definitions, executors, targeting, damage routing, projectiles |
| Skill | skill groups and runtime Weapon containers |
| Dodge | dodge runtime state, cooldown, movement and upgrades |
| Experience / LevelUp | drops and absorption; level-up queue and selection |
| Save | active-run snapshot serialization and recovery |
| UI | display and input binding; hierarchy and Bind generation remain user-owned |

## Runtime Tick Rule

`GameStart` is the single gameplay `Update` / `FixedUpdate` host. It sends the two root commands to `GameLoopSystem`; systems that need time implement `IRunUpdateable` or `IRunFixedUpdateable` and register only while active. Do not add independent gameplay `MonoBehaviour.Update` or `FixedUpdate` methods.

## Data and Extension Rules

ScriptableObjects hold immutable definitions, Models hold runtime state, and save data contains IDs and values only. Add content through integer IDs and catalogs. Add attack behavior through `ExecutorId`; never introduce an attack-type enum or unmanaged global manager.
