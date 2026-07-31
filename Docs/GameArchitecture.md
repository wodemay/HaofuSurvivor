# 2D Survivor Game Architecture

## Product Definition

Single-player 2D survivor game for PC and mobile. Players directly operate movement, one primary weapon, dodge, and an unlockable ultimate. Secondary weapons fire automatically. A run builds temporary power through experience upgrades, weapons, passives, and traits. Gold earned from drops, run settlement, and achievements funds permanent out-of-run upgrades.

## QFramework Rules

`GameArchitecture : Architecture<GameArchitecture>` is the sole business entry point. It registers all Models, Systems, and Utilities.

- `Model`: runtime state, persistent state, and observable data only.
- `System`: domain behavior and lifecycle; may access Models and Utilities.
- `Command`: state-changing intent, such as `StartRunCommand`, `ApplyDamageCommand`, or `PurchaseMetaUpgradeCommand`.
- `Query`: read-only calculations, such as `GetFinalStatQuery` or `GetUpgradeCandidatesQuery`.
- `Event`: typed cross-module notification. Events never contain business decisions.
- `Controller`: Unity scene bridge only. It sends Commands, reads Queries, and subscribes with lifecycle-safe unregistration.

Dependency direction is Controller -> Command -> System -> Model/Utility. Models never reference Systems. UI never mutates Models directly.

## Runtime Modules

| Module | QFramework role | Responsibility | Key outputs |
| --- | --- | --- | --- |
| Run | `RunModel`, `RunSystem` | Own run phase: preparation, active, paused, victory, defeat, settlement. | `RunStartedEvent`, `RunEndedEvent` |
| Input | `InputSystem` | Translate keyboard, gamepad, and touch into normalized movement, aim, dodge, primary-fire, and ultimate intents. | input snapshots/commands |
| Player | `PlayerModel`, `PlayerSystem` | Spawn and own player state, health, invulnerability, death, and position registration. | `PlayerDamagedEvent`, `PlayerDiedEvent` |
| Stats | `StatSystem` | Combine base stats, permanent upgrades, run upgrades, equipment, and buffs. | final stat query results |
| Primary Weapon | `PrimaryWeaponSystem` | Manual targeting, fire cadence, projectile/melee execution, and weapon-specific resource use. | `PrimaryWeaponFiredEvent` |
| Auto Weapons | `AutoWeaponSystem` | Equip and autonomously target/fire secondary weapons. | `AutoWeaponFiredEvent` |
| Ultimate | `UltimateSystem` | Unlock state, charge/cooldown, activation, and ultimate effects. | `UltimateUnlockedEvent`, `UltimateCastEvent` |
| Dodge | `DodgeSystem` | Dodge charges, movement burst, invulnerability window, and equipped dodge traits. | `DodgeStartedEvent`, `DodgeEndedEvent` |
| Damage & Effects | `DamageSystem`, `StatusEffectSystem` | Resolve damage, crit, armor, knockback, healing, and timed effects. | `DamageAppliedEvent`, `EntityKilledEvent` |
| Enemy | `EnemySystem` | Enemy runtime state, movement/attack behavior, elite modifiers, death, and pooling hooks. | `EnemySpawnedEvent`, `EnemyKilledEvent` |
| Spawn & Stage | `SpawnSystem`, `StageSystem` | Stage clock, spawn tables, waves, elite/Boss timing, map rules, difficulty, and victory conditions. | `WaveStartedEvent`, `BossSpawnedEvent` |
| Drops | `DropSystem` | Create and collect experience, gold, chests, health, and other run pickups. | `DropCollectedEvent` |
| Experience & Level | `ExperienceSystem`, `LevelSystem` | Experience gain, level thresholds, level-up pause, and upgrade selection application. | `PlayerLeveledUpEvent` |
| Run Build | `BuildModel`, `BuildSystem` | Temporary run inventory: weapons, passives, traits, evolutions, rarity, stacks, and limits. | `BuildChangedEvent` |
| Run Economy | `RunRewardSystem` | Tally run gold and settlement rewards without directly persisting profile data. | `RunGoldChangedEvent`, `RunRewardsCalculatedEvent` |

## Meta and Content Modules

| Module | QFramework role | Responsibility |
| --- | --- | --- |
| Profile & Save | `ProfileModel`, `SaveSystem`, `SaveUtility` | Load, validate, migrate, and atomically save persistent profile data. |
| Meta Progression | `MetaProgressionModel`, `MetaProgressionSystem` | Spend gold on permanent health, attack, crit, experience multiplier/range, armor, move speed, revive, and future special upgrades. |
| Inventory | `InventoryModel`, `InventorySystem` | Track owned currencies, consumables, equipment, and claimable rewards. |
| Unlocks | `UnlockModel`, `UnlockSystem` | Unlock playable characters, primary weapons, ultimates, dodge traits, automatic weapons, maps, and difficulties. |
| Achievements | `AchievementModel`, `AchievementSystem` | Evaluate progress, unlock achievement state, and grant configured gold/rewards exactly once. |
| Codex | `CodexModel`, `CodexSystem` | Record discovered characters, weapons, enemies, traits, stages, and Bosses. |
| Shop | `ShopSystem` | Present purchasable content through Commands; payment is delegated to Inventory and unlock effects to Unlocks/Meta Progression. |
| Content | `ContentUtility` | Read immutable ID-based definitions for characters, weapons, skills, enemies, stages, upgrades, achievements, and rewards. |

## Shared Services

- `PoolSystem`: pools enemies, projectiles, pickups, and visual actors without owning game rules.
- `AudioSystem` and `VfxSystem`: react to typed gameplay events only.
- `LocalizationUtility`: resolves player-facing text by key.
- `RandomUtility`: provides seedable run randomness; the run seed is stored in `RunModel`.
- `AnalyticsSystem`: optional local event sink; it cannot influence game state.

## Data Ownership

| Data | Owner | Lifetime |
| --- | --- | --- |
| Input snapshot, entities, wave clock, build, run rewards | runtime Models | one run |
| Character/weapon/enemy/stage definitions | Content Utility | immutable asset data |
| Gold, permanent upgrades, owned items, achievements, unlocks, codex | Profile-related Models | persistent save |

All definitions are referenced by stable string IDs. Runtime entities retain only their definition ID plus mutable state. Save data stores IDs and values, never Unity scene object references.

## Command, Query, and Event Contract

Examples of write commands: `StartRunCommand`, `MovePlayerCommand`, `FirePrimaryWeaponCommand`, `TryDodgeCommand`, `CastUltimateCommand`, `KillEnemyCommand`, `CollectDropCommand`, `ChooseRunUpgradeCommand`, `EndRunCommand`, `PurchaseMetaUpgradeCommand`, `ClaimAchievementRewardCommand`.

Examples of read queries: `GetFinalStatQuery`, `GetAvailableDodgeQuery`, `GetUpgradeCandidatesQuery`, `GetRunRewardQuery`, `GetMetaUpgradeCostQuery`, `CanUnlockContentQuery`.

Events communicate completed facts. A system that needs a response must issue a Command or Query instead of assuming an event subscriber exists.

## UI Boundary

UI views consume Models, Queries, and Events for display. UI panels, prefabs, hierarchy changes, controls, and generated bindings are created exclusively by the user. Before any task that requires one of those changes, stop and wait for user confirmation. Code integration begins only after the required UI structure and binding names are available.

## Delivery Sequence

1. Add `GameArchitecture`, content definitions, profile/save, stat calculation, and cross-platform input.
2. Implement Player, Run, primary weapon, automatic weapons, Damage, Enemy, Spawn/Stage, Drops, Experience/Level, and Run Build.
3. Add Ultimate, Dodge traits, elites/Bosses, evolutions, and run settlement.
4. Add Inventory, Meta Progression, Unlocks, Achievements, Codex, and Shop.
5. Integrate user-created UI, then add audio, VFX, localization, balancing, and platform-specific polish.

## Verification

- Unit test Commands, Queries, and Systems without scenes where possible.
- Verify stat stacking order, crit/armor damage, dodge invulnerability, primary/automatic weapon cooldowns, and ultimate unlock/cast rules.
- Verify level-up candidate generation, duplicate prevention, run reward calculation, meta upgrade payment, achievement idempotency, and save migration.
- Run Play Mode tests for spawn/wave progression, pickup collection, defeat/victory settlement, pooling, and input parity across PC and touch simulation.
