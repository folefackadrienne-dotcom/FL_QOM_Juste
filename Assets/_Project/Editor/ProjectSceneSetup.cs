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

        private const string UIThemePath = "Assets/_Project/ScriptableObjects/UI/UITheme.asset";

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
            var entitlementManager = root.AddComponent<EntitlementManager>();
            var audioManager = root.AddComponent<AudioManager>();
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
            SetRef(gameManager, "entitlementManager", entitlementManager);
            SetRef(gameManager, "audioManager", audioManager);

            SetRef(ageManager, "contentGateBehaviour", entitlementManager);

            SetRef(buildingManager, "grid", hexGrid);
            SetRef(buildingManager, "resourceManager", resourceManager);
            SetRef(buildingManager, "allianceSystem", allianceSystem);
            SetRef(templeSystem, "resourceManager", resourceManager);

            SetRef(allianceSystem, "resourceManager", resourceManager);
            SetRef(miracleManager, "resourceManager", resourceManager);
            SetRef(miracleManager, "verseManager", verseManager);
            SetRef(miracleManager, "allianceSystem", allianceSystem);
            SetRef(miracleManager, "collectionManager", collectionManager);
            SetRef(miracleManager, "ageManager", ageManager);
            SetRef(verseManager, "resourceManager", resourceManager);
            SetRef(collectionManager, "resourceManager", resourceManager);
            SetRef(missionManager, "resourceManager", resourceManager);
            SetRef(techTree, "resourceManager", resourceManager);

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

            var prayerPanelGO = new GameObject("PrayerMenuPanel", typeof(RectTransform));
            prayerPanelGO.transform.SetParent(hudGO.transform, false);
            AddParchmentBackground(prayerPanelGO, theme);
            var prayerMenu = prayerPanelGO.AddComponent<PrayerMenuUI>();
            var prayerConfirm = CreateButton(prayerPanelGO.transform, "ConfirmButton", "Confirmer", Vector2.zero, theme);
            var prayerListContainer = new GameObject("ListContainer", typeof(RectTransform));
            prayerListContainer.transform.SetParent(prayerPanelGO.transform, false);
            SetRef(prayerMenu, "panelRoot", prayerPanelGO);
            SetRef(prayerMenu, "confirmButton", prayerConfirm);
            SetRef(prayerMenu, "listContainer", prayerListContainer.transform);
            prayerPanelGO.SetActive(false);

            var versePanelGO = new GameObject("VerseJournalPanel", typeof(RectTransform));
            versePanelGO.transform.SetParent(hudGO.transform, false);
            AddParchmentBackground(versePanelGO, theme);
            var verseJournal = versePanelGO.AddComponent<VerseJournalUI>();
            var verseListContainer = new GameObject("ListContainer", typeof(RectTransform));
            verseListContainer.transform.SetParent(versePanelGO.transform, false);
            SetRef(verseJournal, "panelRoot", versePanelGO);
            SetRef(verseJournal, "listContainer", verseListContainer.transform);
            versePanelGO.SetActive(false);

            var prophecyPanelGO = new GameObject("ProphecyJournalPanel", typeof(RectTransform));
            prophecyPanelGO.transform.SetParent(hudGO.transform, false);
            AddParchmentBackground(prophecyPanelGO, theme);
            prophecyPanelGO.SetActive(false);

            SetRef(hud, "resourceBar", resourceBar);
            SetRef(hud, "prayerMenu", prayerMenu);
            SetRef(hud, "verseJournal", verseJournal);
            SetRef(hud, "prophecyJournalPanel", prophecyPanelGO);

            SaveScene(scene, KingdomPath);
        }

        [MenuItem("Kingdom of God/Setup/Create Battle Scene", priority = 13)]
        public static void CreateBattleScene()
        {
            var scene = NewScene();

            var battleCamera = CreateHexGridCamera();
            CreateEventSystem();

            var gridGO = new GameObject("BattleGrid");
            var hexGrid = gridGO.AddComponent<HexGrid>();
            SetInt(hexGrid, "radius", 5);
            var battleGrid = gridGO.AddComponent<BattleGrid>();
            SetRef(battleGrid, "grid", hexGrid);

            var battleManagerGO = new GameObject("BattleManager");
            var battleManager = battleManagerGO.AddComponent<BattleManager>();
            SetRef(battleManager, "battleGrid", battleGrid);

            var battleInput = battleCamera.gameObject.AddComponent<BattleInputController>();
            SetRef(battleInput, "battleManager", battleManager);
            SetRef(battleInput, "battleGrid", battleGrid);
            SetRef(battleInput, "targetCamera", battleCamera);

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
