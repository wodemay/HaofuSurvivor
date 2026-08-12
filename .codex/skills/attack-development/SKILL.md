---
name: attack-development
description: Design, implement, refactor, diagnose, or document an attack form for ProjectSurvivor when the user describes behavior such as collision, projectile, area, beam, summon, dash, or boss attacks. Use the project's QFramework shared Attack architecture and keep each attack behavior independently implemented through ExecutorId.
---

# Attack Development

Read `Assets/AGENTS.md` and `.codex/skills/project-survivor-qframework/SKILL.md` before acting. This skill implements only the attack request; do not create or restructure UI, actor-root Prefabs, or Bind fields.

## Architecture Contract

Keep the shared chain:

```text
AttackConfig (ID + ExecutorId)
→ AttackCatalog
→ AttackExecutorRegistry
→ IAttackExecutor.ConfigureOwner
→ Unity trigger/controller
→ RegisterAttackCommand / TryExecuteAttackCommand
→ AttackSystem
→ IAttackExecutor.Execute
→ DamageSystem
→ PlayerSystem or EnemyHealthSystem
```

Never add an `AttackType` enum, `if (attackId == ...)` branches, direct health mutation in executors, or a target-faction field in AttackConfig. The owner CombatEntity supplies the faction; AttackSystem rejects same-faction targets.

## Workflow

1. Identify whether the requested attack needs a new behavior or only a new configuration.
   - Existing behavior: create/configure an AttackConfig with the existing ExecutorId and assign its numeric ID to WeaponConfig or EnemyConfig.AttackIds.
   - New behavior: add an `IAttackExecutor` implementation and register it in `AttackExecutorRegistry`.
2. Define only immutable base values in ScriptableObject parameter/config assets. Keep per-run weapon upgrades in `WeaponRuntimeData` modifiers; never mutate assets at runtime.
3. In `ConfigureOwner`, reuse a matching `IAttackTrigger` through `AttackTriggerUtility.Find`; create a component only when no matching trigger exists. Every trigger must register its AttackRuntime in `Initialize`, unregister in `OnDisable`, and carry WeaponRuntimeId.
4. Let triggers obtain targets through `CombatTargetSystem` queries or Unity collision callbacks. They must dispatch Commands, never invoke Systems or damage directly.
5. Make `Execute` perform only the behavior: direct DamageSystem call, ProjectileFactory spawn, area instance, or other runtime effect. Apply enemy timeline damage multiplier here when the owner faction is Enemy.
6. Use `RunTimerModel.DeltaTime` / `FixedDeltaTime` for all time-based behavior, and gate Unity callbacks with `GetRunTimeStateQuery().IsRunning`.
7. Assign the attack to the requested owner:
   - Enemy: add numeric ID to `EnemyConfig.AttackIds`; EnemyFactory configures it at spawn.
   - Player starting weapon: add numeric ID to the appropriate `WeaponConfig.InitialAttackIds`; PlayerLoadoutSystem configures it at loadout equip.
   - Weapon upgrade/evolution: change runtime Attack IDs through WeaponConfig level/evolution data, never edit the running asset.
8. Update the Chinese guide under `Docs/Guides/` and `Docs/Scripts/ScriptReference.zh-CN.md`; append a material change entry to `Assets/AGENTS.md`.

## Existing Executors

- `collision`: `CollisionAttackExecutor` + `CollisionAttackTrigger`; use for overlap/contact attacks. Attack ID 1001 is the reference implementation.
- `projectile`: `ProjectileAttackExecutor` + `ProjectileAttackTrigger` + `ProjectileFactory` + `ProjectileController`; use for target-directed bullets. Attack ID 1002 is the reference implementation.

Read `Docs/Guides/NewAttack.zh-CN.md` for Unity asset/Inspector steps and `Docs/Guides/AttackFlow.zh-CN.md` for the end-to-end runtime path.

## Required Review and Validation

Review changed paths for duplicate triggers on pooled enemies, destroyed pooled references after scene changes, null parameter assets, same-faction hits, cooldown behavior, pause behavior, and WeaponRuntimeId cleanup. Then run:

```powershell
git diff --check
dotnet build Assembly-CSharp.csproj --no-restore --disable-build-servers
```

Use Unity MCP Console and Play Mode validation when available. Preserve user-local `Packages/` MCP changes unless explicitly asked to include them.
