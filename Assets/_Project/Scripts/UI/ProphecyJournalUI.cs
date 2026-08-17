using KingdomOfGod.Core;
using KingdomOfGod.Prophecy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>
    /// Prophecy journal: HUDController.ToggleProphecyJournal used to open an empty parchment panel
    /// (no ProphecyData/ProphecyManager existed at all to back it). One entry per Age, auto-unlocked
    /// via ProphecyManager's AgeManager.AgeUnlocked subscription, shown list+detail like
    /// VerseJournalUI — minus a progression mini-game, since a prophecy is read, not memorized.
    /// </summary>
    public class ProphecyJournalUI : MonoBehaviour
    {
        [SerializeField] private ProphecyManager prophecyManager;
        [SerializeField] private UIThemeData theme;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform listContainer;
        [SerializeField] private Button closeButton;

        [Header("Detail view")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text referenceText;
        [SerializeField] private TMP_Text detailText;

        private ProphecyData selected;

        private void Awake()
        {
            // prophecyManager lives on the persistent Bootstrap GameManager, in a different scene
            // from this panel — Inspector references can't cross scenes, so fall back to the
            // running singleton when this field was left unassigned.
            if (prophecyManager == null && GameManager.Instance != null)
            {
                prophecyManager = GameManager.Instance.Prophecies;
            }
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (prophecyManager != null) prophecyManager.ProphecyUnlocked += OnProphecyUnlocked;
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (prophecyManager != null) prophecyManager.ProphecyUnlocked -= OnProphecyUnlocked;
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

        private void OnProphecyUnlocked(ProphecyData prophecy) => RefreshList();

        private void RefreshList()
        {
            if (listContainer == null || prophecyManager == null) return;

            for (int i = listContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(listContainer.GetChild(i).gameObject);
            }

            ProphecyData firstEntry = null;
            foreach (var prophecy in prophecyManager.UnlockedProphecies)
            {
                if (prophecy == null) continue;
                if (firstEntry == null) firstEntry = prophecy;
                CreateProphecyButton(prophecy);
            }

            if (selected == null && firstEntry != null) SelectProphecy(firstEntry);
        }

        private void CreateProphecyButton(ProphecyData prophecy)
        {
            var go = new GameObject(prophecy.displayName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(listContainer, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240f, 44f);

            var image = go.GetComponent<Image>();
            image.color = theme != null ? theme.deepBlue : new Color(0.15f, 0.1f, 0.05f, 0.9f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => SelectProphecy(prophecy));

            var labelGO = new GameObject("Text", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = prophecy.displayName;
            label.fontSize = 15f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = theme != null ? theme.ivoryWhite : Color.white;

            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
        }

        private void SelectProphecy(ProphecyData prophecy)
        {
            selected = prophecy;
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            UpdateDetail(prophecy);
        }

        private void UpdateDetail(ProphecyData prophecy)
        {
            if (nameText != null) nameText.text = prophecy.displayName;
            if (referenceText != null) referenceText.text = prophecy.reference;
            if (detailText != null) detailText.text = $"{prophecy.prophecyText}\n\n— Accomplissement —\n{prophecy.fulfillmentText}";
        }
    }
}
