# Scenes — setup notes

No `.unity` scene files are checked into this branch: hand-authoring
Unity's YAML scene format outside the Editor is too fragile to trust
without Unity available to open and verify it. Instead, the four base
scenes are generated automatically by an Editor script.

## One-click setup

Open this project in Unity, then run:

**Kingdom of God → Setup → Create All Scenes**

This builds and saves `Bootstrap.unity`, `MainMenu.unity`, `Kingdom.unity`
and `Battle.unity` under this folder, wires every manager component's
serialized references (the same way you'd drag them in the Inspector), and
registers all four in Build Settings in that order. Individual scenes can
also be (re)created one at a time from the same `Kingdom of God → Setup`
menu. Re-running a command overwrites that scene file from scratch — treat
these as regenerable scaffolding, not a place to build permanent
hand-authored scene content; once you start adding real art/levels to a
scene, stop regenerating it.

The generator script lives at
[`Assets/_Project/Editor/ProjectSceneSetup.cs`](../Editor/ProjectSceneSetup.cs).

## What each scene contains

### `Bootstrap`
Persistent scene, loaded first. One `GameManager` GameObject carrying every
manager component (`AgeManager`, `ResourceManager`, `HexGrid`,
`BuildingManager`, `PopulationSystem`, `AllianceSystem`, `MiracleManager`,
`VerseManager`, `CollectionManager`, `MissionManager`, `SaveManager`,
`EntitlementManager`), all cross-wired, plus `BootstrapLoader` which loads
`MainMenu` on `Start()`. `GameManager.Awake()` calls `DontDestroyOnLoad`, so
this object (and its state) survives every later scene load.
`AgeManager`'s content gate is wired to `EntitlementManager`, so free-tier
players stop unlocking ages at the GDD's free limit (first 2-3 ages).
Starting resources (Blé 50, Eau 50, Bois 30, Or 20, Foi 10, Sagesse 5,
Justice 10) are pre-filled so the HUD shows real numbers immediately.

### `MainMenu`
Camera, EventSystem, Canvas with a full-screen background Image, a title
and two buttons (Nouvelle Partie / Continuer) wired to `MainMenuController`,
which loads `Kingdom` and enables/disables Continuer based on
`SaveManager.HasLocalSave()`. Background, title and buttons are colored
from `UITheme.asset` (`UIThemeData` — see `docs/ArtDirection.md`) when it's
found at `Assets/_Project/ScriptableObjects/UI/UITheme.asset`, falling back
to the previous flat defaults otherwise.

### `Kingdom` (territory/management view)
Camera, EventSystem, Canvas with a full-screen `WorldMoodOverlay` (transparent
by default, wired to `WorldMoodUI` + `UITheme.asset`), `HUDController` and a
live resource bar (one label per resource). Its `ResourceBarUI`/
`PrayerMenuUI`/`VerseJournalUI` leave their manager references unassigned in
the scene — since Bootstrap and Kingdom are separate scenes, those
references resolve at runtime via `GameManager.Instance` instead (Unity
can't serialize cross-scene Inspector references); `WorldMoodUI` does the
same for `AllianceSystem`. `PrayerMenuUI`/`VerseJournalUI`/
`ProphecyJournalPanel` each get a parchment-colored background (`Image` +
gold `Outline`, from `UITheme.asset`) and, for the first two, an empty
`ListContainer` child under their panel, wired to `listContainer` — their
`RefreshList()` populates it with one `MiracleListItemUI`/`VerseListItemUI`
per entry, but stays a no-op until a `listItemPrefab` is assigned (no such
prefab exists yet, see `Assets/_Project/Prefabs/`). Still missing: the
actual hex-grid visual (tilemap or mesh) — `HexGrid`'s data lives on the
persistent Bootstrap object, this scene doesn't need its own.
`BuildingManager.TryPlace` (Bootstrap) now instantiates `BuildingData.prefab`
at the placed cell's world position via `HexCoordinates.ToWorldPosition`,
but stays a no-op until a building has a prefab assigned — none of the 39
`BuildingData` assets do yet.

### `Battle`
Camera, EventSystem, a dedicated `BattleGrid`/`HexGrid` pair sized for
tactical combat (radius 5, vs. the kingdom's default 10), and a
`BattleManager`. `VictoryCondition` is left at its default and needs to be
set per mission; `miracleManager` resolves via `GameManager.Instance` like
the Kingdom scene's UI. No unit prefabs yet — see
`Assets/_Project/ScriptableObjects/Units` for the 6 base `UnitData` assets
and the 5 `Unit_Boss*` stat blocks (one per major antagonist, `antagonist`
already linked to their `AntagonistData`) to spawn from once prefabs exist.

## After generating

Unity will create `.meta` files for the new `.unity` scene files (and for
any other asset in the project still missing one) the first time it opens
this project — commit those too, since Unity relies on them to keep GUID
references stable.
