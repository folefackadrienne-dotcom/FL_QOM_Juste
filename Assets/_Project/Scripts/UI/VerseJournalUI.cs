using KingdomOfGod.Core;
using KingdomOfGod.Verses;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>
    /// "Bibliothèque de la Torah" / Mode Méditation, plus the memorization mini-game's trigger.
    /// VerseManager.AdvanceStep was a real, callable method with nothing ever calling it, and this
    /// panel needed a listItemPrefab that was never authored (Assets/_Project/Prefabs is empty) — so
    /// RefreshList silently no-op'd forever regardless. Rebuilt to generate rows in code (the same
    /// UITheme-button pattern used everywhere else in this project) and to drive AdvanceStep for
    /// unlocked-but-not-yet-memorized verses: GDD "lecture → trous → ordre → quiz" is represented
    /// here as one "Avancer" click per step rather than 4 separate interactive puzzle screens, since
    /// none of that puzzle content (which words to blank, which quiz questions) has been authored.
    /// </summary>
    public class VerseJournalUI : MonoBehaviour
    {
        [SerializeField] private VerseManager verseManager;
        [SerializeField] private UIThemeData theme;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform listContainer;
        [SerializeField] private Button closeButton;

        [Header("Detail view")]
        [SerializeField] private TMP_Text referenceText;
        [SerializeField] private TMP_Text stepText;
        [SerializeField] private TMP_Text verseText;
        [SerializeField] private Button actionButton;
        [SerializeField] private TMP_Text actionButtonLabel;

        private VerseData selected;

        private void Awake()
        {
            // verseManager lives on the persistent Bootstrap GameManager, in a different
            // scene from this journal — Inspector references can't cross scenes, so fall back
            // to the running singleton when this field was left unassigned.
            if (verseManager == null && GameManager.Instance != null)
            {
                verseManager = GameManager.Instance.Verses;
            }
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (actionButton != null) actionButton.onClick.AddListener(OnActionClicked);

            if (verseManager != null)
            {
                verseManager.VerseUnlocked += OnVerseChanged;
                verseManager.VerseMemorized += OnVerseChanged;
            }
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (actionButton != null) actionButton.onClick.RemoveListener(OnActionClicked);

            if (verseManager != null)
            {
                verseManager.VerseUnlocked -= OnVerseChanged;
                verseManager.VerseMemorized -= OnVerseChanged;
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
            GameManager.Instance?.Audio.StopNarration();
            GameManager.Instance?.Audio.PlaySfx("Interface - Fermeture de Menu");
        }

        private void OnVerseChanged(VerseData verse)
        {
            RefreshList();
            if (selected == verse) UpdateDetail(verse);
        }

        private void RefreshList()
        {
            if (listContainer == null || verseManager == null) return;

            for (int i = listContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(listContainer.GetChild(i).gameObject);
            }

            VerseData firstEntry = null;
            foreach (var verse in verseManager.UnlockedVerses)
            {
                if (verse == null) continue;
                if (firstEntry == null) firstEntry = verse;
                CreateVerseButton(verse);
            }

            if ((selected == null || !verseManager.IsUnlocked(selected)) && firstEntry != null)
            {
                SelectVerse(firstEntry);
            }
        }

        private void CreateVerseButton(VerseData verse)
        {
            bool memorized = verseManager.IsMemorized(verse);

            var go = new GameObject(verse.reference, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(listContainer, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240f, 44f);

            var image = go.GetComponent<Image>();
            image.color = memorized
                ? (theme != null ? theme.divineLight : new Color(0.957f, 0.769f, 0.188f))
                : (theme != null ? theme.deepBlue : new Color(0.15f, 0.1f, 0.05f, 0.9f));

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => SelectVerse(verse));

            var labelGO = new GameObject("Text", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = verse.reference;
            label.fontSize = 16f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = theme != null ? theme.ivoryWhite : Color.white;

            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
        }

        private void SelectVerse(VerseData verse)
        {
            selected = verse;
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            UpdateDetail(verse);
        }

        private void UpdateDetail(VerseData verse)
        {
            bool memorized = verseManager.IsMemorized(verse);
            var step = verseManager.GetProgress(verse);

            if (referenceText != null) referenceText.text = verse.reference;
            if (verseText != null) verseText.text = verse.text;
            if (stepText != null) stepText.text = memorized ? "Mémorisé" : $"Étape : {StepLabel(step)}";
            if (actionButtonLabel != null) actionButtonLabel.text = memorized ? "Écouter" : "Avancer";
        }

        private static string StepLabel(MemorizationStep step) => step switch
        {
            MemorizationStep.Reading => "Lecture",
            MemorizationStep.FillInTheBlanks => "Compléter les trous",
            MemorizationStep.Reorder => "Remettre dans l'ordre",
            MemorizationStep.ContextQuiz => "Quiz de contexte",
            _ => "Terminé"
        };

        private void OnActionClicked()
        {
            if (selected == null || verseManager == null) return;

            if (verseManager.IsMemorized(selected))
            {
                PlayNarration(selected);
            }
            else
            {
                verseManager.AdvanceStep(selected);
                UpdateDetail(selected);
            }
        }

        /// <summary>"Les versets mémorisés peuvent être écoutés en boucle avec une musique très douce en fond" — Mode Méditation's read-aloud.</summary>
        public void PlayNarration(VerseData verse) => GameManager.Instance?.Audio.PlayVerseNarration(verse);

        public void StopNarration() => GameManager.Instance?.Audio.StopNarration();
    }
}
