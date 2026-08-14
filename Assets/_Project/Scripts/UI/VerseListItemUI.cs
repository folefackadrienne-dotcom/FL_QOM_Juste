using KingdomOfGod.Verses;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>One row of VerseJournalUI's memorized-verses list — instantiated once per verse, see VerseJournalUI.RefreshList.</summary>
    public class VerseListItemUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text referenceLabel;
        [SerializeField] private Button playButton;

        public void Bind(VerseData verse, VerseJournalUI owner)
        {
            if (referenceLabel != null) referenceLabel.text = verse.reference;

            if (playButton != null)
            {
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(() => owner.PlayNarration(verse));
            }
        }
    }
}
