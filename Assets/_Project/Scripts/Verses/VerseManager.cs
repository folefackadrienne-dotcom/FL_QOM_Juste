using System;
using System.Collections.Generic;
using KingdomOfGod.Resources;
using UnityEngine;

namespace KingdomOfGod.Verses
{
    /// <summary>Steps of the progressive memorization mini-game (GDD: lecture, trous, ordre, quiz de contexte).</summary>
    public enum MemorizationStep
    {
        Reading,
        FillInTheBlanks,
        Reorder,
        ContextQuiz,
        Completed
    }

    /// <summary>Tracks unlocked verses, memorization progress, and the "Bibliothèque de la Torah" / Meditation mode.</summary>
    public class VerseManager : MonoBehaviour
    {
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private List<VerseData> allVerses = new List<VerseData>();

        private readonly HashSet<VerseData> unlockedVerses = new HashSet<VerseData>();
        private readonly HashSet<VerseData> memorizedVerses = new HashSet<VerseData>();
        private readonly Dictionary<VerseData, MemorizationStep> progress = new Dictionary<VerseData, MemorizationStep>();

        public IReadOnlyCollection<VerseData> MemorizedVerses => memorizedVerses;
        public event Action<VerseData> VerseUnlocked;
        public event Action<VerseData> VerseMemorized;

        public void Unlock(VerseData verse)
        {
            if (unlockedVerses.Add(verse))
            {
                progress[verse] = MemorizationStep.Reading;
                VerseUnlocked?.Invoke(verse);
            }
        }

        public bool IsMemorized(VerseData verse) => memorizedVerses.Contains(verse);

        public MemorizationStep GetProgress(VerseData verse) =>
            progress.TryGetValue(verse, out var step) ? step : MemorizationStep.Reading;

        /// <summary>Advances a verse to the next memorization step; grants the permanent bonus on completion.</summary>
        public void AdvanceStep(VerseData verse)
        {
            if (!unlockedVerses.Contains(verse) || memorizedVerses.Contains(verse)) return;

            var current = GetProgress(verse);
            var next = current switch
            {
                MemorizationStep.Reading => MemorizationStep.FillInTheBlanks,
                MemorizationStep.FillInTheBlanks => MemorizationStep.Reorder,
                MemorizationStep.Reorder => MemorizationStep.ContextQuiz,
                MemorizationStep.ContextQuiz => MemorizationStep.Completed,
                _ => MemorizationStep.Completed
            };
            progress[verse] = next;

            if (next == MemorizationStep.Completed)
            {
                memorizedVerses.Add(verse);
                // TODO: route through a permanent-modifier system once one exists (e.g. "+10% Faith
                // generation"); for now completing a verse grants its bonus as an immediate resource gain.
                foreach (var bonus in verse.permanentBonus)
                {
                    resourceManager.Add(bonus.type, bonus.amount);
                }
                VerseMemorized?.Invoke(verse);
            }
        }

        /// <summary>Reapplies memorized verses loaded from a save file, matched by VerseData.reference against allVerses — marks each unlocked and Completed directly, bypassing AdvanceStep's permanentBonus grant since the save's resource stock already reflects it.</summary>
        public void RestoreFromSave(IEnumerable<string> savedMemorizedReferences)
        {
            var referenceSet = new HashSet<string>(savedMemorizedReferences);
            foreach (var verse in allVerses)
            {
                if (!referenceSet.Contains(verse.reference)) continue;
                unlockedVerses.Add(verse);
                memorizedVerses.Add(verse);
                progress[verse] = MemorizationStep.Completed;
            }
        }
    }
}
