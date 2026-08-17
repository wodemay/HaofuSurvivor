# Character Package Config Contract

## Required Assets

| Asset | Path | Required links |
| --- | --- | --- |
| CharacterConfig | `Assets/Resources/Configs/Character/Character_<Slug>.asset` | `SkillGroupId`, temporary `PlayerPrefab` |
| SkillConfig | `Assets/Resources/Configs/Combat/Skill/<Slug>/Skill_<Slug>.asset` | `InitialAttackIds` |
| SkillGroupConfig | `Assets/Resources/Configs/Combat/Skill/SkillGroup_<Slug>.asset` | copied weapon IDs, generated Skill and Dodge IDs |
| DodgeConfig | `Assets/Resources/Configs/Combat/Dodge/Dodge_<Slug>.asset` | generic dash values or a separately implemented behavior |
| AttackConfig | `Assets/Resources/Configs/Combat/Skill/<Slug>/Attack_<SkillSlug>.asset` | generated only when the exclusive Skill needs an Attack |

## Catalogs

- `Configs/Combat/Attack/AttackCatalog.asset` owns AttackConfig references.
- `Configs/Combat/Skill/SkillCatalog.asset` owns SkillConfig references.
- `Configs/Combat/Skill/SkillGroupCatalog.asset` owns SkillGroupConfig references.
- `Configs/Combat/Dodge/DodgeCatalog.asset` owns DodgeConfig references.

Update catalogs with Unity serialization APIs. Reject a generated package if an ID is duplicated, a catalog link is missing, or a referenced Attack Executor is unavailable.

## Runtime Wiring

`CharacterConfig.SkillGroupId` resolves to `SkillGroupConfig`. The group retains survivor starting Weapon IDs, adds the generated Skill ID to `StartingSkillIds`, and assigns the generated Dodge ID to `StartingDodgeId`. `PlayerLoadoutSystem` equips the group; `RequestSkillCommand` triggers equipped manual skills from Space; `RequestDodgeCommand` triggers the equipped dodge from Shift.

## Current Limits

`CharacterConfig.PlayerPrefab` cannot be null. Until user-provided art exists, copy the survivor's existing content-prefab reference. Icons may remain null. Do not create UI, PlayerRoot, EnemyRoot, or final art. The standard `DodgeSystem` supports dash movement, cooldown, duration, distance, invulnerability, and data upgrades; special dodge effects require code support before a config can use them.
