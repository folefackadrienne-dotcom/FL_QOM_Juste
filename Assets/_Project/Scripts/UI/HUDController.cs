using UnityEngine;

namespace KingdomOfGod.UI
{
    /// <summary>Top-level HUD wiring: resource bar + the always-accessible Torah tab and prophecy journal (GDD 10. Interface).</summary>
    public class HUDController : MonoBehaviour
    {
        [SerializeField] private ResourceBarUI resourceBar;
        [SerializeField] private PrayerMenuUI prayerMenu;
        [SerializeField] private VerseJournalUI verseJournal;
        [SerializeField] private GameObject prophecyJournalPanel;

        public void OpenPrayerMenu() => prayerMenu.Open();
        public void OpenVerseJournal() => verseJournal.Open();
        public void ToggleProphecyJournal() => prophecyJournalPanel.SetActive(!prophecyJournalPanel.activeSelf);
    }
}
