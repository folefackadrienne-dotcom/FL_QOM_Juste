# Scenes — setup notes

No `.unity` scene files are checked in yet: hand-authoring Unity's YAML
scene format outside the Editor is fragile, so scenes are created directly
in Unity instead. Suggested first scenes, matching the manager scripts
already in `Assets/_Project/Scripts`:

## `Bootstrap`
Persistent scene loaded first. Contains one `GameManager` GameObject with
the manager components attached and wired in the Inspector:
`AgeManager`, `ResourceManager`, `HexGrid`, `BuildingManager`,
`PopulationSystem`, `AllianceSystem`, `MiracleManager`, `VerseManager`,
`CollectionManager`, `MissionManager`, `SaveManager`. `GameManager` itself
calls `DontDestroyOnLoad`, so this scene stays loaded for the whole session.

## `Kingdom` (territory/management view)
The hex-grid kingdom view. Needs a `HexGrid` component driving the visual
tilemap/mesh, a `BuildingManager`-driven placement controller, and the HUD
(`HUDController` + `ResourceBarUI` + `PrayerMenuUI` + `VerseJournalUI`)
on a Canvas.

## `Battle`
Loaded additively (or as its own scene) per mission. Needs a `BattleGrid`,
a `BattleManager` with its `VictoryCondition` configured for the mission,
and unit prefabs referencing `UnitData` assets.

## `MainMenu`
Entry point: New Game / Continue (via `SaveManager.HasLocalSave()`) /
Mode Libre (once unlocked) / Options.

Once these exist in the Editor, commit the generated `.unity` files (Unity
will also generate matching `.meta` files for every asset in this project —
those should be committed too, since Unity relies on them to keep GUID
references stable).
