# Repository Guidelines

## Project Structure & Module Organization

This directory is the Unity project's `Assets/` root. Put game code in `Scripts/`: gameplay belongs in `Scripts/Game/` and UI panels in `Scripts/UI/`. Scene assets are in `Scenes/`; art content is in `Art/`; render and Unity asset settings are in `Settings/`. `QFramework/` and `QFrameworkData/` contain the installed framework and its generated configuration; do not edit framework sources unless explicitly upgrading it.

Gameplay is organized around QFramework. Keep Unity-facing components and panels as controllers/views; express state in Models, domain behavior in Systems, mutations in Commands, read-only calculations in Queries, and cross-module updates as typed Events. Register application services through an `Architecture` rather than introducing unmanaged global state.

## Build, Test, and Development Commands

Open the Unity project root (the parent of `Assets/`) with the version recorded in `ProjectSettings/ProjectVersion.txt`. Use the Unity Editor to open `Scenes/SampleScene.unity` and enter Play mode for local verification. There is currently no committed CLI build or automated test suite; add Unity Test Framework tests before claiming automated coverage.

## Coding Style & Naming Conventions

Write C# with tabs, four-space visual width, braces on separate lines, and PascalCase for types, methods, public fields, and serialized fields. Use camelCase for parameters and local variables. Keep the `HaoFuSurvivor` namespace for project code. Name UI panels `UI<Name>Panel` and retain QFramework's paired partial files: hand-written behavior goes in `Name.cs`; generated bindings remain in `Name.Designer.cs` and must not be hand-edited.

Always unregister QFramework event subscriptions with the owning GameObject lifecycle, for example `.UnRegisterWhenGameObjectDestroyed(gameObject)`.

## Testing Guidelines

Place new Unity Test Framework tests under `Tests/` with `Editor/` or `PlayMode/` subfolders. Name test fixtures after the subject and tests as `Method_Condition_ExpectedResult`. Cover Commands, Queries, and Systems without scene dependencies where possible; use Play Mode tests for Unity integration.

## Commit & Pull Request Guidelines

No commit history exists yet. Use imperative, scoped messages such as `feat(ui): add pause panel` or `fix(player): stop movement on death`. Keep each commit focused. Pull requests should state the behavioral change, list affected scenes/assets, link the issue when available, and include screenshots or a short capture for visible UI or gameplay changes.

## Persistent Project Context

`AGENTS.md` is the repository's durable handoff document. After every agent-performed operation that changes code, assets, scenes, configuration, architecture decisions, validation status, or known issues, update this file in the same task. Record only material facts: what changed, why, affected paths, verification performed, and unresolved follow-up. Do not overwrite existing history; append a dated entry under **Change Log**. Keep entries concise enough to scan in one pass.

### Current State

- Project type: Unity 2D survivor game; current gameplay code is in `Scripts/Game/`, UI code in `Scripts/UI/`, and the primary scene is `Scenes/SampleScene.unity`.
- Framework decision (2026-07-30): QFramework is the mandatory core architecture for all new and refactored business logic. Enforce Controller -> Command -> System -> Model/Utility dependencies; use Query for reads and typed Event for notifications.
- Existing examples: `Player` controls movement and opens `UIGameOverPanel` when its hurt box is triggered. `*.Designer.cs` files contain generated UI bindings.
- Validation baseline: no committed automated tests or build scripts exist. The Git repository uses `main` and has an initial project commit.

### Change Log

- 2026-07-30 | Added this contributor guide and durable project-context policy. Verified the Assets layout, QFramework installation, gameplay/UI scripts, scenes, and empty Git history.
- 2026-07-30 | Added root `.gitignore` for Unity-generated files, IDE files, and the QFramework package cache. Git remote: `https://github.com/wodemay/HaofuSurvivor.git`; the initial project submission is on `main` and excludes local generated content.
- 2026-07-30 | Created initial project commit `ba5dc5e` (2,392 files). Integrated the remote repository's initial `LICENSE` and `README.md` with merge commit `c813a91`; no conflicts occurred. A push to `origin/main` failed because this environment could not connect to `github.com:443`; retry `git push -u origin main` after network access is restored.
