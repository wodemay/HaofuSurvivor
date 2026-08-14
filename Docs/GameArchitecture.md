# ProjectSurvivor Architecture

## Scope

This document defines architecture boundaries. It does not describe runtime call sequences or asset authoring steps.

## QFramework Boundary

`GameArchitecture` is the only business registration point.

```text
Controller/View -> Command -> System -> Model/Utility
                         \-> Query/Event
```

- Model stores mutable state and does not call Systems.
- System owns domain behavior and lifecycle.
- Command expresses a state-changing request.
- Query returns read-only information.
- Event reports a completed fact.
- Controller and UI bridge Unity objects to QFramework only.

## Implemented Module Boundaries

| Module | Owns |
| --- | --- |
| Run | Run phase, timer coordination, settlement boundary, save lifecycle |
| Player | PlayerRoot registration, movement state, health, death |
| Enemy | Enemy configuration, spawning, movement, health, pooling |
| Combat | Attack definitions, executors, targeting, damage routing |
| Skill | Skill groups, Weapon containers, runtime Attack contents |
| Experience | Experience drops, absorption, level threshold calculation |
| LevelUp | Upgrade queue, option generation, selection phase |
| Save | Active-run snapshot serialization and recovery |
| UI | Display and input binding; user owns hierarchy and Bind generation |

## Data Ownership

- ScriptableObjects hold immutable content definitions.
- Models hold mutable run state.
- Runtime GameObjects are never serialized into save data.
- PlayerPrefs currently stores one active-run JSON snapshot and the selected character ID.

## Extension Rules

New attack behavior is registered through `ExecutorId`. New content is added through integer IDs and catalogs. New business behavior must be placed in a System and entered through a Command or Query; do not add unmanaged global managers.
