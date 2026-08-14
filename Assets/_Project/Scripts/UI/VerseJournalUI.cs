using System.Collections.Generic;
using KingdomOfGod.Core;
using KingdomOfGod.Verses;
using UnityEngine;

namespace KingdomOfGod.UI
{
    /// <summary>"Bibliothèque de la Torah" / Mode Méditation: browse all memorized verses outside of missions.</summary>
    public class VerseJournalUI : MonoBehaviour
    {
        [SerializeField] private VerseManager verseManager;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform listContainer;
        [SerializeField] private VerseListItemUI listItemPrefab;

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

        public void Open()
        {
            panelRoot.SetActive(true);
            RefreshList();
            GameManager.Instance?.Audio.PlaySfx("Interface - Ouverture de Menu");
        }

        /// <summary>Repopulates the list with one VerseListItemUI per memorized verse — left a no-op until listItemPrefab exists (art not yet produced).</summary>
        private void RefreshList()
        {
            if (listContainer == null || listItemPrefab == null) return;

            for (int i = listContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(listContainer.GetChild(i).gameObject);
            }

            foreach (var verse in GetMemorizedVerses())
            {
                Instantiate(listItemPrefab, listContainer).Bind(verse, this);
            }
        }

        public void Close()
        {
            panelRoot.SetActive(false);
            GameManager.Instance?.Audio.StopNarration();
            GameManager.Instance?.Audio.PlaySfx("Interface - Fermeture de Menu");
        }

        public IReadOnlyCollection<VerseData> GetMemorizedVerses() => verseManager.MemorizedVerses;

        /// <summary>"Les versets mémorisés peuvent être écoutés en boucle avec une musique très douce en fond" — Mode Méditation's read-aloud.</summary>
        public void PlayNarration(VerseData verse) => GameManager.Instance?.Audio.PlayVerseNarration(verse);

        public void StopNarration() => GameManager.Instance?.Audio.StopNarration();
    }
}
