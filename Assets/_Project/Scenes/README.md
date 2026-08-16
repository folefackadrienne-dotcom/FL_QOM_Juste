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
`SaveCoordinator`, `EntitlementManager`), all cross-wired, plus
`BootstrapLoader` which loads `MainMenu` on `Start()`. `GameManager.Awake()`
calls `DontDestroyOnLoad`, so this object (and its state) survives every
later scene load.

`SaveCoordinator` is the chain link `SaveManager` never had: `SaveManager`
only reads/writes the JSON file, and until this round nothing ever called
`SaveLocal`/applied a loaded `LoadLocal` result — `MainMenuController.
OnContinue` discarded it. `SaveCoordinator.Capture()`/`Apply()` gather and
reapply every system's state (see the `MainMenu`/`Kingdom` sections below).
`BuildingManager.allBuildingTypes`, `VerseManager.allVerses`,
`CollectionManager.allArtifacts` and `MissionManager.allMissions` are
populated by `ProjectSceneSetup.SetAssetList<T>` — the lookup registries
`SaveCoordinator` needs to match a saved ID string back to its asset.
`AgeManager`'s content gate is wired to `EntitlementManager`, so free-tier
players stop unlocking ages at the GDD's free limit (first 2-3 ages).
Starting resources (Blé 50, Eau 50, Bois 30, Or 20, Foi 10, Sagesse 5,
Justice 10) are pre-filled so the HUD shows real numbers immediately.
`TechTree.allNodes` is populated here too, from every `TechNode` asset
under `Assets/_Project/ScriptableObjects/Techs` — previously left empty,
which made `CanUnlock`/`TryUnlock` unable to ever pass a prerequisite
check regardless of cost.

`LeaderManager.Unlock`/`SetActiveLeader` and `CollectionManager.Collect`
were three more real, callable methods nothing ever called. Both managers
now wire an `ageManager` ref and subscribe to `AgeManager.AgeUnlocked`
themselves: `CollectionManager` collects every `ArtifactData` whose `age`
matches, `LeaderManager` unlocks every `LeaderData` whose `age` matches
*and* has no `unlockMission` set (Abraham/Moïse/Josué/Néhémie). The other
6 leaders (David/Débora/Élie/Gédéon/Salomon/Samson) instead unlock on
`MissionManager.MissionCompleted` matching their new `LeaderData.
unlockMission` reference — wired via a `missionManager` ref — which
needed a first `.meta`/GUID for those 6 `Mission_*.asset` files (none of
the 35 Mission assets had one before this round; Unity would have
auto-assigned GUIDs itself once opened live, but nothing else in this
repo could reference one ahead of that). `LeaderManager.allLeaders` and
`CollectionManager.allArtifacts`/`ageManager` are populated/wired by
`ProjectSceneSetup` the same way as the registries mentioned above.

### `MainMenu`
Camera, EventSystem, Canvas with a full-screen background Image, a title
and two buttons (Nouvelle Partie / Continuer) wired to `MainMenuController`,
which loads `Kingdom` and enables/disables Continuer based on
`SaveManager.HasLocalSave()`. Background, title and buttons are colored
from `UITheme.asset` (`UIThemeData` — see `docs/ArtDirection.md`) when it's
found at `Assets/_Project/ScriptableObjects/UI/UITheme.asset`, falling back
to the previous flat defaults otherwise.

`OnContinue` now actually resumes a saved game: it calls `SaveManager.
LoadLocal()` and passes the result to `SaveCoordinator.Apply()` before
loading `Kingdom` — until this round the loaded `SaveData` was thrown away
and every "Continuer" click silently started over from Awake's defaults.

`MainMenuController.Start()` also calls `AudioManager.PlayVoiceLine`
with `narratorIntro` — loaded from `Voice_NarrateurPrincipal.asset` via
`ProjectSceneSetup.NarratorIntroPath`, same load-by-path pattern as
`UITheme.asset`. `PlayVoiceLine` was a real, callable method with
nothing anywhere calling it; the asset's `lineText` was also empty
(deliberately left as a pure style-reference placeholder) until an
intro line was written and validated with the user. `clipFrench`/
`English`/`Hebrew` are still unassigned — no recording exists yet, so
`PlayVoiceLine` just no-ops silently until one is.

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
gold `Outline`, from `UITheme.asset`). `VerseJournalUI` still has an empty
`ListContainer` child under its panel, wired to `listContainer` — its
`RefreshList()` populates it with one `VerseListItemUI` per memorized
verse, but stays a no-op until a `listItemPrefab` is assigned (no such
prefab exists yet, see `Assets/_Project/Prefabs/`). `PrayerMenuUI` no
longer needs one: it has two child views, `SelectionView` (a
`ListContainer` of runtime-generated `UITheme`-colored buttons, one per
castable `MiracleData` — same code-generated-button pattern as
`BuildingPaletteUI`, `MiracleListItemUI` is gone) and `RitualView`
(`RitualStatusText` + `AccelerateButton` + `AbandonButton`), toggled by
`MiracleManager.IsPraying`; see the `EndTurnButton` paragraph below for the
full ritual wiring. `HexGrid`'s data lives
on the persistent Bootstrap object, this scene doesn't need its own — but
it now has a local `HexGridVisual` GameObject carrying `HexGridRenderer`,
which resolves that same Bootstrap `HexGrid` via `GameManager.Instance` and
renders it: one flat-top hexagon per cell, combined into a couple of
draw-call-cheap meshes per TerrainType present — real variety now, not
just `Plain` everywhere: `HexGrid.GenerateHexagonalMap` calls the new
`TerrainGenerator.Apply` after laying out cells, which classifies each one
from four hand-rolled value-noise channels (elevation/moisture/river/coast
— a random lattice bilinearly interpolated and smoothstepped, deliberately
not `Mathf.PerlinNoise`, so the thresholds below were calibrated against a
Python simulation of the actual output distribution rather than guessed)
plus a per-cell hash for sparse `Ruins`. Seeded (`HexGrid.seed`, `12345` by
default) so the same map reproduces every time within a play session
(`HexGrid` lives on the persistent Bootstrap object, so `Awake` — and thus
generation — only ever runs once; map layout isn't part of `SaveData` yet,
matching placed buildings, which also aren't, so a fresh app launch can
still produce a different map). Two cells are guarded against ever
becoming `Mountain` — the only `TerrainType` `HexCell.IsPassable` treats as
impassable: every cell on the `q == ±radius` column, since
`MissionBattleSetup.SpawnEdge` always places Battle units there, and the
map center `(0, 0)`, the one hardcoded `VictoryConditionType.CapturePoint`
coordinate across all 35 missions
(`Mission_Age5_03_DavidRoiAHebronPuisJerusalem`) — without this an
unlucky map could spawn a unit somewhere it can never move from, or make
that mission's objective permanently unreachable. Each TerrainType layer is either a
flat color from `UIThemeData` (the `Plain` tint follows
`UIThemeData.GetAgeAccent(AgeManager.CurrentAge)`, a hook that existed in
`UITheme.asset` since an earlier round but had no visual consumer until
`HexGridRenderer`) or, when a `TerrainTileSet` asset is wired in and has an
entry for that TerrainType, a real texture — `Assets/_Project/
ScriptableObjects/Grid/TerrainTileSet.asset` currently covers 6 of the 8
types (`Plain`/`Desert`/`Hill`/`River`/`Coast`/`Ruins`; `Mountain`/`Forest`
still fall back to flat color) from illustrations imported into
`Assets/_Project/Art/Sprites/Terrain_*.jpg`. Each hex's hand-built fan mesh
carries UVs (`AppendHex` maps every perimeter vertex to its own position on
the unit circle, remapped to [0,1] — the fan never samples the UV square's
corners, so any black/white padding baked around a source hex illustration
is simply never drawn) and `HexGridRenderer.CreateTexturedMaterial` swaps
the same flat-color shader onto `_BaseMap`/`_MainTex` instead of a solid
color. A darker border ring per tile (always flat-colored, never
textured) and a brighter tile that follows the mouse via the same
ground-plane raycast `KingdomInputController` already used for clicks
round it out — so hovering a cell now shows something, not just resolves
silently. A sibling `MiracleVfx` GameObject carries the new
`MiracleVfxController`: `MiracleManager.PrayerStarted`/`MiracleCast`/
`PrayerCancelled` were already fully wired to SFX (`AudioManager`) but had
no visual consumer, so this builds one `ParticleSystem` entirely in code
(`Resources.GetBuiltinResource<Material>("Default-Particle.mat")` — Unity's
built-in particle material, so no texture needs to be produced first) — a
soft golden glow loops while `MiracleManager.IsPraying`, bursts once on
`MiracleCast`, and stops on `PrayerCancelled`. It's anchored at a fixed
point above the map's world origin rather than any specific unit or
building, since prayer isn't tied to one in either this scene or `Battle`
(where the same component is wired the same way).

A sibling `TempleVisual` GameObject carries the new
`TempleVisualController`: `TempleSystem` was pure data/logic — no
`HexCoordinates`, no `HexGrid` reference, `TempleLevelData.prefab` never
instantiated anywhere — so the Temple itself was completely invisible
regardless of level. Fixed at the map center `(0, 0)`, `HexCoordinates.
ToWorldPosition` there gives its world position; `TerrainGenerator`
already guarantees that exact cell is never `Mountain` (it's the
hardcoded `CapturePoint` objective for
`Mission_Age5_03_DavidRoiAHebronPuisJerusalem`, reused here as the
capital site). A new `HexCell.IsReserved` flag, set on that cell in
`TempleVisualController.Start()`, keeps `BuildingManager.CanPlace` from
ever letting the player build over it — the Temple isn't a
`BuildingInstance` so occupying `HexCell.Building` the way a normal
building does wasn't an option. The placeholder itself (docs/
ArtDirection.md: "imposant, lumineux, couvert d'or et de motifs sacrés")
is a golden `Cylinder` body — color lerped `ochre`→`warmGold` by
`TempleSystem.CurrentLevel` — topped by a 45°-rotated `Cube` "capstone"
in `UITheme.divineLight`, both scaling up with level, plus a
`BillboardLabel` reading "Temple — Niveau N". Rebuilt on every
`TempleSystem.LevelUpgraded`, so upgrading via `TempleUI`'s "Améliorer"
button now visibly changes the map, not just the HUD widget's number.

`BuildingManager.TryPlace`
(Bootstrap) now instantiates `BuildingData.prefab` at the placed cell's
world position via `HexCoordinates.ToWorldPosition` — or, since none of the
39 `BuildingData` assets have a `prefab` assigned, falls through to
`SpawnPlaceholderVisual`: a Unity primitive (`Cube`/`Cylinder`/`Sphere`)
scaled to the hex footprint, shaped/sized/colored by `BuildingCategory` via
the `theme` field now wired onto `BuildingManager` itself (`Habitat` →
short pale cube, `Production` → ochre cylinder, `Military` → tall dark
cube, `Spiritual` → tall gold cylinder, `Special` → blue sphere), plus a
`TextMeshPro` `NameLabel` that stays facing the camera via a new
`BillboardLabel` component. This is scene-agnostic (`BuildingManager` lives
on the persistent Bootstrap `GameManager`, so `CreateBootstrapScene` now
also calls `LoadTheme()` — it didn't before, having no visual concerns of
its own until this). Swapping in real building art later is just assigning
`BuildingData.prefab`; the placeholder branch stops being reached
automatically, no code change needed.

A `BuildingPalettePanel` (parchment `Image` + gold `Outline`, hidden by
default) holds a `ListContainer` populated by `BuildingPaletteUI`: one
runtime-generated `UITheme`-colored button per `BuildingData` under
`Assets/_Project/ScriptableObjects/Buildings` (assigned into
`allBuildings` by `ProjectSceneSetup`, same reasoning as
`BattleHUDController`'s miracle list — no icon art needed to be
functional), filtered to buildings from an unlocked `Age`
(`AgeManager.IsUnlocked`, resolved at runtime via `GameManager.Instance`
like the rest of this scene). A button shows `BuildingData.icon` when one
is assigned — 4 of the 39 buildings (Autel de Pierres, Marché,
Tabernacle, Tente Familiale) have real imported art that just sat on the
asset with nothing ever displaying it; the other 35 stay flat-colored,
still fully usable. Clicking one calls
`KingdomInputController.SelectBuilding` and closes the panel. Outside the
panel, always visible: a `SelectedBuildingLabel` showing "En cours de pose
: <bâtiment>" (or nothing) and a `CancelButton` that clears the selection —
both track `KingdomInputController.BuildingSelected` even while the palette
itself is closed, so an in-progress placement is never silently forgotten.

A `MissionListPanel` (parchment `Image` + gold `Outline`, hidden by
default) holds a `ListContainer` populated by `MissionListUI`: one
runtime-generated `UITheme`-colored button per `MissionData` under
`Assets/_Project/ScriptableObjects/Missions` (all 35, assigned into
`allMissions` by `ProjectSceneSetup` — same reasoning as
`BuildingPaletteUI.allBuildings`), filtered at runtime to an unlocked
`Age` (`AgeManager.IsUnlocked`) that isn't already completed
(`MissionManager.IsCompleted`). Clicking one calls
`MissionManager.StartMission`: for the 8 `MissionType.Battle` missions
that loads the `Battle` scene right away (see that scene's entry below);
for the other 27, across the 5 remaining types, it also opens a second
panel — `MissionResolutionPanel`, holding `MissionResolutionUI` — right
on top of `Kingdom`, no scene change needed. A `CloseButton` on each panel
dismisses it without starting/resolving anything.

`MissionResolutionUI` shows the picked mission's title and summary, then
rebuilds a small `ActionContainer` of runtime-generated buttons (same
`UITheme`-colored pattern as everywhere else in this scene) based on
`MissionData.type`: `Construction` shows the resource cost and a
"Contribuer" button (`MissionManager.TryResolveConstruction`, spends
`constructionCost` via `ResourceManager.TrySpend`); `Survival` shows the
resource requirement and a "Vérifier" button
(`MissionManager.TryResolveSurvival`, checks `survivalRequirement` via
`ResourceManager.CanAfford` without spending anything — failing leaves the
mission active so the player can stock up and try again); `MoralChoice`
and `Diplomacy` each show two buttons labeled from `MissionData.optionA`/
`optionB` (`MissionManager.ResolveMoralChoice` applies the chosen option's
`allianceDelta` via `AllianceSystem.Modify` and grants the mission's base
rewards either way — the weight is spiritual; `MissionManager.
ResolveDiplomacy` instead grants the chosen option's own `rewardOverride`
in place of the base rewards — the weight is practical); `Sandbox` shows a
single unconditional "Terminer" button
(`MissionManager.ResolveSandbox`). All four resolution paths funnel
through a shared `MissionManager.Complete` that marks the mission
completed and grants whichever reward list applies — something that
simply didn't exist before this round for anything but `Battle` missions.

Three more panels share a single layout built by the new
`CreateCodexPanel` helper — a `ListContainer` on the left, a portrait
`Image` plus name/role/detail `TMP_Text` on the right, a `CloseButton`
at the bottom — since `LeaderScreenUI`, `AntagonistCodexUI` and
`CollectionUI` all needed the exact same shape:

- `LeaderScreenPanel` (`LeaderScreenUI`): one button per `LeaderData`
  under `Assets/_Project/ScriptableObjects/Leaders` (all 10, via
  `LeaderManager.AllLeaders`). `LeaderManager.Unlock`/`SetActiveLeader`
  were real, callable methods with nothing calling them — this screen is
  the first caller, on top of `LeaderManager`'s own new auto-unlock (see
  the `Bootstrap` section above). A locked entry's button reads "???
  (Verrouillé)"; selecting it shows `unlockCondition` instead of the
  narrative fields. An unlocked entry shows its portrait (or a flat
  `UITheme` swatch — 4 of the 10 `LeaderData.portrait` fields are still
  unassigned), `gameplayRole`, `narrativeArc`, and an "Activer" button
  that calls `SetActiveLeader` and relabels to "Leader Actif" once it is.
- `AntagonistCodexPanel` (`AntagonistCodexUI`): read-only codex of the 5
  `AntagonistData` assets, revealed by `AgeManager.IsUnlocked` (no
  unlock/collected state of its own — `AntagonistData` never had a
  manager, `BattleManager` only ever reads it to pick a boss SFX cue).
  Shows `role`, `encounterDescription`, `uniqueMechanicName`/
  `uniqueMechanicDescription`, and `victoryCondition`.
- `CollectionPanel` (`CollectionUI`): fiche journal for the 45
  `ArtifactData` assets. `CollectionManager.Collect` was likewise a real
  method nothing called — `CollectionManager` now subscribes to
  `AgeManager.AgeUnlocked` itself and collects every artifact whose
  `age` matches (no new per-artifact authoring needed, `ArtifactData.age`
  already existed). An undiscovered entry reads "??? (Non découvert)";
  a discovered one shows `biblicalReference`/`historicalContext`/
  `educationalComment`/`activeAbilityDescription`, colored by `rarity`
  (Commun→Légendaire mapped onto the existing `UITheme` palette).

A bottom-center `Toolbar` (`HorizontalLayoutGroup`, 6 buttons — Prière /
Versets / Prophétie / Bâtiments / Missions / Sauvegarder) opens
`PrayerMenuUI`/`VerseJournalUI`/`ProphecyJournalPanel`/`BuildingPalettePanel`/
`MissionListPanel` via `HUDController.OpenPrayerMenu`/`OpenVerseJournal`/
`ToggleProphecyJournal`/`OpenBuildingPalette`/`OpenMissionList`. The first
four `HUDController` methods already existed, but until an earlier round
nothing in this scene ever wired a clickable `Button` to any of them. The
6th, "Sauvegarder", calls the new `HUDController.SaveGame` →
`SaveCoordinator.SaveGame` for an explicit save on demand, on top of the
automatic save `EndTurnButton` now also triggers (see below).

A second row, `ToolbarRow2`, sits above the first (3 buttons — Leaders /
Antagonistes / Collection, opening the three panels above): each
`CreateButton` is a fixed 240px wide and the row's own `HorizontalLayoutGroup`
doesn't shrink them (`childControlWidth`/`childForceExpandWidth` both
false), so a 7th button on the first row would have pushed past the
1920px reference canvas — hence the second row rather than a wider one.

`ConfigureListLayout` also fixes a real bug in every `ListContainer` in
this scene, old and new alike: `PrayerMenuUI`'s selection list,
`VerseJournalUI`, `BuildingPaletteUI` and `MissionListUI` never had a
`VerticalLayoutGroup` on their `ListContainer`, so every runtime-generated
item button landed at local `(0,0)` — fully overlapped, not stacked,
regardless of how many entries `RefreshList` created. Mirrors the one
list in the project that already had a `VerticalLayoutGroup`,
`BattleHUDController.miracleListContainer`.

Below the resource bar, a permanent (never closed) `TempleWidget` shows
`TempleUI`: "Temple — Niveau N" plus an "Améliorer" button, interactable
once `TempleSystem.CanUpgrade()` is true. `TempleSystem.levels` — 4 entries
covering levels 2 through 5 — is populated by
`ProjectSceneSetup.SetTempleLevels`; before this round the list was empty,
so `CanUpgrade`/`TryUpgrade` (both pre-existing, real methods) could never
succeed regardless of a UI calling them. Bottom-right, an `EndTurnButton`
("Fin de Tour") calls `HUDController.EndTurn` → `KingdomTurnManager.
EndTurn` — this scene's only source of a turn advancing at all — then
`SaveCoordinator.SaveGame`, auto-saving after every turn, next to a
`TurnLabel` ("Tour N") that updates on `KingdomTurnManager.TurnAdvanced`
and a `PrayerStatusLabel` that tracks an in-progress ritual.
`KingdomTurnManager` itself lives on the persistent Bootstrap `GameManager`
(cross-scene-resolved by `HUDController`/`TempleUI` like every other
manager here) and, on each `EndTurn`, runs `BuildingManager.
ProcessTurnProduction` (scaled by the new `PopulationSystem.
ProductionMultiplier`) followed by population upkeep — both were real,
callable methods with nothing calling them until this round, so every
placed building's `productionPerTurn` was inert regardless of how
carefully it had been costed. A third step, `ApplyGovernanceLoyalty`,
grants a further +1 Loyalty (on top of the upkeep step's own bonus/penalty)
once Justice and Faith stock both clear 0.5 per capita — docs/Economy.md
§3's "Justice et Foi maintiennent la Loyauté," deliberately one-sided (no
penalty for falling short, since Justice-producing buildings don't exist
before Age 2). A fourth step advances an active prayer:
`MiracleManager.BeginPrayer`/`AdvancePrayerTurn`/`AccelerateWithFaith`/
`InterruptPrayer` already existed (used by `Battle`'s `TurnController`),
but `Kingdom` had no turn loop to drive them until `KingdomTurnManager`,
so `PrayerMenuUI` used the instant `TryCast` instead — now that the loop
exists, `EndTurn` calls `AdvancePrayerTurn` whenever
`MiracleManager.IsPraying`. `KingdomTurnManager` also subscribes to
`PopulationSystem.LoyaltyCritical` (already fired by `ModifyLoyalty` in the
rebellion band, previously with no Kingdom-side consumer) and calls
`InterruptPrayer` on it — the territory-management equivalent of Battle's
"an enemy attack can set the prayer gauge back," since there's no enemy
unit on the Kingdom map to do it directly; an `interruptedThisTurn` flag
stops the same turn from also calling `AdvancePrayerTurn` right after.
The `PrayerStatusLabel` mentioned above lives on `HUDController`, not on
`PrayerMenuUI` itself, because `PrayerMenuUI.Close()` disables its own
GameObject (doubling as `panelRoot`), which would also disable
`OnEnable`-subscribed event handlers — the same reasoning already behind
`TurnLabel` living on `HUDController` rather than some per-panel widget.

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
A local `MiracleVfx` GameObject carries the same `MiracleVfxController` as
`Kingdom` (see that scene's entry above for how the particle glow is
built) — `BattleManager.TryCastMiracle` drives the same
`MiracleManager.PrayerStarted`/`MiracleCast`/`PrayerCancelled` events, so
no Battle-specific wiring was needed beyond adding the GameObject.
`VictoryCondition` is left at its default at edit time — `MissionBattleSetup`
(on the same GameObject as `BattleManager`) overrides it at runtime via the
new `BattleManager.Configure`, but only when this scene was reached through
`MissionManager.StartMission`: it reads `GameManager.Instance.Missions.
ActiveMission` in `Start()` and, if one is set, applies that mission's own
`victoryCondition` and spawns `MissionData.playerUnits`/`enemyUnits` along
the grid's west/east edge columns (falling back to 3 copies of
`Unit_Fantassin` per side — wired from `Assets/_Project/ScriptableObjects/
Units/Unit_Fantassin.asset` — when a mission hasn't authored its own
roster, so an empty roster can't auto-win an `AnnihilateEnemy` battle or
soft-lock a `CapturePoint` one). It then listens for
`BattleManager.BattleEnded` and reports back to `MissionManager` —
`CompleteActiveMission` on Victory (grants `MissionData.rewards`),
`FailActiveMission` on Defeat — something nothing did before, since the
outcome panel's return button previously just loaded `Kingdom` without
telling `MissionManager` anything happened. Opening `Battle` directly
(`ActiveMission` unset, e.g. to playtest the scene on its own) leaves
`MissionBattleSetup` a no-op, same as before this round.
`miracleManager` resolves via `GameManager.Instance` like the Kingdom
scene's UI.
`BattleManager.SpawnUnit`/`TryMove`/`OnUnitDied` instantiate, reposition
and destroy `UnitData.prefab` (parented under `BattleGrid`) the same way
`BuildingManager` does for buildings — but no unit has a prefab assigned
yet, so that path falls through to a code-generated placeholder instead
(same UITheme-instead-of-art discipline as `BuildingManager`'s building
placeholders): a primitive shaped by `UnitClass` (`Cube` for Infantry,
`Cylinder` for Archer/Priest/Prophet, `Capsule` for Cavalry, a squat
`Cube` for Chariot, `Sphere` for Special), colored by `Allegiance`
(`theme.deepBlue` for Player, `theme.crisisRed` for Enemy,
`theme.oliveGreen` for the as-yet-unused `Allegiance.Ally`), scaled ×1.5
and recolored to `theme.panelText` for a boss (`UnitData.antagonist`
set), with a `TextMeshPro` name label that stays camera-facing via
`BillboardLabel` — moved from `Buildings/` to `Core/` this round since
it's now shared between `BuildingManager` and `BattleManager`. `theme`
is wired onto `BattleManager` the same way it already was on
`HexGridRenderer` for this scene. See
`Assets/_Project/ScriptableObjects/Units` for the 6 base `UnitData` assets
and the 5 `Unit_Boss*` stat blocks (one per major antagonist, `antagonist`
already linked to their `AntagonistData`) to spawn from once real prefabs exist.
All 9 now have a `.meta` with a fixed GUID (created this round — none
existed before, since nothing had referenced a `UnitData` by GUID from
another `.asset` until `MissionData.playerUnits`/`enemyUnits` needed to).
Three of the 8 `MissionType.Battle` missions use a `Unit_Boss*` directly
(Pharaon → La Mer Rouge, Goliath → David et Goliath, Sennachérib →
Ézéchias et Sennachérib); the other five compose their rosters from the
6 base units.

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
