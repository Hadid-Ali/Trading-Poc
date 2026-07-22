# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

TraderPOC is a Unity mobile-style proof-of-concept app that teaches trading/investing concepts through a
home screen plus three self-contained mini-games (Portfolio Builder, FOMO Firewall, Market Detector).
There is no backend — all state is in-memory/scene-local, and "money" values are read/written by parsing
TextMeshPro label strings (see below).

- Unity Editor version: **6000.3.2f1** (Unity 6), see `ProjectSettings/ProjectVersion.txt`.
- Render pipeline: Universal Render Pipeline (URP).
- Repo root is `F:\Unity\TraderPOC` (this is a standard Unity project folder: `Assets/`, `Packages/`,
  `ProjectSettings/`, etc. live at the root). The `Builds/` folder is git-ignored and used for editor
  output; it is not part of the source tree.

## Working in this repo

This is a Unity project, not a CLI/npm/dotnet-CLI-driven codebase — there is no `package.json`, test
runner script, or CLI build command checked in. Common tasks are performed through the Unity Editor:

- **Open the project**: open `F:\Unity\TraderPOC` in Unity Hub / Unity Editor 6000.3.2f1.
- **Play/test**: press Play in the Editor on `Assets/Scenes/MainMenu.unity` (the main scene with the
  home screen and mini-games). `Assets/Scenes/SampleScene.unity` is the default empty template scene.
- **Build**: use Unity's `File > Build Settings`. Build output is git-ignored (`/[Bb]uilds/`).
- **No automated test suite exists** (`com.unity.test-framework` is an installed package dependency but
  there are no test assemblies/`Tests` folders in `Assets`). There is nothing to run via CLI for
  lint/build/test — verify changes by opening the relevant scene in the Editor and entering Play mode.
- C# project/solution files (`*.csproj`, `TraderPOC.sln`) are Unity-generated; don't hand-edit them.

## Architecture

### Panel-based UI, no scene transitions

The whole app runs in a single scene (`MainMenu.unity`) built as a stack of `GameObject` panels toggled
on/off by **`UiManager`** (`Assets/Scripts/UiManager.cs`). There is no Unity `SceneManager` navigation —
"screens" are just panels under one Canvas that get `SetActive(true/false)`. The expected hierarchy
(documented in a comment at the bottom of `UiManager.cs`) is:

```
Canvas
├── Screen Layer (Home, Trade, Learn, Portfolio, Daily Challenges)
├── Persistent UI (Header, Bottom Navigation)
├── Fullscreen Layer (Get Started, Lesson Detail, Quiz, Complete Lesson)
└── Overlay Layer (Blur, Loading, Reward Popup, Dialog, Toast, etc.)
```

`UiManager` exposes one `Toggle<X>Panel(bool)` method per panel plus `ToggleAllPanels(bool)` /
`HideAllPanels(GameObject)` bulk helpers. When adding a new panel/screen, follow this same pattern:
add a `[SerializeField] GameObject` + a dedicated toggle method, and wire it into the bulk-toggle methods.

### Singletons

`GameManager` and `AudioManager` are both simple `Instance`-based singletons (set in `Awake`, extra
instances self-destroy). `GameManager` currently only tracks the player's virtual balance **as a
formatted string** (e.g. `"$10,450.25"`), read/written via `GetVirtualBalance()`/`SetVirtualBalance()`.
`AudioManager.PlaySFX(SoundType)` plays one-shot SFX clips (`BtnClick`, `Correct`, `Wrong`,
`GameComplete`) — mini-game controllers call this directly on answer/completion events.

### Money/text-parsing pattern (important, easy to break)

Several scripts (`PortfolioBuilderController`, `PBAsset`) treat a `TextMeshProUGUI` label as the source
of truth for a numeric value: they `float.Parse(text.Replace("$","").Replace(",",""))` to read the
current amount, then reformat and write it back (`$"${amount:N2}"`). There is no underlying float/decimal
model — the UI text *is* the state. When modifying money-related UI, preserve this
parse-format-round-trip exactly (currency symbol + thousands separators), or the parsing will throw.

### Mini-games

Each mini-game is a self-contained controller + ScriptableObject data pattern, driven by `UiManager` to
return to the home panel on completion:

- **Portfolio Builder** (`PortfolioBuilderController` + `PBAsset` + `PBEventsDataSO`): player allocates
  virtual money across `AssetType` (Apple, Tesla, Bitcoin, Gold, Oil, Bonds, ETF) assets via `PBAsset`
  buttons, which broadcast changes through the static event
  `PortfolioBuilderController.OnInvestedAmountChanged`. Each round shows an `EventData` (scripted market
  event with correct `AssetDirection`s); submitting compares the player's up/down allocation deltas
  against the event's correct directions.
- **FOMO Firewall** (`FomoFirewallController` + `FomoFirewallSO`): linear scenario → question → multiple
  choice (`FomoOption`) → result flow driven by a `List<FomoFirewallSO>`, advancing `currentScenario`
  until the list is exhausted.
- **Market Detector** (`MarketDetectorControllor` + `MarketPredictionSO` + `Prediction`/`PredictionType`
  enums): player picks up/down predictions per asset category; correctness is checked by intersecting
  `userPredictions` against the scenario's `CorrectPredictions`.

All three follow the same shape: a `List<...SO>` of scenario data, an index cursor, `LoadScenario`/
`ShowResult`/`OnNext`/`OnRetry`/`ResetGame` methods, and `HideAll()` toggling child screens rather than
using `UiManager` internally (only `OnContinue`/`OnGameComplete` calls back into `UiManager` to return to
Home). When adding a new mini-game, mirror this structure rather than inventing a new flow-control shape.

### Animation & audio

DOTween (`Assets/Plugins/Demigiant/DOTween*`, imported as an Asset Store plugin, *not* a Package Manager
dependency — it won't appear in `Packages/manifest.json`) is the standard for UI tweens (`DOScale`,
`DOAnchorPosY`, etc.) throughout the mini-game result/option panels. `UIAnimationUtility` is a
frame-by-frame sprite-sequence animator (distinct from DOTween, used for icon-style flip animations).
`UISoundTrigger` wires button clicks to `AudioManager.PlaySFX`.

### Fonts/UI assets

TextMesh Pro font assets and several other `.asset`/scene files show as modified in git status frequently
due to Unity re-serializing GUIDs/metadata on open — don't assume every diff in `TextMesh Pro/`,
`UniversalRP.asset`, or `UniversalRenderPipelineGlobalSettings.asset` reflects an intentional change;
check with `git diff` before treating them as part of a feature change.
