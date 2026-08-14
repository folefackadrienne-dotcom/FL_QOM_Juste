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
            GameManager.Instance?.Audio.PlaySfx("Interface - Ouverture de Menu");
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
