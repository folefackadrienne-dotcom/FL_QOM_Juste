using KingdomOfGod.Core;
using KingdomOfGod.Miracles;
using KingdomOfGod.Quiz;
using KingdomOfGod.SaveSystem;
using TMPro;
using UnityEngine;

namespace KingdomOfGod.UI
{
    /// <summary>Top-level HUD wiring: resource bar + the always-accessible Torah tab and prophecy journal (GDD 10. Interface), plus the "Fin de Tour" button that drives KingdomTurnManager — Kingdom's only source of a turn advancing at all — and the "Sauvegarder" button, which along with an auto-save on every turn end is the only place SaveCoordinator.SaveGame is ever actually called.</summary>
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private ResourceBarUI resourceBar;
        [SerializeField] private PrayerMenuUI prayerMenu;
        [SerializeField] private VerseJournalUI verseJournal;
        [SerializeField] private ProphecyJournalUI prophecyJournal;
        [SerializeField] private BuildingPaletteUI buildingPalette;
        [SerializeField] private MissionListUI missionList;
        [SerializeField] private LeaderScreenUI leaderScreen;
        [SerializeField] private AntagonistCodexUI antagonistCodex;
        [SerializeField] private CollectionUI collectionUI;
        [SerializeField] private TechScreenUI techScreen;
        [SerializeField] private StoreUI storeUI;
        [SerializeField] private QuizUI quizUI;
        [SerializeField] private KingdomTurnManager turnManager;
        [SerializeField] private TMP_Text turnLabel;
        [SerializeField] private MiracleManager miracleManager;
        [SerializeField] private TMP_Text prayerStatusLabel;
        [SerializeField] private QuizManager quizManager;
        [SerializeField] private TMP_Text quizStatusLabel;
        [SerializeField] private SaveCoordinator saveCoordinator;

        private void Awake()
        {
            // turnManager/miracleManager/saveCoordinator live on the persistent Bootstrap
            // GameManager, in a different scene from this HUD — Inspector references can't cross
            // scenes, so fall back to the running singleton when these fields were left unassigned.
            if (turnManager == null && GameManager.Instance != null)
            {
                turnManager = GameManager.Instance.Turns;
            }
            if (miracleManager == null && GameManager.Instance != null)
            {
                miracleManager = GameManager.Instance.Miracles;
            }
            if (saveCoordinator == null && GameManager.Instance != null)
            {
                saveCoordinator = GameManager.Instance.SaveCoordinator;
            }
            if (quizManager == null && GameManager.Instance != null)
            {
                quizManager = GameManager.Instance.Quiz;
            }
        }

        private void OnEnable()
        {
            if (turnManager != null) turnManager.TurnAdvanced += OnTurnAdvanced;
            UpdateTurnLabel();

            // PrayerMenuUI's own view only refreshes while its panel is open (its GameObject
            // doubles as panelRoot, so Close() disabling it also unsubscribes its handlers) —
            // this label lives on the HUD's always-active GameObject instead, so it keeps
            // tracking ritual progress even with the prayer panel closed, mirroring turnLabel.
            if (miracleManager != null)
            {
                miracleManager.PrayerStarted += OnPrayerStateChanged;
                miracleManager.PrayerProgressed += OnPrayerProgressed;
                miracleManager.PrayerInterrupted += OnPrayerStateChanged;
                miracleManager.PrayerCancelled += OnPrayerStateChanged;
                miracleManager.MiracleCast += OnPrayerStateChanged;
            }
            UpdatePrayerLabel();

            if (quizManager != null)
            {
                quizManager.QuestionAvailable += OnQuizQuestionAvailable;
                quizManager.QuestionAnswered += OnQuizQuestionAnswered;
            }
            UpdateQuizLabel();
        }

        private void OnDisable()
        {
            if (turnManager != null) turnManager.TurnAdvanced -= OnTurnAdvanced;

            if (miracleManager != null)
            {
                miracleManager.PrayerStarted -= OnPrayerStateChanged;
                miracleManager.PrayerProgressed -= OnPrayerProgressed;
                miracleManager.PrayerInterrupted -= OnPrayerStateChanged;
                miracleManager.PrayerCancelled -= OnPrayerStateChanged;
                miracleManager.MiracleCast -= OnPrayerStateChanged;
            }

            if (quizManager != null)
            {
                quizManager.QuestionAvailable -= OnQuizQuestionAvailable;
                quizManager.QuestionAnswered -= OnQuizQuestionAnswered;
            }
        }

        public void OpenPrayerMenu() => prayerMenu.Open();
        public void OpenVerseJournal() => verseJournal.Open();
        public void OpenProphecyJournal() => prophecyJournal.Open();
        public void OpenBuildingPalette() => buildingPalette.Open();
        public void OpenMissionList() => missionList.Open();
        public void OpenLeaderScreen() => leaderScreen.Open();
        public void OpenAntagonistCodex() => antagonistCodex.Open();
        public void OpenCollection() => collectionUI.Open();
        public void OpenTechScreen() => techScreen.Open();
        public void OpenStore() => storeUI.Open();
        public void OpenQuiz() => quizUI.Open();

        public void EndTurn()
        {
            turnManager.EndTurn();
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            saveCoordinator?.SaveGame();
        }

        /// <summary>Manual save, e.g. from a "Sauvegarder" toolbar button — on top of the automatic save at the end of every turn, so the player always has an explicit way to be sure their progress is on disk.</summary>
        public void SaveGame() => saveCoordinator?.SaveGame();

        private void OnTurnAdvanced(int turn) => UpdateTurnLabel();

        private void UpdateTurnLabel()
        {
            if (turnLabel != null && turnManager != null) turnLabel.text = $"Tour {turnManager.TurnNumber}";
        }

        private void OnPrayerStateChanged(MiracleData miracle) => UpdatePrayerLabel();
        private void OnPrayerProgressed(MiracleData miracle, int turns) => UpdatePrayerLabel();

        private void UpdatePrayerLabel()
        {
            if (prayerStatusLabel == null || miracleManager == null) return;

            if (!miracleManager.IsPraying)
            {
                prayerStatusLabel.text = "";
                return;
            }

            int duration = miracleManager.GetEffectivePrayerDuration(miracleManager.ActiveMiracle);
            prayerStatusLabel.text = $"Prière : {miracleManager.ActiveMiracle.displayName} ({miracleManager.PrayerProgressTurns}/{duration})";
        }

        /// <summary>A new question rolled by QuizManager every turnsBetweenQuestions — mirrors prayerStatusLabel's always-visible HUD indicator, so the player notices without the panel being forced open.</summary>
        private void OnQuizQuestionAvailable(QuizQuestionData question) => UpdateQuizLabel();
        private void OnQuizQuestionAnswered(QuizQuestionData question, bool correct) => UpdateQuizLabel();

        private void UpdateQuizLabel()
        {
            if (quizStatusLabel == null || quizManager == null) return;
            quizStatusLabel.text = quizManager.HasAvailableQuestion ? "Nouvelle question disponible !" : "";
        }
    }
}
