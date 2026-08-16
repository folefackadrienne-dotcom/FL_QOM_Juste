using KingdomOfGod.Core;
using KingdomOfGod.Miracles;
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
        [SerializeField] private GameObject prophecyJournalPanel;
        [SerializeField] private BuildingPaletteUI buildingPalette;
        [SerializeField] private MissionListUI missionList;
        [SerializeField] private LeaderScreenUI leaderScreen;
        [SerializeField] private AntagonistCodexUI antagonistCodex;
        [SerializeField] private CollectionUI collectionUI;
        [SerializeField] private KingdomTurnManager turnManager;
        [SerializeField] private TMP_Text turnLabel;
        [SerializeField] private MiracleManager miracleManager;
        [SerializeField] private TMP_Text prayerStatusLabel;
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
        }

        public void OpenPrayerMenu() => prayerMenu.Open();
        public void OpenVerseJournal() => verseJournal.Open();
        public void OpenBuildingPalette() => buildingPalette.Open();
        public void OpenMissionList() => missionList.Open();
        public void OpenLeaderScreen() => leaderScreen.Open();
        public void OpenAntagonistCodex() => antagonistCodex.Open();
        public void OpenCollection() => collectionUI.Open();

        public void EndTurn()
        {
            turnManager.EndTurn();
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            saveCoordinator?.SaveGame();
        }

        /// <summary>Manual save, e.g. from a "Sauvegarder" toolbar button — on top of the automatic save at the end of every turn, so the player always has an explicit way to be sure their progress is on disk.</summary>
        public void SaveGame() => saveCoordinator?.SaveGame();

        public void ToggleProphecyJournal()
        {
            bool opening = !prophecyJournalPanel.activeSelf;
            prophecyJournalPanel.SetActive(opening);
            GameManager.Instance?.Audio.PlaySfx(opening
                ? "Interface - Ouverture de Menu"
                : "Interface - Fermeture de Menu");
        }

        private void OnTurnAdvanced(int turn) => UpdateTurnLabel();

        private void UpdateTurnLabel()
        {
            if (turnLabel != null && turnManager != null) turnLabel.text = $"Tour {turnManager.TurnNumber}";
        }
    }
}
