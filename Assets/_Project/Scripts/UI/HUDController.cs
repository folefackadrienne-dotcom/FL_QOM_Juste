using KingdomOfGod.Core;
using TMPro;
using UnityEngine;

namespace KingdomOfGod.UI
{
    /// <summary>Top-level HUD wiring: resource bar + the always-accessible Torah tab and prophecy journal (GDD 10. Interface), plus the "Fin de Tour" button that drives KingdomTurnManager — Kingdom's only source of a turn advancing at all.</summary>
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private ResourceBarUI resourceBar;
        [SerializeField] private PrayerMenuUI prayerMenu;
        [SerializeField] private VerseJournalUI verseJournal;
        [SerializeField] private GameObject prophecyJournalPanel;
        [SerializeField] private BuildingPaletteUI buildingPalette;
        [SerializeField] private MissionListUI missionList;
        [SerializeField] private KingdomTurnManager turnManager;
        [SerializeField] private TMP_Text turnLabel;

        private void Awake()
        {
            // turnManager lives on the persistent Bootstrap GameManager, in a different scene
            // from this HUD — Inspector references can't cross scenes, so fall back to the
            // running singleton when this field was left unassigned.
            if (turnManager == null && GameManager.Instance != null)
            {
                turnManager = GameManager.Instance.Turns;
            }
        }

        private void OnEnable()
        {
            if (turnManager != null) turnManager.TurnAdvanced += OnTurnAdvanced;
            UpdateTurnLabel();
        }

        private void OnDisable()
        {
            if (turnManager != null) turnManager.TurnAdvanced -= OnTurnAdvanced;
        }

        public void OpenPrayerMenu() => prayerMenu.Open();
        public void OpenVerseJournal() => verseJournal.Open();
        public void OpenBuildingPalette() => buildingPalette.Open();
        public void OpenMissionList() => missionList.Open();

        public void EndTurn()
        {
            turnManager.EndTurn();
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
        }

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
