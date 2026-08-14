using System.Collections.Generic;
using KingdomOfGod.Verses;
using UnityEngine;

namespace KingdomOfGod.UI
{
    /// <summary>"Bibliothèque de la Torah" / Mode Méditation: browse all memorized verses outside of missions.</summary>
    public class VerseJournalUI : MonoBehaviour
    {
        [SerializeField] private VerseManager verseManager;
        [SerializeField] private GameObject panelRoot;

        public void Open() => panelRoot.SetActive(true);
        public void Close() => panelRoot.SetActive(false);

        public IReadOnlyCollection<VerseData> GetMemorizedVerses() => verseManager.MemorizedVerses;
    }
}
