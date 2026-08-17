---
name: character-generation
description: Design and integrate a new playable ProjectSurvivor character from a natural-language brief. Use when the user asks to create a character, exclusive skill, or exclusive dodge and expects automatic ID allocation, Config asset creation, catalog wiring, and a playable loadout without manual Inspector binding.
---

# Character Generation

Create one complete, playable character package. Read `Assets/AGENTS.md`, `project-survivor-qframework`, and `references/config-contract.md` before changing project files.

## Input

Extract or infer these fields from the user's description:

- Character display name, play style, MaxHealth, and MoveSpeed.
- One exclusive Skill: display name, behavior, manual/automatic activation, cooldown, damage, and upgrade intent.
- One exclusive Dodge: display name, behavior, cooldown, duration, distance, invulnerability, and upgrade intent.

Use existing balance defaults when a numeric value is omitted. Do not ask the user to choose IDs, asset paths, catalog slots, initial weapon, icon, or placeholder content prefab. Report those automatic decisions after creation. Ask only when the requested behavior would materially change the intended gameplay and cannot be inferred.

## Package Creation

1. Inspect all current Character, SkillGroup, Skill, Dodge, and Attack catalogs before allocating IDs. Allocate each positive ID as `max(existing IDs) + 1`; never fill apparent gaps or reuse deleted IDs.
2. Implement the Skill behavior through the shared Attack architecture. Use an existing Executor only when it exactly matches; otherwise invoke `$attack-development` to add an isolated Executor and immutable parameter Config. Create its AttackConfig and add it to `AttackCatalog.asset` automatically.
3. Create the Character Config under `Assets/Resources/Configs/Character/`, the Skill and related immutable attack assets under `Assets/Resources/Configs/Combat/Skill/<CharacterSlug>/`, the SkillGroup under `Assets/Resources/Configs/Combat/Skill/`, and the Dodge Config under `Assets/Resources/Configs/Combat/Dodge/`.
4. Add the new Skill, SkillGroup, and Dodge references to their respective catalog assets with `SerializedObject`/`AssetDatabase`; never require Inspector drag-and-drop. Set CharacterConfig.SkillGroupId, SkillGroupConfig.StartingSkillIds, and SkillGroupConfig.StartingDodgeId to the generated IDs.
5. Preserve a playable starting loadout without designing a new Weapon: clone the current survivor SkillGroup's `StartingWeaponIds` and `RequireStartingWeapons` into the new SkillGroup. Reuse the current survivor content prefab as the temporary CharacterConfig.PlayerPrefab and leave Icon unset. Do not create or alter UI, scenes, root prefabs, or final art.
6. Map the current single manual exclusive skill to Space through the existing `RequestSkillCommand` flow. The current Dodge input is Shift. Do not create per-character input bindings without an explicit input-system extension.

## Dodge Boundary

The current `DodgeSystem` implements the generic dash values only. For a standard dodge, generate a `DodgeConfig` with `ExecutorId: dash`. If the requested dodge includes damage, hazards, projectiles, teleport, or another effect, first add the required modular Dodge behavior; do not claim that changing `ExecutorId` alone implements it.

## Automation Rules

Use Unity `AssetDatabase` through the available Unity bridge or a reusable Editor command. Do not hand-write ScriptableObject YAML and do not hand the user a binding checklist. Ensure every generated asset has a valid Unity GUID, every catalog contains exactly one reference to the new asset, and all IDs resolve through their runtime catalogs.

## Verification and Handoff

1. Review the changed code and assets for QFramework boundaries, duplicate catalog entries, null prefab/config references, ID collisions, pause/timer behavior, and skill cleanup on restart.
2. Run `git diff --check` and `dotnet build Assembly-CSharp.csproj --no-restore --disable-build-servers`.
3. Use Unity Console and Play Mode validation when the bridge is available. Verify character selection starts the run, Space uses the exclusive skill, and Shift uses the new dodge.
4. Update the relevant Chinese guides and `Docs/Scripts/ScriptReference.zh-CN.md` only for new architecture or behavior. Append a concise dated `Assets/AGENTS.md` entry with IDs, paths, automatic defaults, and validation.
5. Do not commit, push, or create a PR unless the user explicitly requests Git handoff.
