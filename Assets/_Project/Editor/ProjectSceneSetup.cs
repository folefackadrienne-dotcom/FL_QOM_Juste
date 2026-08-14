using System.Collections.Generic;
using System.IO;
using KingdomOfGod.Alliance;
using KingdomOfGod.Audio;
using KingdomOfGod.Battle;
using KingdomOfGod.Buildings;
using KingdomOfGod.Collectibles;
using KingdomOfGod.Core;
using KingdomOfGod.Grid;
using KingdomOfGod.Miracles;
using KingdomOfGod.Missions;
using KingdomOfGod.Monetization;
using KingdomOfGod.Population;
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
            var populationSystem = root.AddComponent<PopulationSystem>();
            var allianceSystem = root.AddComponent<AllianceSystem>();
            var miracleManager = root.AddComponent<MiracleManager>();
            var verseManager = root.AddComponent<VerseManager>();
            var collectionManager = root.AddComponent<CollectionManager>();
            var missionManager = root.AddComponent<MissionManager>();
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
            SetRef(gameManager, "populationSystem", populationSystem);
            SetRef(gameManager, "allianceSystem", allianceSystem);
            SetRef(gameManager, "miracleManager", miracleManager);
            SetRef(gameManager, "verseManager", verseManager);
            SetRef(gameManager, "collectionManager", collectionManager);
            SetRef(gameManager, "missionManager", missionManager);
            SetRef(gameManager, "saveManager", saveManager);
            SetRef(gameManager, "entitlementManager", entitlementManager);
            SetRef(gameManager, "audioManager", audioManager);

            SetRef(ageManager, "contentGateBehaviour", entitlementManager);

            SetRef(buildingManager, "grid", hexGrid);
            SetRef(buildingManager, "resourceManager", resourceManager);
            SetRef(buildingManager, "allianceSystem", allianceSystem);

            SetRef(allianceSystem, "resourceManager", resourceManager);
            SetRef(miracleManager, "resourceManager", resourceManager);
            SetRef(miracleManager, "verseManager", verseManager);
            SetRef(miracleManager, "allianceSystem", allianceSystem);
            SetRef(miracleManager, "collectionManager", collectionManager);
            SetRef(miracleManager, "ageManager", ageManager);
            SetRef(verseManager, "resourceManager", resourceManager);
            SetRef(collectionManager, "resourceManager", resourceManager);
            SetRef(missionManager, "resourceManager", resourceManager);

            SetRef(audioManager, "miracleManager", miracleManager);
            SetRef(audioManager, "allianceSystem", allianceSystem);
            SetRef(audioManager, "buildingManager", buildingManager);
            SetRef(audioManager, "resourceManager", resourceManager);

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

            CreateCamera();
            CreateEventSystem();
            var canvas = CreateCanvas();

            CreateLabel(canvas.transform, "Title", "Kingdom of God", 48, TextAlignmentOptions.Center,
                new Vector2(0, 300), new Vector2(600, 80));

            var newGameButton = CreateButton(canvas.transform, "NewGameButton", "Nouvelle Partie", new Vector2(0, 40));
            var continueButton = CreateButton(canvas.transform, "ContinueButton", "Continuer", new Vector2(0, -30));

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

            CreateCamera();
            CreateEventSystem();
            var canvas = CreateCanvas();

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
                    18, TextAlignmentOptions.MidlineLeft, new Vector2(0, -i * 26), new Vector2(180, 24));
                label.rectTransform.anchorMin = new Vector2(0f, 1f);
                label.rectTransform.anchorMax = new Vector2(0f, 1f);
                label.rectTransform.pivot = new Vector2(0f, 1f);
                resourceLabels.Add(new ResourceLabelRef { type = resourceOrder[i], text = label });
            }
            SetResourceLabels(resourceBar, resourceLabels);

            var prayerPanelGO = new GameObject("PrayerMenuPanel", typeof(RectTransform));
            prayerPanelGO.transform.SetParent(hudGO.transform, false);
            var prayerMenu = prayerPanelGO.AddComponent<PrayerMenuUI>();
            var prayerConfirm = CreateButton(prayerPanelGO.transform, "ConfirmButton", "Confirmer", Vector2.zero);
            SetRef(prayerMenu, "panelRoot", prayerPanelGO);
            SetRef(prayerMenu, "confirmButton", prayerConfirm);
            prayerPanelGO.SetActive(false);

            var versePanelGO = new GameObject("VerseJournalPanel", typeof(RectTransform));
            versePanelGO.transform.SetParent(hudGO.transform, false);
            var verseJournal = versePanelGO.AddComponent<VerseJournalUI>();
            SetRef(verseJournal, "panelRoot", versePanelGO);
            versePanelGO.SetActive(false);

            var prophecyPanelGO = new GameObject("ProphecyJournalPanel", typeof(RectTransform));
            prophecyPanelGO.transform.SetParent(hudGO.transform, false);
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

            CreateCamera();
            CreateEventSystem();

            var gridGO = new GameObject("BattleGrid");
            var hexGrid = gridGO.AddComponent<HexGrid>();
            SetInt(hexGrid, "radius", 5);
            var battleGrid = gridGO.AddComponent<BattleGrid>();
            SetRef(battleGrid, "grid", hexGrid);

            var battleManagerGO = new GameObject("BattleManager");
            var battleManager = battleManagerGO.AddComponent<BattleManager>();
            SetRef(battleManager, "battleGrid", battleGrid);

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

        private static void CreateCamera()
        {
            var cameraGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraGO.tag = "MainCamera";
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
            TextAlignmentOptions alignment, Vector2 anchoredPosition, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            return label;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240, 50);
            rect.anchoredPosition = anchoredPosition;

            var image = go.GetComponent<Image>();
            image.color = new Color(0.15f, 0.1f, 0.05f, 0.9f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            CreateLabel(go.transform, "Text", label, 22, TextAlignmentOptions.Center, Vector2.zero, rect.sizeDelta);

            return button;
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
