using System;
using System.Collections.Generic;
using KingdomOfGod.Resources;
using KingdomOfGod.Verses;
using UnityEngine;

namespace KingdomOfGod.Miracles
{
    /// <summary>Gates and casts miracles: checks Faith cost, required memorized verse, and prior moral choices.</summary>
    public class MiracleManager : MonoBehaviour
    {
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private VerseManager verseManager;
        [SerializeField] private List<MiracleData> unlockedMiracles = new List<MiracleData>();

        private readonly HashSet<string> fulfilledChoices = new HashSet<string>();

        public IReadOnlyList<MiracleData> UnlockedMiracles => unlockedMiracles;
        public event Action<MiracleData> MiracleCast;

        public void Unlock(MiracleData miracle)
        {
            if (!unlockedMiracles.Contains(miracle)) unlockedMiracles.Add(miracle);
        }

        public void RecordMoralChoice(string choiceId) => fulfilledChoices.Add(choiceId);

        public bool CanCast(MiracleData miracle)
        {
            if (!unlockedMiracles.Contains(miracle)) return false;
            if (resourceManager.Get(ResourceType.Faith) < miracle.faithCost) return false;
            if (miracle.requiredVerse != null && !verseManager.IsMemorized(miracle.requiredVerse)) return false;
            if (!string.IsNullOrEmpty(miracle.requiredPriorChoiceId) && !fulfilledChoices.Contains(miracle.requiredPriorChoiceId)) return false;
            return true;
        }

        public bool TryCast(MiracleData miracle)
        {
            if (!CanCast(miracle)) return false;

            resourceManager.Spend(ResourceType.Faith, miracle.faithCost);
            MiracleCast?.Invoke(miracle);
            return true;
        }
    }
}
