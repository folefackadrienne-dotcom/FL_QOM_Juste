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
Camera positioned above and behind the grid origin (elevated ~55° RTS angle)
with `HexCameraController` (WASD/arrow-key pan, scroll-wheel zoom) and
`KingdomInputController` attached — left-click resolves through
`HexCoordinates.FromWorldPosition` (a mouse-to-ground-plane raycast against
the mathematical y=0 plane, since no grid mesh/collider exists yet) and
calls `BuildingManager.TryPlace` with `selectedBuilding`, now driven by
`BuildingPaletteUI` rather than a hand-set Inspector field.
`buildingManager`/`grid` resolve at runtime via `GameManager.Instance`, same
cross-scene pattern as the rest of this scene's UI. EventSystem, Canvas with
a full-screen `WorldMoodOverlay` (transparent by default, wired to
`WorldMoodUI` + `UITheme.asset`), `HUDController` and a live resource bar
(one label per resource). Its `ResourceBarUI`/
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
prefab exists yet, see `Assets/_Project/Prefabs/`). `HexGrid`'s data lives
on the persistent Bootstrap object, this scene doesn't need its own — but
it now has a local `HexGridVisual` GameObject carrying `HexGridRenderer`,
which resolves that same Bootstrap `HexGrid` via `GameManager.Instance` and
renders it: one flat-top hexagon per cell, combined into a couple of
draw-call-cheap meshes rather than one GameObject per tile, colored by
`HexCell.TerrainType` (all cells are `Plain` today — no terrain generator
exists yet — tinted by `UIThemeData.GetAgeAccent(AgeManager.CurrentAge)`,
a hook that existed in `UITheme.asset` since an earlier round but had no
visual consumer until now) with a darker border ring per tile and a
brighter tile that follows the mouse via the same ground-plane raycast
`KingdomInputController` already used for clicks — so hovering a cell now
shows something, not just resolves silently. `BuildingManager.TryPlace`
(Bootstrap) now instantiates `BuildingData.prefab` at the placed cell's
world position via `HexCoordinates.ToWorldPosition`, but stays a no-op
until a building has a prefab assigned — none of the 39 `BuildingData`
assets do yet.

A `BuildingPalettePanel` (parchment `Image` + gold `Outline`, hidden by
default) holds a `ListContainer` populated by `BuildingPaletteUI`: one
runtime-generated `UITheme`-colored button per `BuildingData` under
`Assets/_Project/ScriptableObjects/Buildings` (assigned into
`allBuildings` by `ProjectSceneSetup`, same reasoning as
`BattleHUDController`'s miracle list — no icon art needed to be
functional), filtered to buildings from an unlocked `Age`
(`AgeManager.IsUnlocked`, resolved at runtime via `GameManager.Instance`
like the rest of this scene). Clicking one calls
`KingdomInputController.SelectBuilding` and closes the panel. Outside the
panel, always visible: a `SelectedBuildingLabel` showing "En cours de pose
: <bâtiment>" (or nothing) and a `CancelButton` that clears the selection —
both track `KingdomInputController.BuildingSelected` even while the palette
itself is closed, so an in-progress placement is never silently forgotten.

A bottom-center `Toolbar` (`HorizontalLayoutGroup`, 4 buttons — Prière /
Versets / Prophétie / Bâtiments) opens `PrayerMenuUI`/`VerseJournalUI`/
`ProphecyJournalPanel`/`BuildingPalettePanel` via
`HUDController.OpenPrayerMenu`/`OpenVerseJournal`/`ToggleProphecyJournal`/
`OpenBuildingPalette`. Those `HUDController` methods already existed, but
until now nothing in this scene ever wired a clickable `Button` to any of
them — this toolbar is the first one.

### `Battle`
Same elevated camera + `HexCameraController` as `Kingdom`, plus
`BattleInputController` (fully wired locally — `battleManager`/`battleGrid`
live in this same scene, no cross-scene fallback needed here): first
left-click on a cell occupied by a `Allegiance.Player` unit selects it, a
second click resolves through `BattleManager` — an enemy-occupied cell
attacks, an empty one (within movement range) moves, anything else just
deselects; a failed attack/move plays "Interface - Erreur / Action
Impossible". EventSystem, Canvas (this scene previously had none), a
dedicated `BattleGrid`/`HexGrid` pair sized for tactical combat (radius 5,
vs. the kingdom's default 10), and a `BattleManager`. Its own local
`HexGridVisual`/`HexGridRenderer` (same script as `Kingdom`'s, wired
directly to the local `HexGrid` here instead of resolving one via
`GameManager.Instance`) renders the tactical grid the same way — terrain
tiles plus a mouse-following hover tile, both reused by
`BattleInputController` even though the two-click select/attack/move logic
itself is unaffected; it's purely the missing visual feedback that's fixed.
`VictoryCondition` is left at its default and needs to be set per mission;
`miracleManager` resolves via `GameManager.Instance` like the Kingdom
scene's UI.
`BattleManager.SpawnUnit`/`TryMove`/`OnUnitDied` instantiate, reposition
and destroy `UnitData.prefab` (parented under `BattleGrid`) the same way
`BuildingManager` does for buildings — but no unit has a prefab assigned
yet, so it's still a no-op in practice. See
`Assets/_Project/ScriptableObjects/Units` for the 6 base `UnitData` assets
and the 5 `Unit_Boss*` stat blocks (one per major antagonist, `antagonist`
already linked to their `AntagonistData`) to spawn from once prefabs exist.

`BattleHUDController` on the Canvas wires four pieces: a top-left stats
panel that follows `BattleInputController.SelectionChanged`; an "End Tour"
button calling `BattleManager.EndPlayerPhase`; a right-side list of
castable-miracle buttons generated at runtime (not from a prefab — plain
`UITheme`-colored buttons built in code, since the castable set isn't
known until play) calling `BattleManager.TryCastMiracle`, hidden once
`MiracleManager.PrayerStarted` fires (only one miracle per battle); and a
centered Victoire/Défaite panel with a "Retour au Royaume" button
(`SceneManager.LoadScene("Kingdom")`) on `BattleManager.BattleEnded` — an
event that fired into nothing before this. That last part surfaced a real
bug: the method computing it was `CheckVictory` and could only ever set
`Outcome.Victory`, never `Defeat`, despite the enum and event both
supporting it. Renamed `CheckBattleEnd` and now also declares Defeat once
every Player unit that ever spawned this battle is dead.

## After generating

Unity will create `.meta` files for the new `.unity` scene files (and for
any other asset in the project still missing one) the first time it opens
this project — commit those too, since Unity relies on them to keep GUID
references stable.
