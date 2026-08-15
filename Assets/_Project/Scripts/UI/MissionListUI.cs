using System.Collections.Generic;
using KingdomOfGod.Core;
using KingdomOfGod.Missions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>
    /// Mission selection list for the Kingdom scene, filtered to MissionType.Battle missions from
    /// an unlocked Age that aren't already completed (allBattleMissions populated by
    /// ProjectSceneSetup from Assets/_Project/ScriptableObjects/Missions/ — same
    /// no-manual-dragging reasoning as BuildingPaletteUI.allBuildings). Selecting one calls
    /// MissionManager.StartMission, which for a Battle mission loads the Battle scene right away —
    /// this panel's job ends the moment a mission is picked. Only MissionType.Battle missions are
    /// listed: MissionManager.StartMission works for the other five types too, but nothing yet
    /// resolves what happens next for them (no Construction/MoralChoice/Diplomacy/Survival/Sandbox
    /// flow exists), so surfacing them here would look like a working feature it isn't.
    /// </summary>
    public class MissionListUI : MonoBehaviour
    {
        [SerializeField] private MissionManager missionManager;
        [SerializeField] private AgeManager ageManager;
        [SerializeField] private UIThemeData theme;

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform listContainer;
        [SerializeField] private Button closeButton;

        [SerializeField] private List<MissionData> allBattleMissions = new List<MissionData>();

        private void Awake()
        {
            // missionManager/ageManager live on the persistent Bootstrap GameManager, in a
            // different scene from this panel — Inspector references can't cross scenes, so fall
            // back to the running singleton when these fields were left unassigned.
            if (missionManager == null && GameManager.Instance != null)
            {
                missionManager = GameManager.Instance.Missions;
            }
            if (ageManager == null && GameManager.Instance != null)
            {
                ageManager = GameManager.Instance.Ages;
            }
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
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

        private void RefreshList()
        {
            if (listContainer == null) return;

            for (int i = listContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(listContainer.GetChild(i).gameObject);
            }

            foreach (var mission in allBattleMissions)
            {
                if (mission == null) continue;
                if (ageManager != null && !ageManager.IsUnlocked(mission.age)) continue;
                if (missionManager != null && missionManager.IsCompleted(mission)) continue;

                CreateMissionButton(mission);
            }
        }

        private void CreateMissionButton(MissionData mission)
        {
            var go = new GameObject(mission.displayName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(listContainer, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280f, 44f);

            var image = go.GetComponent<Image>();
            image.color = theme != null ? theme.deepBlue : new Color(0.15f, 0.1f, 0.05f, 0.9f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => OnMissionButtonClicked(mission));

            var labelGO = new GameObject("Text", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = mission.displayName;
            label.fontSize = 16f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = theme != null ? theme.ivoryWhite : Color.white;

            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
        }

        private void OnMissionButtonClicked(MissionData mission)
        {
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            missionManager.StartMission(mission);
        }
    }
}
