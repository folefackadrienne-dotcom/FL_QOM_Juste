using KingdomOfGod.Core;
using KingdomOfGod.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>
    /// Leader roster screen: one button per LeaderData (LeaderManager.AllLeaders, populated by
    /// ProjectSceneSetup the same no-manual-dragging way as BuildingPaletteUI.allBuildings), a
    /// locked entry shows its unlockCondition instead of its narrative content, and an unlocked one
    /// can be put "in command" via SetActiveLeader — LeaderManager.Unlock/SetActiveLeader were real,
    /// callable methods with nothing ever calling them before this screen and LeaderManager's own
    /// Age/mission-triggered auto-unlock existed.
    /// </summary>
    public class LeaderScreenUI : MonoBehaviour
    {
        [SerializeField] private LeaderManager leaderManager;
        [SerializeField] private UIThemeData theme;

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform listContainer;
        [SerializeField] private Button closeButton;

        [Header("Detail view")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text roleText;
        [SerializeField] private TMP_Text arcText;
        [SerializeField] private Button activateButton;
        [SerializeField] private TMP_Text activateButtonLabel;

        private LeaderData selected;

        private void Awake()
        {
            // leaderManager lives on the persistent Bootstrap GameManager, in a different scene
            // from this screen — Inspector references can't cross scenes, so fall back to the
            // running singleton when this field was left unassigned.
            if (leaderManager == null && GameManager.Instance != null)
            {
                leaderManager = GameManager.Instance.Leaders;
            }
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (activateButton != null) activateButton.onClick.AddListener(ActivateSelected);

            if (leaderManager != null)
            {
                leaderManager.LeaderUnlocked += OnLeaderChanged;
                leaderManager.LeaderActivated += OnLeaderChanged;
            }
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (activateButton != null) activateButton.onClick.RemoveListener(ActivateSelected);

            if (leaderManager != null)
            {
                leaderManager.LeaderUnlocked -= OnLeaderChanged;
                leaderManager.LeaderActivated -= OnLeaderChanged;
            }
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

        private void OnLeaderChanged(LeaderData leader)
        {
            RefreshList();
            if (selected != null) UpdateDetail(selected);
        }

        private void RefreshList()
        {
            if (listContainer == null || leaderManager == null) return;

            for (int i = listContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(listContainer.GetChild(i).gameObject);
            }

            LeaderData firstEntry = null;
            foreach (var leader in leaderManager.AllLeaders)
            {
                if (leader == null) continue;
                if (firstEntry == null) firstEntry = leader;
                CreateLeaderButton(leader);
            }

            if (selected == null && firstEntry != null) SelectLeader(firstEntry);
        }

        private void CreateLeaderButton(LeaderData leader)
        {
            bool unlocked = leaderManager.IsUnlocked(leader);

            var go = new GameObject(leader.displayName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(listContainer, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240f, 44f);

            var image = go.GetComponent<Image>();
            image.color = unlocked
                ? (theme != null ? theme.deepBlue : new Color(0.15f, 0.1f, 0.05f, 0.9f))
                : new Color(0.3f, 0.3f, 0.3f, 0.6f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => SelectLeader(leader));

            var labelGO = new GameObject("Text", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = unlocked ? leader.displayName : "??? (Verrouillé)";
            label.fontSize = 16f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = theme != null ? theme.ivoryWhite : Color.white;

            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
        }

        private void SelectLeader(LeaderData leader)
        {
            selected = leader;
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            UpdateDetail(leader);
        }

        private void UpdateDetail(LeaderData leader)
        {
            bool unlocked = leaderManager != null && leaderManager.IsUnlocked(leader);

            if (nameText != null) nameText.text = unlocked ? leader.displayName : "??? (Verrouillé)";
            if (roleText != null) roleText.text = unlocked ? leader.gameplayRole : leader.unlockCondition;
            if (arcText != null) arcText.text = unlocked ? leader.narrativeArc : string.Empty;

            if (portraitImage != null)
            {
                portraitImage.sprite = unlocked ? leader.portrait : null;
                bool hasRealArt = unlocked && leader.portrait != null;
                portraitImage.color = hasRealArt
                    ? Color.white
                    : (theme != null ? theme.deepBlue : new Color(0.15f, 0.15f, 0.2f));
            }

            bool isActive = leaderManager != null && leaderManager.ActiveLeader == leader;
            if (activateButton != null)
            {
                activateButton.gameObject.SetActive(unlocked);
                activateButton.interactable = unlocked && !isActive;
            }
            if (activateButtonLabel != null) activateButtonLabel.text = isActive ? "Leader Actif" : "Activer";
        }

        private void ActivateSelected()
        {
            if (selected == null || leaderManager == null) return;
            leaderManager.SetActiveLeader(selected);
        }
    }
}
