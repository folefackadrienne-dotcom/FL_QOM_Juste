using System.Collections.Generic;
using System.IO;
using KingdomOfGod.Alliance;
using KingdomOfGod.Audio;
using KingdomOfGod.Battle;
using KingdomOfGod.Buildings;
using KingdomOfGod.Collectibles;
using KingdomOfGod.Core;
using KingdomOfGod.Grid;
using KingdomOfGod.Interaction;
using KingdomOfGod.Miracles;
using KingdomOfGod.Missions;
using KingdomOfGod.Monetization;
using KingdomOfGod.Population;
using KingdomOfGod.Progression;
using KingdomOfGod.Resources;
using KingdomOfGod.SaveSystem;
using KingdomOfGod.UI;
using KingdomOfGod.Verses;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KingdomOfGod.EditorTools
{
    /// <summary>
    /// One-click scaffolding for the four base scenes (Bootstrap/MainMenu/Kingdom/Battle),
    /// wiring the manager components together the same way a developer would by hand in the
    /// Inspector. Re-running a command overwrites that scene file from scratch — treat these
    /// as regenerable scaffolding, not a place to build permanent hand-authored scene content.
    /// </summary>
    public static class ProjectSceneSetup
    {
        private const string ScenesFolder = "Assets/_Project/Scenes";
        private const string BootstrapPath = ScenesFolder + "/Bootstrap.unity";
        private const string MainMenuPath = ScenesFolder + "/MainMenu.unity";
        private const string KingdomPath = ScenesFolder + "/Kingdom.unity";
        private const string BattlePath = ScenesFolder + "/Battle.unity";

        private const string EditionCompleteProductPath =
            "Assets/_Project/ScriptableObjects/Monetization/Product_EditionComplete.asset";

        private const string NarratorIntroPath =
            "Assets/_Project/ScriptableObjects/Audio/Voice/Voice_NarrateurPrincipal.asset";

        private const string UIThemePath = "Assets/_Project/ScriptableObjects/UI/UITheme.asset";
        private const string TerrainTileSetPath = "Assets/_Project/ScriptableObjects/Grid/TerrainTileSet.asset";

        [MenuItem("Kingdom of God/Setup/Create All Scenes", priority = 0)]
        public static void CreateAllScenes()
        {
            CreateBootstrapScene();
            CreateMainMenuScene();
            CreateKingdomScene();
            CreateBattleScene();
            RegisterBuildScenes();
            Debug.Log("Kingdom of God: Bootstrap, MainMenu, Kingdom and Battle scenes created and registered in Build Settings.");
        }

        [MenuItem("Kingdom of God/Setup/Create Bootstrap Scene", priority = 10)]
        public static void CreateBootstrapScene()
        {
            var scene = NewScene();
            var theme = LoadTheme();

            var root = new GameObject("GameManager");
            var ageManager = root.AddComponent<AgeManager>();
            var resourceManager = root.AddComponent<ResourceManager>();
            var hexGrid = root.AddComponent<HexGrid>();
            var buildingManager = root.AddComponent<BuildingManager>();
            var templeSystem = root.AddComponent<TempleSystem>();
            var populationSystem = root.AddComponent<PopulationSystem>();
            var allianceSystem = root.AddComponent<AllianceSystem>();
            var miracleManager = root.AddComponent<MiracleManager>();
            var verseManager = root.AddComponent<VerseManager>();
            var collectionManager = root.AddComponent<CollectionManager>();
            var missionManager = root.AddComponent<MissionManager>();
            var techTree = root.AddComponent<TechTree>();
            var leaderManager = root.AddComponent<LeaderManager>();
            var saveManager = root.AddComponent<SaveManager>();
            var saveCoordinator = root.AddComponent<SaveCoordinator>();
            var entitlementManager = root.AddComponent<EntitlementManager>();
            var audioManager = root.AddComponent<AudioManager>();
            var kingdomTurnManager = root.AddComponent<KingdomTurnManager>();
            var gameManager = root.AddComponent<GameManager>();
            root.AddComponent<BootstrapLoader>();

            SetStartingResources(resourceManager);

            SetRef(gameManager, "ageManager", ageManager);
            SetRef(gameManager, "resourceManager", resourceManager);
            SetRef(gameManager, "hexGrid", hexGrid);
            SetRef(gameManager, "buildingManager", buildingManager);
            SetRef(gameManager, "templeSystem", templeSystem);
            SetRef(gameManager, "populationSystem", populationSystem);
            SetRef(gameManager, "allianceSystem", allianceSystem);
            SetRef(gameManager, "miracleManager", miracleManager);
            SetRef(gameManager, "verseManager", verseManager);
            SetRef(gameManager, "collectionManager", collectionManager);
            SetRef(gameManager, "missionManager", missionManager);
            SetRef(gameManager, "techTree", techTree);
            SetRef(gameManager, "leaderManager", leaderManager);
            SetRef(gameManager, "saveManager", saveManager);
            SetRef(gameManager, "saveCoordinator", saveCoordinator);
            SetRef(gameManager, "entitlementManager", entitlementManager);
            SetRef(gameManager, "audioManager", audioManager);
            SetRef(gameManager, "kingdomTurnManager", kingdomTurnManager);

            SetRef(ageManager, "contentGateBehaviour", entitlementManager);

            SetRef(buildingManager, "grid", hexGrid);
            SetRef(buildingManager, "resourceManager", resourceManager);
            SetRef(buildingManager, "allianceSystem", allianceSystem);
            SetRef(buildingManager, "populationSystem", populationSystem);
            SetRef(buildingManager, "theme", theme);
            SetAssetList<BuildingData>(buildingManager, "allBuildingTypes", "Assets/_Project/ScriptableObjects/Buildings");
            SetRef(templeSystem, "resourceManager", resourceManager);
            SetTempleLevels(templeSystem);

            SetRef(kingdomTurnManager, "buildingManager", buildingManager);
            SetRef(kingdomTurnManager, "resourceManager", resourceManager);
            SetRef(kingdomTurnManager, "populationSystem", populationSystem);
            SetRef(kingdomTurnManager, "miracleManager", miracleManager);

            SetRef(allianceSystem, "resourceManager", resourceManager);
            SetRef(miracleManager, "resourceManager", resourceManager);
            SetRef(miracleManager, "verseManager", verseManager);
            SetRef(miracleManager, "allianceSystem", allianceSystem);
            SetRef(miracleManager, "collectionManager", collectionManager);
            SetRef(miracleManager, "ageManager", ageManager);
            SetRef(verseManager, "resourceManager", resourceManager);
            SetAssetList<VerseData>(verseManager, "allVerses", "Assets/_Project/ScriptableObjects/Verses");
            SetRef(collectionManager, "resourceManager", resourceManager);
            SetRef(collectionManager, "ageManager", ageManager);
            SetAssetList<ArtifactData>(collectionManager, "allArtifacts", "Assets/_Project/ScriptableObjects/Artifacts");
            SetRef(missionManager, "resourceManager", resourceManager);
            SetRef(missionManager, "allianceSystem", allianceSystem);
            SetAssetList<MissionData>(missionManager, "allMissions", "Assets/_Project/ScriptableObjects/Missions");
            SetRef(techTree, "resourceManager", resourceManager);
            SetTechNodeList(techTree, "Assets/_Project/ScriptableObjects/Techs");
            SetRef(leaderManager, "ageManager", ageManager);
            SetRef(leaderManager, "missionManager", missionManager);
            SetAssetList<LeaderData>(leaderManager, "allLeaders", "Assets/_Project/ScriptableObjects/Leaders");

            SetRef(saveCoordinator, "gameManager", gameManager);

            SetRef(audioManager, "miracleManager", miracleManager);
            SetRef(audioManager, "allianceSystem", allianceSystem);
            SetRef(audioManager, "buildingManager", buildingManager);
            SetRef(audioManager, "resourceManager", resourceManager);
            SetRef(audioManager, "techTree", techTree);
            SetRef(audioManager, "leaderManager", leaderManager);
            SetRef(audioManager, "populationSystem", populationSystem);
            SetRef(audioManager, "templeSystem", templeSystem);
            SetRef(audioManager, "missionManager", missionManager);
            SetRef(audioManager, "verseManager", verseManager);
            SetRef(audioManager, "saveManager", saveManager);
            SetRef(audioManager, "entitlementManager", entitlementManager);
            SetRef(audioManager, "collectionManager", collectionManager);

            var fullEdition = AssetDatabase.LoadAssetAtPath<ProductData>(EditionCompleteProductPath);
            if (fullEdition != null)
            {
                SetRef(entitlementManager, "fullEditionProduct", fullEdition);
            }
            else
            {
                Debug.LogWarning($"Kingdom of God setup: could not find {EditionCompleteProductPath} — EntitlementManager.fullEditionProduct left unassigned.");
            }

            SaveScene(scene, BootstrapPath);
        }

        [MenuItem("Kingdom of God/Setup/Create Main Menu Scene", priority = 11)]
        public static void CreateMainMenuScene()
        {
            var scene = NewScene();
            var theme = LoadTheme();

            CreateCamera();
            CreateEventSystem();
            var canvas = CreateCanvas();

            var backgroundGO = new GameObject("Background", typeof(RectTransform));
            backgroundGO.transform.SetParent(canvas.transform, false);
            var backgroundRect = backgroundGO.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.sizeDelta = Vector2.zero;
            var background = backgroundGO.AddComponent<Image>();
            background.color = theme != null ? theme.deepBlue : new Color(0.05f, 0.05f, 0.08f);

            CreateLabel(canvas.transform, "Title", "Kingdom of God", 48, TextAlignmentOptions.Center,
                new Vector2(0, 300), new Vector2(600, 80), theme != null ? theme.divineLight : (Color?)null);

            var newGameButton = CreateButton(canvas.transform, "NewGameButton", "Nouvelle Partie", new Vector2(0, 40), theme);
            var continueButton = CreateButton(canvas.transform, "ContinueButton", "Continuer", new Vector2(0, -30), theme);

            var controllerGO = new GameObject("MainMenuController");
            controllerGO.transform.SetParent(canvas.transform, false);
            var controller = controllerGO.AddComponent<MainMenuController>();
            SetRef(controller, "continueButton", continueButton);
            SetRef(controller, "narratorIntro", AssetDatabase.LoadAssetAtPath<VoiceLineData>(NarratorIntroPath));

            UnityEventTools.AddPersistentListener(newGameButton.onClick, controller.OnNewGame);
            UnityEventTools.AddPersistentListener(continueButton.onClick, controller.OnContinue);

            SaveScene(scene, MainMenuPath);
        }

        [MenuItem("Kingdom of God/Setup/Create Kingdom Scene", priority = 12)]
        public static void CreateKingdomScene()
        {
            var scene = NewScene();
            var theme = LoadTheme();

            var kingdomCamera = CreateHexGridCamera();
            var kingdomInput = kingdomCamera.gameObject.AddComponent<KingdomInputController>();
            SetRef(kingdomInput, "targetCamera", kingdomCamera);

            var kingdomGridVisualGO = new GameObject("HexGridVisual");
            var kingdomGridVisual = kingdomGridVisualGO.AddComponent<HexGridRenderer>();
            SetRef(kingdomGridVisual, "theme", theme);
            SetRef(kingdomGridVisual, "tileSet", LoadTerrainTileSet());
            SetRef(kingdomGridVisual, "targetCamera", kingdomCamera);

            var kingdomMiracleVfxGO = new GameObject("MiracleVfx");
            var kingdomMiracleVfx = kingdomMiracleVfxGO.AddComponent<MiracleVfxController>();
            SetRef(kingdomMiracleVfx, "theme", theme);

            var kingdomTempleVisualGO = new GameObject("TempleVisual");
            var kingdomTempleVisual = kingdomTempleVisualGO.AddComponent<TempleVisualController>();
            SetRef(kingdomTempleVisual, "theme", theme);

            CreateEventSystem();
            var canvas = CreateCanvas();

            var moodOverlayGO = new GameObject("WorldMoodOverlay", typeof(RectTransform));
            moodOverlayGO.transform.SetParent(canvas.transform, false);
            var moodOverlayRect = moodOverlayGO.GetComponent<RectTransform>();
            moodOverlayRect.anchorMin = Vector2.zero;
            moodOverlayRect.anchorMax = Vector2.one;
            moodOverlayRect.sizeDelta = Vector2.zero;
            var moodOverlay = moodOverlayGO.AddComponent<Image>();
            moodOverlay.color = new Color(0f, 0f, 0f, 0f);
            moodOverlay.raycastTarget = false;
            var worldMood = moodOverlayGO.AddComponent<WorldMoodUI>();
            SetRef(worldMood, "theme", theme);
            SetRef(worldMood, "moodOverlay", moodOverlay);

            var hudGO = new GameObject("HUD", typeof(RectTransform));
            hudGO.transform.SetParent(canvas.transform, false);
            var hud = hudGO.AddComponent<HUDController>();

            var resourceBarGO = new GameObject("ResourceBar", typeof(RectTransform));
            resourceBarGO.transform.SetParent(hudGO.transform, false);
            var resourceBar = resourceBarGO.AddComponent<ResourceBarUI>();

            var resourceOrder = new[]
            {
                ResourceType.Wheat, ResourceType.Water, ResourceType.Wood, ResourceType.Gold,
                ResourceType.Faith, ResourceType.Wisdom, ResourceType.Justice
            };

            var resourceLabels = new List<ResourceLabelRef>();
            for (int i = 0; i < resourceOrder.Length; i++)
            {
                var label = CreateLabel(resourceBarGO.transform, resourceOrder[i] + "Label", $"{resourceOrder[i]}: 0",
                    18, TextAlignmentOptions.MidlineLeft, new Vector2(0, -i * 26), new Vector2(180, 24),
                    theme != null ? theme.ivoryWhite : (Color?)null);
                label.rectTransform.anchorMin = new Vector2(0f, 1f);
                label.rectTransform.anchorMax = new Vector2(0f, 1f);
                label.rectTransform.pivot = new Vector2(0f, 1f);
                resourceLabels.Add(new ResourceLabelRef { type = resourceOrder[i], text = label });
            }
            SetResourceLabels(resourceBar, resourceLabels);

            var templeWidgetGO = new GameObject("TempleWidget", typeof(RectTransform));
            templeWidgetGO.transform.SetParent(hudGO.transform, false);
            var templeUI = templeWidgetGO.AddComponent<TempleUI>();
            var templeLevelLabel = CreateLabel(templeWidgetGO.transform, "LevelText", "Temple — Niveau 1", 16,
                TextAlignmentOptions.MidlineLeft, new Vector2(0f, -resourceOrder.Length * 26f - 10f), new Vector2(220f, 24f),
                theme != null ? theme.ivoryWhite : (Color?)null);
            templeLevelLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
            templeLevelLabel.rectTransform.anchorMax = new Vector2(0f, 1f);
            templeLevelLabel.rectTransform.pivot = new Vector2(0f, 1f);
            var templeUpgradeButton = CreateButton(templeWidgetGO.transform, "UpgradeButton", "Améliorer",
                new Vector2(0f, -resourceOrder.Length * 26f - 40f), theme);
            var templeUpgradeRect = templeUpgradeButton.GetComponent<RectTransform>();
            templeUpgradeRect.anchorMin = new Vector2(0f, 1f);
            templeUpgradeRect.anchorMax = new Vector2(0f, 1f);
            templeUpgradeRect.pivot = new Vector2(0f, 1f);
            templeUpgradeRect.sizeDelta = new Vector2(160f, 34f);
            SetRef(templeUI, "levelText", templeLevelLabel);
            SetRef(templeUI, "upgradeButton", templeUpgradeButton);

            var endTurnButton = CreateButton(canvas.transform, "EndTurnButton", "Fin de Tour", new Vector2(-140f, 40f), theme);
            var endTurnRect = endTurnButton.GetComponent<RectTransform>();
            endTurnRect.anchorMin = new Vector2(1f, 0f);
            endTurnRect.anchorMax = new Vector2(1f, 0f);
            endTurnRect.pivot = new Vector2(1f, 0f);
            UnityEventTools.AddPersistentListener(endTurnButton.onClick, hud.EndTurn);
            var turnLabel = CreateLabel(canvas.transform, "TurnLabel", "Tour 1", 18, TextAlignmentOptions.Center,
                new Vector2(-140f, 80f), new Vector2(220f, 30f), theme != null ? theme.ivoryWhite : (Color?)null);
            turnLabel.rectTransform.anchorMin = new Vector2(1f, 0f);
            turnLabel.rectTransform.anchorMax = new Vector2(1f, 0f);
            turnLabel.rectTransform.pivot = new Vector2(1f, 0f);
            SetRef(hud, "turnLabel", turnLabel);

            var prayerStatusLabel = CreateLabel(canvas.transform, "PrayerStatusLabel", "", 16, TextAlignmentOptions.Center,
                new Vector2(-140f, 112f), new Vector2(260f, 26f), theme != null ? theme.ivoryWhite : (Color?)null);
            prayerStatusLabel.rectTransform.anchorMin = new Vector2(1f, 0f);
            prayerStatusLabel.rectTransform.anchorMax = new Vector2(1f, 0f);
            prayerStatusLabel.rectTransform.pivot = new Vector2(1f, 0f);
            SetRef(hud, "prayerStatusLabel", prayerStatusLabel);

            var prayerPanelGO = new GameObject("PrayerMenuPanel", typeof(RectTransform));
            prayerPanelGO.transform.SetParent(hudGO.transform, false);
            AddParchmentBackground(prayerPanelGO, theme);
            var prayerMenu = prayerPanelGO.AddComponent<PrayerMenuUI>();

            var prayerSelectionGO = new GameObject("SelectionView", typeof(RectTransform));
            prayerSelectionGO.transform.SetParent(prayerPanelGO.transform, false);
            var prayerConfirm = CreateButton(prayerSelectionGO.transform, "ConfirmButton", "Commencer la Prière", Vector2.zero, theme);
            var prayerListContainer = new GameObject("ListContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            prayerListContainer.transform.SetParent(prayerSelectionGO.transform, false);
            ConfigureListLayout(prayerListContainer);

            var prayerRitualGO = new GameObject("RitualView", typeof(RectTransform));
            prayerRitualGO.transform.SetParent(prayerPanelGO.transform, false);
            var prayerRitualStatus = CreateLabel(prayerRitualGO.transform, "RitualStatusText", "Prière en cours", 16,
                TextAlignmentOptions.Center, new Vector2(0f, 40f), new Vector2(260f, 30f), theme != null ? theme.ivoryWhite : (Color?)null);
            var prayerAccelerateButton = CreateButton(prayerRitualGO.transform, "AccelerateButton", "Accélérer (+Foi)", new Vector2(0f, 0f), theme);
            var prayerAbandonButton = CreateButton(prayerRitualGO.transform, "AbandonButton", "Abandonner", new Vector2(0f, -50f), theme);
            prayerRitualGO.SetActive(false);

            SetRef(prayerMenu, "theme", theme);
            SetRef(prayerMenu, "panelRoot", prayerPanelGO);
            SetRef(prayerMenu, "selectionView", prayerSelectionGO);
            SetRef(prayerMenu, "confirmButton", prayerConfirm);
            SetRef(prayerMenu, "listContainer", prayerListContainer.transform);
            SetRef(prayerMenu, "ritualView", prayerRitualGO);
            SetRef(prayerMenu, "ritualStatusText", prayerRitualStatus);
            SetRef(prayerMenu, "accelerateButton", prayerAccelerateButton);
            SetRef(prayerMenu, "abandonButton", prayerAbandonButton);
            UnityEventTools.AddPersistentListener(prayerConfirm.onClick, prayerMenu.ConfirmCast);
            prayerPanelGO.SetActive(false);

            var versePanelGO = new GameObject("VerseJournalPanel", typeof(RectTransform));
            versePanelGO.transform.SetParent(hudGO.transform, false);
            AddParchmentBackground(versePanelGO, theme);
            var verseJournal = versePanelGO.AddComponent<VerseJournalUI>();
            var verseListContainer = new GameObject("ListContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            verseListContainer.transform.SetParent(versePanelGO.transform, false);
            ConfigureListLayout(verseListContainer);
            SetRef(verseJournal, "panelRoot", versePanelGO);
            SetRef(verseJournal, "listContainer", verseListContainer.transform);
            versePanelGO.SetActive(false);

            var prophecyPanelGO = new GameObject("ProphecyJournalPanel", typeof(RectTransform));
            prophecyPanelGO.transform.SetParent(hudGO.transform, false);
            AddParchmentBackground(prophecyPanelGO, theme);
            prophecyPanelGO.SetActive(false);

            var buildingPanelGO = new GameObject("BuildingPalettePanel", typeof(RectTransform));
            buildingPanelGO.transform.SetParent(hudGO.transform, false);
            AddParchmentBackground(buildingPanelGO, theme);
            var buildingPalette = buildingPanelGO.AddComponent<BuildingPaletteUI>();
            var buildingListContainer = new GameObject("ListContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            buildingListContainer.transform.SetParent(buildingPanelGO.transform, false);
            ConfigureListLayout(buildingListContainer);
            var buildingCancelButton = CreateButton(buildingPanelGO.transform, "CancelButton", "Annuler", Vector2.zero, theme);
            SetRef(buildingPalette, "inputController", kingdomInput);
            SetRef(buildingPalette, "theme", theme);
            SetRef(buildingPalette, "panelRoot", buildingPanelGO);
            SetRef(buildingPalette, "listContainer", buildingListContainer.transform);
            SetRef(buildingPalette, "cancelButton", buildingCancelButton);
            SetBuildingList(buildingPalette, "Assets/_Project/ScriptableObjects/Buildings");
            buildingPanelGO.SetActive(false);

            var selectedBuildingLabel = CreateLabel(hudGO.transform, "SelectedBuildingLabel", "", 16,
                TextAlignmentOptions.MidlineLeft, new Vector2(220, 0), new Vector2(360, 24),
                theme != null ? theme.ivoryWhite : (Color?)null);
            selectedBuildingLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
            selectedBuildingLabel.rectTransform.anchorMax = new Vector2(0f, 1f);
            selectedBuildingLabel.rectTransform.pivot = new Vector2(0f, 1f);
            SetRef(buildingPalette, "selectedBuildingLabel", selectedBuildingLabel);

            var resolutionPanelGO = new GameObject("MissionResolutionPanel", typeof(RectTransform));
            resolutionPanelGO.transform.SetParent(hudGO.transform, false);
            AddParchmentBackground(resolutionPanelGO, theme);
            var missionResolution = resolutionPanelGO.AddComponent<MissionResolutionUI>();
            var resolutionTitle = CreateLabel(resolutionPanelGO.transform, "Title", "", 22,
                TextAlignmentOptions.Center, new Vector2(0f, 90f), new Vector2(360f, 30f),
                theme != null ? theme.panelText : (Color?)null);
            var resolutionSummary = CreateLabel(resolutionPanelGO.transform, "Summary", "", 15,
                TextAlignmentOptions.Center, new Vector2(0f, 40f), new Vector2(360f, 60f),
                theme != null ? theme.panelText : (Color?)null);
            var resolutionDetail = CreateLabel(resolutionPanelGO.transform, "Detail", "", 15,
                TextAlignmentOptions.Center, new Vector2(0f, -10f), new Vector2(360f, 40f),
                theme != null ? theme.panelText : (Color?)null);
            var resolutionActions = new GameObject("ActionContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            resolutionActions.transform.SetParent(resolutionPanelGO.transform, false);
            var resolutionActionsRect = resolutionActions.GetComponent<RectTransform>();
            resolutionActionsRect.anchoredPosition = new Vector2(0f, -70f);
            resolutionActionsRect.sizeDelta = new Vector2(280f, 120f);
            var resolutionActionsLayout = resolutionActions.GetComponent<VerticalLayoutGroup>();
            resolutionActionsLayout.spacing = 8f;
            resolutionActionsLayout.childForceExpandHeight = false;
            resolutionActionsLayout.childControlHeight = false;
            resolutionActionsLayout.childAlignment = TextAnchor.UpperCenter;
            var resolutionCloseButton = CreateButton(resolutionPanelGO.transform, "CloseButton", "Fermer", new Vector2(0f, -150f), theme);
            SetRef(missionResolution, "theme", theme);
            SetRef(missionResolution, "panelRoot", resolutionPanelGO);
            SetRef(missionResolution, "titleText", resolutionTitle);
            SetRef(missionResolution, "summaryText", resolutionSummary);
            SetRef(missionResolution, "detailText", resolutionDetail);
            SetRef(missionResolution, "actionContainer", resolutionActions.transform);
            SetRef(missionResolution, "closeButton", resolutionCloseButton);
            resolutionPanelGO.SetActive(false);

            var missionPanelGO = new GameObject("MissionListPanel", typeof(RectTransform));
            missionPanelGO.transform.SetParent(hudGO.transform, false);
            AddParchmentBackground(missionPanelGO, theme);
            var missionList = missionPanelGO.AddComponent<MissionListUI>();
            var missionListContainer = new GameObject("ListContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            missionListContainer.transform.SetParent(missionPanelGO.transform, false);
            ConfigureListLayout(missionListContainer);
            var missionCloseButton = CreateButton(missionPanelGO.transform, "CloseButton", "Fermer", Vector2.zero, theme);
            SetRef(missionList, "theme", theme);
            SetRef(missionList, "panelRoot", missionPanelGO);
            SetRef(missionList, "listContainer", missionListContainer.transform);
            SetRef(missionList, "closeButton", missionCloseButton);
            SetRef(missionList, "resolutionUI", missionResolution);
            SetMissionList(missionList, "Assets/_Project/ScriptableObjects/Missions");
            missionPanelGO.SetActive(false);

            var (leaderPanelGO, leaderListContainer, leaderPortrait, leaderNameText, leaderRoleText, leaderArcText, leaderCloseButton) =
                CreateCodexPanel(hudGO.transform, "LeaderScreenPanel", theme);
            var leaderScreen = leaderPanelGO.AddComponent<LeaderScreenUI>();
            var leaderActivateButton = CreateButton(leaderPanelGO.transform, "ActivateButton", "Activer", new Vector2(140f, -170f), theme);
            SetRef(leaderScreen, "theme", theme);
            SetRef(leaderScreen, "panelRoot", leaderPanelGO);
            SetRef(leaderScreen, "listContainer", leaderListContainer);
            SetRef(leaderScreen, "closeButton", leaderCloseButton);
            SetRef(leaderScreen, "portraitImage", leaderPortrait);
            SetRef(leaderScreen, "nameText", leaderNameText);
            SetRef(leaderScreen, "roleText", leaderRoleText);
            SetRef(leaderScreen, "arcText", leaderArcText);
            SetRef(leaderScreen, "activateButton", leaderActivateButton);
            SetRef(leaderScreen, "activateButtonLabel", leaderActivateButton.GetComponentInChildren<TextMeshProUGUI>());

            var (antagonistPanelGO, antagonistListContainer, antagonistPortrait, antagonistNameText, antagonistRoleText, antagonistDetailText, antagonistCloseButton) =
                CreateCodexPanel(hudGO.transform, "AntagonistCodexPanel", theme);
            var antagonistCodex = antagonistPanelGO.AddComponent<AntagonistCodexUI>();
            SetRef(antagonistCodex, "theme", theme);
            SetRef(antagonistCodex, "panelRoot", antagonistPanelGO);
            SetRef(antagonistCodex, "listContainer", antagonistListContainer);
            SetRef(antagonistCodex, "closeButton", antagonistCloseButton);
            SetRef(antagonistCodex, "portraitImage", antagonistPortrait);
            SetRef(antagonistCodex, "nameText", antagonistNameText);
            SetRef(antagonistCodex, "roleText", antagonistRoleText);
            SetRef(antagonistCodex, "detailText", antagonistDetailText);
            SetAssetList<AntagonistData>(antagonistCodex, "allAntagonists", "Assets/_Project/ScriptableObjects/Antagonists");

            var (collectionPanelGO, collectionListContainer, collectionIcon, collectionNameText, collectionRarityText, collectionDetailText, collectionCloseButton) =
                CreateCodexPanel(hudGO.transform, "CollectionPanel", theme);
            var collectionUI = collectionPanelGO.AddComponent<CollectionUI>();
            SetRef(collectionUI, "theme", theme);
            SetRef(collectionUI, "panelRoot", collectionPanelGO);
            SetRef(collectionUI, "listContainer", collectionListContainer);
            SetRef(collectionUI, "closeButton", collectionCloseButton);
            SetRef(collectionUI, "iconImage", collectionIcon);
            SetRef(collectionUI, "nameText", collectionNameText);
            SetRef(collectionUI, "rarityText", collectionRarityText);
            SetRef(collectionUI, "detailText", collectionDetailText);

            var toolbarGO = new GameObject("Toolbar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            toolbarGO.transform.SetParent(hudGO.transform, false);
            var toolbarRect = toolbarGO.GetComponent<RectTransform>();
            toolbarRect.anchorMin = new Vector2(0.5f, 0f);
            toolbarRect.anchorMax = new Vector2(0.5f, 0f);
            toolbarRect.pivot = new Vector2(0.5f, 0f);
            toolbarRect.anchoredPosition = new Vector2(0f, 20f);
            toolbarRect.sizeDelta = new Vector2(750f, 50f);
            var toolbarLayout = toolbarGO.GetComponent<HorizontalLayoutGroup>();
            toolbarLayout.spacing = 10f;
            toolbarLayout.childForceExpandWidth = false;
            toolbarLayout.childControlWidth = false;

            var prayerToolbarButton = CreateButton(toolbarGO.transform, "PrayerButton", "Prière", Vector2.zero, theme);
            UnityEventTools.AddPersistentListener(prayerToolbarButton.onClick, hud.OpenPrayerMenu);
            var verseToolbarButton = CreateButton(toolbarGO.transform, "VerseButton", "Versets", Vector2.zero, theme);
            UnityEventTools.AddPersistentListener(verseToolbarButton.onClick, hud.OpenVerseJournal);
            var prophecyToolbarButton = CreateButton(toolbarGO.transform, "ProphecyButton", "Prophétie", Vector2.zero, theme);
            UnityEventTools.AddPersistentListener(prophecyToolbarButton.onClick, hud.ToggleProphecyJournal);
            var buildingToolbarButton = CreateButton(toolbarGO.transform, "BuildingButton", "Bâtiments", Vector2.zero, theme);
            UnityEventTools.AddPersistentListener(buildingToolbarButton.onClick, hud.OpenBuildingPalette);
            var missionToolbarButton = CreateButton(toolbarGO.transform, "MissionButton", "Missions", Vector2.zero, theme);
            UnityEventTools.AddPersistentListener(missionToolbarButton.onClick, hud.OpenMissionList);
            var saveToolbarButton = CreateButton(toolbarGO.transform, "SaveButton", "Sauvegarder", Vector2.zero, theme);
            UnityEventTools.AddPersistentListener(saveToolbarButton.onClick, hud.SaveGame);

            // Second row: the first row's 750-wide RectTransform already holds 6 buttons at their
            // fixed 240-wide CreateButton size (childControlWidth/childForceExpandWidth both false,
            // so the HorizontalLayoutGroup doesn't shrink them) — a 7th would push the row past the
            // 1920-wide reference canvas, so Leaders/Antagonistes/Collection get their own row above it.
            var toolbarRow2GO = new GameObject("ToolbarRow2", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            toolbarRow2GO.transform.SetParent(hudGO.transform, false);
            var toolbarRow2Rect = toolbarRow2GO.GetComponent<RectTransform>();
            toolbarRow2Rect.anchorMin = new Vector2(0.5f, 0f);
            toolbarRow2Rect.anchorMax = new Vector2(0.5f, 0f);
            toolbarRow2Rect.pivot = new Vector2(0.5f, 0f);
            toolbarRow2Rect.anchoredPosition = new Vector2(0f, 80f);
            toolbarRow2Rect.sizeDelta = new Vector2(750f, 50f);
            var toolbarRow2Layout = toolbarRow2GO.GetComponent<HorizontalLayoutGroup>();
            toolbarRow2Layout.spacing = 10f;
            toolbarRow2Layout.childForceExpandWidth = false;
            toolbarRow2Layout.childControlWidth = false;

            var leaderToolbarButton = CreateButton(toolbarRow2GO.transform, "LeaderButton", "Leaders", Vector2.zero, theme);
            UnityEventTools.AddPersistentListener(leaderToolbarButton.onClick, hud.OpenLeaderScreen);
            var antagonistToolbarButton = CreateButton(toolbarRow2GO.transform, "AntagonistButton", "Antagonistes", Vector2.zero, theme);
            UnityEventTools.AddPersistentListener(antagonistToolbarButton.onClick, hud.OpenAntagonistCodex);
            var collectionToolbarButton = CreateButton(toolbarRow2GO.transform, "CollectionButton", "Collection", Vector2.zero, theme);
            UnityEventTools.AddPersistentListener(collectionToolbarButton.onClick, hud.OpenCollection);

            SetRef(hud, "resourceBar", resourceBar);
            SetRef(hud, "prayerMenu", prayerMenu);
            SetRef(hud, "verseJournal", verseJournal);
            SetRef(hud, "prophecyJournalPanel", prophecyPanelGO);
            SetRef(hud, "leaderScreen", leaderScreen);
            SetRef(hud, "antagonistCodex", antagonistCodex);
            SetRef(hud, "collectionUI", collectionUI);
            SetRef(hud, "buildingPalette", buildingPalette);
            SetRef(hud, "missionList", missionList);

            SaveScene(scene, KingdomPath);
        }

        [MenuItem("Kingdom of God/Setup/Create Battle Scene", priority = 13)]
        public static void CreateBattleScene()
        {
            var scene = NewScene();
            var theme = LoadTheme();

            var battleCamera = CreateHexGridCamera();
            CreateEventSystem();
            var canvas = CreateCanvas();

            var gridGO = new GameObject("BattleGrid");
            var hexGrid = gridGO.AddComponent<HexGrid>();
            SetInt(hexGrid, "radius", 5);
            var battleGrid = gridGO.AddComponent<BattleGrid>();
            SetRef(battleGrid, "grid", hexGrid);

            var battleGridVisualGO = new GameObject("HexGridVisual");
            var battleGridVisual = battleGridVisualGO.AddComponent<HexGridRenderer>();
            SetRef(battleGridVisual, "grid", hexGrid);
            SetRef(battleGridVisual, "theme", theme);
            SetRef(battleGridVisual, "tileSet", LoadTerrainTileSet());
            SetRef(battleGridVisual, "targetCamera", battleCamera);

            var battleMiracleVfxGO = new GameObject("MiracleVfx");
            var battleMiracleVfx = battleMiracleVfxGO.AddComponent<MiracleVfxController>();
            SetRef(battleMiracleVfx, "theme", theme);

            var battleManagerGO = new GameObject("BattleManager");
            var battleManager = battleManagerGO.AddComponent<BattleManager>();
            SetRef(battleManager, "battleGrid", battleGrid);
            SetRef(battleManager, "theme", theme);

            var missionBattleSetup = battleManagerGO.AddComponent<MissionBattleSetup>();
            SetRef(missionBattleSetup, "battleManager", battleManager);
            SetRef(missionBattleSetup, "battleGrid", battleGrid);
            SetRef(missionBattleSetup, "defaultPlayerUnit", LoadUnit("Unit_Fantassin"));
            SetRef(missionBattleSetup, "defaultEnemyUnit", LoadUnit("Unit_Fantassin"));

            var battleInput = battleCamera.gameObject.AddComponent<BattleInputController>();
            SetRef(battleInput, "battleManager", battleManager);
            SetRef(battleInput, "battleGrid", battleGrid);
            SetRef(battleInput, "targetCamera", battleCamera);

            var unitInfoPanelGO = new GameObject("UnitInfoPanel", typeof(RectTransform));
            unitInfoPanelGO.transform.SetParent(canvas.transform, false);
            var unitInfoRect = unitInfoPanelGO.GetComponent<RectTransform>();
            unitInfoRect.anchorMin = new Vector2(0f, 1f);
            unitInfoRect.anchorMax = new Vector2(0f, 1f);
            unitInfoRect.pivot = new Vector2(0f, 1f);
            unitInfoRect.anchoredPosition = new Vector2(20f, -20f);
            unitInfoRect.sizeDelta = new Vector2(260f, 120f);
            AddParchmentBackground(unitInfoPanelGO, theme);
            var unitInfoText = CreateLabel(unitInfoPanelGO.transform, "Text", "", 16, TextAlignmentOptions.TopLeft,
                Vector2.zero, unitInfoRect.sizeDelta, theme != null ? theme.panelText : (Color?)null);

            var endTurnButton = CreateButton(canvas.transform, "EndTurnButton", "Fin de Tour", new Vector2(-140f, 40f), theme);
            var endTurnRect = endTurnButton.GetComponent<RectTransform>();
            endTurnRect.anchorMin = new Vector2(1f, 0f);
            endTurnRect.anchorMax = new Vector2(1f, 0f);
            endTurnRect.pivot = new Vector2(1f, 0f);

            var miracleListGO = new GameObject("MiracleList", typeof(RectTransform), typeof(VerticalLayoutGroup));
            miracleListGO.transform.SetParent(canvas.transform, false);
            var miracleListRect = miracleListGO.GetComponent<RectTransform>();
            miracleListRect.anchorMin = new Vector2(1f, 1f);
            miracleListRect.anchorMax = new Vector2(1f, 1f);
            miracleListRect.pivot = new Vector2(1f, 1f);
            miracleListRect.anchoredPosition = new Vector2(-20f, -20f);
            miracleListRect.sizeDelta = new Vector2(220f, 400f);
            var miracleListLayout = miracleListGO.GetComponent<VerticalLayoutGroup>();
            miracleListLayout.spacing = 6f;
            miracleListLayout.childForceExpandHeight = false;
            miracleListLayout.childControlHeight = false;

            var outcomePanelGO = new GameObject("OutcomePanel", typeof(RectTransform));
            outcomePanelGO.transform.SetParent(canvas.transform, false);
            var outcomeRect = outcomePanelGO.GetComponent<RectTransform>();
            outcomeRect.anchorMin = new Vector2(0.5f, 0.5f);
            outcomeRect.anchorMax = new Vector2(0.5f, 0.5f);
            outcomeRect.sizeDelta = new Vector2(420f, 220f);
            AddParchmentBackground(outcomePanelGO, theme);
            var outcomeText = CreateLabel(outcomePanelGO.transform, "Text", "", 32, TextAlignmentOptions.Center,
                new Vector2(0f, 30f), new Vector2(380f, 80f), theme != null ? theme.panelText : (Color?)null);
            var outcomeReturnButton = CreateButton(outcomePanelGO.transform, "ReturnButton", "Retour au Royaume", new Vector2(0f, -50f), theme);
            outcomePanelGO.SetActive(false);

            var battleHUD = canvas.gameObject.AddComponent<BattleHUDController>();
            SetRef(battleHUD, "battleManager", battleManager);
            SetRef(battleHUD, "inputController", battleInput);
            SetRef(battleHUD, "theme", theme);
            SetRef(battleHUD, "unitInfoText", unitInfoText);
            SetRef(battleHUD, "endTurnButton", endTurnButton);
            SetRef(battleHUD, "miracleListContainer", miracleListGO.transform);
            SetRef(battleHUD, "outcomePanel", outcomePanelGO);
            SetRef(battleHUD, "outcomeText", outcomeText);
            SetRef(battleHUD, "outcomeReturnButton", outcomeReturnButton);

            SaveScene(scene, BattlePath);
        }

        [MenuItem("Kingdom of God/Setup/Register Scenes in Build Settings", priority = 20)]
        public static void RegisterBuildScenes()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootstrapPath, true),
                new EditorBuildSettingsScene(MainMenuPath, true),
                new EditorBuildSettingsScene(KingdomPath, true),
                new EditorBuildSettingsScene(BattlePath, true),
            };
        }

        // --- scene helpers ---

        private static Scene NewScene() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        private static void SaveScene(Scene scene, string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !AssetDatabase.IsValidFolder(directory))
            {
                Debug.LogError($"Kingdom of God setup: target folder '{directory}' does not exist.");
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"Kingdom of God setup: saved {path}");
        }

        private static Camera CreateCamera()
        {
            var cameraGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGO.tag = "MainCamera";
            return cameraGO.GetComponent<Camera>();
        }

        /// <summary>Elevated RTS-style angle looking down at the grid origin, with HexCameraController attached for WASD pan / scroll zoom.</summary>
        private static Camera CreateHexGridCamera()
        {
            var camera = CreateCamera();
            camera.transform.position = new Vector3(0f, 14f, -10f);
            camera.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            camera.gameObject.AddComponent<HexCameraController>();
            return camera;
        }

        private static void CreateEventSystem()
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static Canvas CreateCanvas()
        {
            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            return canvas;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string name, string text, float fontSize,
            TextAlignmentOptions alignment, Vector2 anchoredPosition, Vector2 size, Color? color = null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = color ?? Color.white;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            return label;
        }

        /// <summary>
        /// Shared list+detail layout for the Leader/Antagonist/Collection screens: a scrollable-
        /// height list on the left (ListContainer, VerticalLayoutGroup via ConfigureListLayout) and
        /// a portrait + name/role/detail text column on the right, closed by a bottom-center button.
        /// All three screens share this exact shape, so it's built once instead of three times.
        /// </summary>
        private static (GameObject panel, Transform listContainer, Image portrait, TextMeshProUGUI nameText,
            TextMeshProUGUI roleText, TextMeshProUGUI detailText, Button closeButton) CreateCodexPanel(
                Transform parent, string panelName, UIThemeData theme)
        {
            var panelGO = new GameObject(panelName, typeof(RectTransform));
            panelGO.transform.SetParent(parent, false);
            var rect = panelGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(760f, 480f);
            AddParchmentBackground(panelGO, theme);

            var listContainerGO = new GameObject("ListContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            listContainerGO.transform.SetParent(panelGO.transform, false);
            var listRect = listContainerGO.GetComponent<RectTransform>();
            listRect.anchoredPosition = new Vector2(-260f, 20f);
            listRect.sizeDelta = new Vector2(220f, 380f);
            ConfigureListLayout(listContainerGO);

            var portraitGO = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            portraitGO.transform.SetParent(panelGO.transform, false);
            var portraitRect = portraitGO.GetComponent<RectTransform>();
            portraitRect.anchoredPosition = new Vector2(140f, 150f);
            portraitRect.sizeDelta = new Vector2(140f, 140f);

            var nameText = CreateLabel(panelGO.transform, "NameText", "", 20, TextAlignmentOptions.Center,
                new Vector2(140f, 50f), new Vector2(340f, 30f), theme != null ? theme.panelText : (Color?)null);
            var roleText = CreateLabel(panelGO.transform, "RoleText", "", 15, TextAlignmentOptions.Center,
                new Vector2(140f, 0f), new Vector2(340f, 50f), theme != null ? theme.panelText : (Color?)null);
            var detailText = CreateLabel(panelGO.transform, "DetailText", "", 13, TextAlignmentOptions.TopLeft,
                new Vector2(140f, -100f), new Vector2(340f, 160f), theme != null ? theme.panelText : (Color?)null);

            var closeButton = CreateButton(panelGO.transform, "CloseButton", "Fermer", new Vector2(0f, -215f), theme);

            panelGO.SetActive(false);
            return (panelGO, listContainerGO.transform, portraitGO.GetComponent<Image>(), nameText, roleText, detailText, closeButton);
        }

        /// <summary>Flat-color chrome per docs/ArtDirection.md section 6 ("bordures dorées fines") — an Outline effect stands in for a real 9-slice border sprite until one exists.</summary>
        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, UIThemeData theme = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240, 50);
            rect.anchoredPosition = anchoredPosition;

            var image = go.GetComponent<Image>();
            image.color = theme != null ? theme.deepBlue : new Color(0.15f, 0.1f, 0.05f, 0.9f);

            if (theme != null)
            {
                var outline = go.AddComponent<Outline>();
                outline.effectColor = theme.goldBorder;
                outline.effectDistance = new Vector2(2f, 2f);
            }

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            CreateLabel(go.transform, "Text", label, 22, TextAlignmentOptions.Center, Vector2.zero, rect.sizeDelta,
                theme != null ? theme.ivoryWhite : (Color?)null);

            return button;
        }

        /// <summary>"Fond semi-transparent en parchemin ou pierre claire" — docs/ArtDirection.md section 6.</summary>
        private static Image AddParchmentBackground(GameObject go, UIThemeData theme)
        {
            var image = go.AddComponent<Image>();
            if (theme != null)
            {
                image.color = theme.parchmentBackground;
                var outline = go.AddComponent<Outline>();
                outline.effectColor = theme.goldBorder;
                outline.effectDistance = new Vector2(2f, 2f);
            }
            return image;
        }

        private static UIThemeData LoadTheme()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeData>(UIThemePath);
            if (theme == null)
            {
                Debug.LogWarning($"Kingdom of God setup: could not find {UIThemePath} — generated UI falls back to default colors.");
            }
            return theme;
        }

        // No warning when missing, unlike LoadTheme: a TerrainTileSet is optional by design —
        // HexGridRenderer already falls back to flat UITheme colors for any TerrainType with no
        // texture entry (or when tileSet itself is null), so an absent asset is a normal state
        // (no terrain art produced yet), not a setup mistake.
        private static TerrainTileSet LoadTerrainTileSet() => AssetDatabase.LoadAssetAtPath<TerrainTileSet>(TerrainTileSetPath);

        private static UnitData LoadUnit(string assetName)
        {
            var path = $"Assets/_Project/ScriptableObjects/Units/{assetName}.asset";
            var unit = AssetDatabase.LoadAssetAtPath<UnitData>(path);
            if (unit == null)
            {
                Debug.LogWarning($"Kingdom of God setup: could not find {path} — MissionBattleSetup's fallback squad will stay empty until it's assigned by hand.");
            }
            return unit;
        }

        private static void SetStartingResources(ResourceManager resourceManager)
        {
            var starting = new (ResourceType type, float amount)[]
            {
                (ResourceType.Wheat, 50f), (ResourceType.Water, 50f), (ResourceType.Wood, 30f),
                (ResourceType.Gold, 20f), (ResourceType.Faith, 10f), (ResourceType.Wisdom, 5f),
                (ResourceType.Justice, 10f)
            };

            var so = new SerializedObject(resourceManager);
            var stockProp = so.FindProperty("startingStock");
            stockProp.arraySize = starting.Length;
            for (int i = 0; i < starting.Length; i++)
            {
                var element = stockProp.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("type").intValue = (int)starting[i].type;
                element.FindPropertyRelative("amount").floatValue = starting[i].amount;
            }
            so.ApplyModifiedProperties();
        }

        private struct ResourceLabelRef
        {
            public ResourceType type;
            public TextMeshProUGUI text;
        }

        private static void SetResourceLabels(ResourceBarUI resourceBar, List<ResourceLabelRef> entries)
        {
            var so = new SerializedObject(resourceBar);
            var labelsProp = so.FindProperty("labels");
            labelsProp.arraySize = entries.Count;
            for (int i = 0; i < entries.Count; i++)
            {
                var element = labelsProp.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("type").intValue = (int)entries[i].type;
                element.FindPropertyRelative("valueText").objectReferenceValue = entries[i].text;
            }
            so.ApplyModifiedProperties();
        }

        /// <summary>
        /// Populates TempleSystem.levels (2 through 5 — level 1 is the free starting level, never
        /// looked up by CanUpgrade/TryUpgrade's GetLevelData(CurrentLevel + 1)). Was an empty list
        /// before this round, which made TryUpgrade/CanUpgrade permanently unreachable regardless
        /// of any UI calling them. Costs escalate in Or/Bois (docs/Economy.md §2: "consomme
        /// beaucoup d'Or, de Bois") plus Justice/Sagesse from level 3 on, and level 5 also asks for
        /// Foi itself — reaching the Faith cap costs some of the Faith it will make room for.
        /// Validated by simulation against the already-authored building economy: reachable by
        /// roughly the midpoint of a 7-age playthrough for an actively-building player.
        /// </summary>
        private static void SetTempleLevels(TempleSystem templeSystem)
        {
            var so = new SerializedObject(templeSystem);
            var levelsProp = so.FindProperty("levels");
            levelsProp.arraySize = 4;

            SetTempleLevel(levelsProp.GetArrayElementAtIndex(0), 2, 20f,
                new ResourceAmount { type = ResourceType.Wood, amount = 40f },
                new ResourceAmount { type = ResourceType.Gold, amount = 60f },
                new ResourceAmount { type = ResourceType.Justice, amount = 10f });

            SetTempleLevel(levelsProp.GetArrayElementAtIndex(1), 3, 30f,
                new ResourceAmount { type = ResourceType.Wood, amount = 60f },
                new ResourceAmount { type = ResourceType.Gold, amount = 90f },
                new ResourceAmount { type = ResourceType.Justice, amount = 20f },
                new ResourceAmount { type = ResourceType.Wisdom, amount = 10f });

            SetTempleLevel(levelsProp.GetArrayElementAtIndex(2), 4, 40f,
                new ResourceAmount { type = ResourceType.Wood, amount = 90f },
                new ResourceAmount { type = ResourceType.Gold, amount = 130f },
                new ResourceAmount { type = ResourceType.Justice, amount = 30f },
                new ResourceAmount { type = ResourceType.Wisdom, amount = 20f });

            SetTempleLevel(levelsProp.GetArrayElementAtIndex(3), 5, 50f,
                new ResourceAmount { type = ResourceType.Wood, amount = 130f },
                new ResourceAmount { type = ResourceType.Gold, amount = 180f },
                new ResourceAmount { type = ResourceType.Justice, amount = 40f },
                new ResourceAmount { type = ResourceType.Wisdom, amount = 30f },
                new ResourceAmount { type = ResourceType.Faith, amount = 50f });

            so.ApplyModifiedProperties();
        }

        private static void SetTempleLevel(SerializedProperty element, int level, float faithCapBonus, params ResourceAmount[] cost)
        {
            element.FindPropertyRelative("level").intValue = level;
            element.FindPropertyRelative("faithCapBonus").floatValue = faithCapBonus;

            var costProp = element.FindPropertyRelative("upgradeCost");
            costProp.arraySize = cost.Length;
            for (int i = 0; i < cost.Length; i++)
            {
                var costElement = costProp.GetArrayElementAtIndex(i);
                costElement.FindPropertyRelative("type").intValue = (int)cost[i].type;
                costElement.FindPropertyRelative("amount").floatValue = cost[i].amount;
            }
        }

        /// <summary>
        /// Populates TechTree.allNodes from every TechNode asset in the given folder. Was never
        /// called before this round — CanUnlock/TryUnlock are real, correct methods (they even
        /// resolve prerequisiteIds against allNodes correctly), but with allNodes empty,
        /// allNodes.Find(prereqId) always returned null, so CanUnlock returned false for every
        /// node that has at least one prerequisite — nearly all of them — regardless of cost.
        /// </summary>
        private static void SetTechNodeList(TechTree tree, string folder)
        {
            var guids = AssetDatabase.FindAssets("t:TechNode", new[] { folder });
            var so = new SerializedObject(tree);
            var listProp = so.FindProperty("allNodes");
            listProp.arraySize = guids.Length;
            for (int i = 0; i < guids.Length; i++)
            {
                var node = AssetDatabase.LoadAssetAtPath<TechNode>(AssetDatabase.GUIDToAssetPath(guids[i]));
                listProp.GetArrayElementAtIndex(i).objectReferenceValue = node;
            }
            so.ApplyModifiedProperties();
        }

        /// <summary>
        /// Every list panel's ListContainer (Prayer/Verse/Building/Mission, plus the new Leader/
        /// Antagonist/Collection ones below) had a plain RectTransform and no layout component, so
        /// every code-generated item button landed at the same (0,0) local position — fully
        /// overlapping, not stacked, regardless of how many entries RefreshList created. Mirrors
        /// BattleHUDController's miracleListContainer, the one list in the project that already had
        /// a VerticalLayoutGroup.
        /// </summary>
        private static void ConfigureListLayout(GameObject listContainer)
        {
            var layout = listContainer.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childForceExpandHeight = false;
            layout.childControlHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;
        }

        /// <summary>
        /// Generic version of SetBuildingList/SetTechNodeList/SetMissionList below, used to populate
        /// the new save/load lookup registries (BuildingManager.allBuildingTypes, VerseManager.
        /// allVerses, CollectionManager.allArtifacts, MissionManager.allMissions) from every asset of
        /// type T in the given folder, so SaveCoordinator.Apply can match a saved ID string back to
        /// its asset without 39+ references dragged in by hand.
        /// </summary>
        private static void SetAssetList<T>(UnityEngine.Object target, string fieldName, string folder) where T : UnityEngine.Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder });
            var so = new SerializedObject(target);
            var listProp = so.FindProperty(fieldName);
            listProp.arraySize = guids.Length;
            for (int i = 0; i < guids.Length; i++)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[i]));
                listProp.GetArrayElementAtIndex(i).objectReferenceValue = asset;
            }
            so.ApplyModifiedProperties();
        }

        /// <summary>Populates BuildingPaletteUI.allBuildings from every BuildingData asset in the given folder, so the palette doesn't need 39 assets dragged in by hand.</summary>
        private static void SetBuildingList(BuildingPaletteUI palette, string folder)
        {
            var guids = AssetDatabase.FindAssets("t:BuildingData", new[] { folder });
            var so = new SerializedObject(palette);
            var listProp = so.FindProperty("allBuildings");
            listProp.arraySize = guids.Length;
            for (int i = 0; i < guids.Length; i++)
            {
                var building = AssetDatabase.LoadAssetAtPath<BuildingData>(AssetDatabase.GUIDToAssetPath(guids[i]));
                listProp.GetArrayElementAtIndex(i).objectReferenceValue = building;
            }
            so.ApplyModifiedProperties();
        }

        /// <summary>Populates MissionListUI.allMissions from every MissionData asset in the given folder, so the list doesn't need 35 assets dragged in by hand — every MissionType now has a real resolution path (Battle scene, or MissionResolutionUI for the other five).</summary>
        private static void SetMissionList(MissionListUI list, string folder)
        {
            var guids = AssetDatabase.FindAssets("t:MissionData", new[] { folder });
            var missions = new List<MissionData>();
            foreach (var guid in guids)
            {
                var mission = AssetDatabase.LoadAssetAtPath<MissionData>(AssetDatabase.GUIDToAssetPath(guid));
                if (mission != null)
                {
                    missions.Add(mission);
                }
            }

            var so = new SerializedObject(list);
            var listProp = so.FindProperty("allMissions");
            listProp.arraySize = missions.Count;
            for (int i = 0; i < missions.Count; i++)
            {
                listProp.GetArrayElementAtIndex(i).objectReferenceValue = missions[i];
            }
            so.ApplyModifiedProperties();
        }

        private static void SetRef(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogError($"Kingdom of God setup: field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }

        private static void SetInt(Object target, string fieldName, int value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop == null)
            {
                Debug.LogError($"Kingdom of God setup: field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }
            prop.intValue = value;
            so.ApplyModifiedProperties();
        }
    }
}
