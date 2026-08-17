using System.Collections.Generic;
using KingdomOfGod.Buildings;
using KingdomOfGod.Core;
using KingdomOfGod.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>
    /// Building selection palette for the Kingdom scene: one runtime-generated button per
    /// BuildingData (populated into allBuildings by ProjectSceneSetup from
    /// Assets/_Project/ScriptableObjects/Buildings/ — no manual drag needed, same reasoning as
    /// BattleHUDController's miracle list: flat UITheme-colored buttons, no icon art needed to be
    /// functional), filtered to buildings from an Age the player has actually unlocked. A button
    /// shows BuildingData.icon when one is assigned (14 of the 39 buildings have real art so far —
    /// the rest stay flat-colored, still fully usable) — the icon was imported and sitting on the
    /// asset with nothing ever displaying it. Selecting a button calls KingdomInputController.
    /// SelectBuilding and closes the panel; a persistent label and cancel button (both outside
    /// panelRoot, always visible) track/clear the current selection so it isn't forgotten once the
    /// palette closes.
    /// </summary>
    public class BuildingPaletteUI : MonoBehaviour
    {
        [SerializeField] private KingdomInputController inputController;
        [SerializeField] private AgeManager ageManager;
        [SerializeField] private UIThemeData theme;

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform listContainer;
        [SerializeField] private TMP_Text selectedBuildingLabel;
        [SerializeField] private Button cancelButton;

        [SerializeField] private List<BuildingData> allBuildings = new List<BuildingData>();

        private void Awake()
        {
            // ageManager lives on the persistent Bootstrap GameManager, in a different scene
            // from this palette — Inspector references can't cross scenes, so fall back to the
            // running singleton when this field was left unassigned.
            if (ageManager == null && GameManager.Instance != null)
            {
                ageManager = GameManager.Instance.Ages;
            }
        }

        private void OnEnable()
        {
            if (inputController != null) inputController.BuildingSelected += OnBuildingSelected;
            if (cancelButton != null) cancelButton.onClick.AddListener(OnCancelClicked);

            OnBuildingSelected(inputController != null ? inputController.SelectedBuilding : null);
        }

        private void OnDisable()
        {
            if (inputController != null) inputController.BuildingSelected -= OnBuildingSelected;
            if (cancelButton != null) cancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        public void Open()
        {
            panelRoot.SetActive(true);
            RefreshList();
            GameManager.Instance?.Audio.PlaySfx("Interface - Ouverture de Menu");
        }

        public void Close()
        {
            panelRoot.SetActive(false);
            GameManager.Instance?.Audio.PlaySfx("Interface - Fermeture de Menu");
        }

        private void OnCancelClicked() => inputController.SelectBuilding(null);

        private void OnBuildingSelected(BuildingData building)
        {
            if (selectedBuildingLabel == null) return;

            selectedBuildingLabel.text = building != null
                ? $"En cours de pose : {building.displayName}"
                : string.Empty;
        }

        private void RefreshList()
        {
            if (listContainer == null) return;

            for (int i = listContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(listContainer.GetChild(i).gameObject);
            }

            foreach (var building in allBuildings)
            {
                if (building == null) continue;
                if (ageManager != null && !ageManager.IsUnlocked(building.introducedInAge)) continue;

                CreateBuildingButton(building);
            }
        }

        private void CreateBuildingButton(BuildingData building)
        {
            var go = new GameObject(building.displayName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(listContainer, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240f, 44f);

            var image = go.GetComponent<Image>();
            image.color = theme != null ? theme.deepBlue : new Color(0.15f, 0.1f, 0.05f, 0.9f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => OnBuildingButtonClicked(building));

            float labelInsetLeft = 0f;
            if (building.icon != null)
            {
                var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(go.transform, false);
                var iconImage = iconGO.GetComponent<Image>();
                iconImage.sprite = building.icon;
                iconImage.preserveAspect = true;

                var iconRect = iconGO.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0f, 0.5f);
                iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.pivot = new Vector2(0f, 0.5f);
                iconRect.anchoredPosition = new Vector2(4f, 0f);
                iconRect.sizeDelta = new Vector2(36f, 36f);

                labelInsetLeft = 40f;
            }

            var labelGO = new GameObject("Text", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = building.displayName;
            label.fontSize = 16f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = theme != null ? theme.ivoryWhite : Color.white;

            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(labelInsetLeft, 0f);
            labelRect.offsetMax = Vector2.zero;
        }

        private void OnBuildingButtonClicked(BuildingData building)
        {
            inputController.SelectBuilding(building);
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            Close();
        }
    }
}
