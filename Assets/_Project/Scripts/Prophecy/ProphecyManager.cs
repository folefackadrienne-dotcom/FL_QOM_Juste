using System;
using System.Collections.Generic;
using KingdomOfGod.Core;
using UnityEngine;

namespace KingdomOfGod.Prophecy
{
    /// <summary>
    /// Tracks which ProphecyData entries are unlocked for the "Journal Prophétique" —
    /// HUDController.ToggleProphecyJournal opened a panel that ProjectSceneSetup built with
    /// nothing in it but a parchment background: no ProphecyData/ProphecyManager existed at all.
    /// Auto-unlocks the current Age's prophecy the moment that Age does, the same
    /// AgeManager.AgeUnlocked pattern already used for Verses/Miracles/Artifacts/Leaders.
    /// </summary>
    public class ProphecyManager : MonoBehaviour
    {
        [SerializeField] private AgeManager ageManager;
        [SerializeField] private List<ProphecyData> allProphecies = new List<ProphecyData>();

        private readonly HashSet<ProphecyData> unlockedProphecies = new HashSet<ProphecyData>();

        public IReadOnlyCollection<ProphecyData> UnlockedProphecies => unlockedProphecies;
        public event Action<ProphecyData> ProphecyUnlocked;

        private void Start()
        {
            if (ageManager == null) return;

            foreach (var prophecy in allProphecies)
            {
                if (prophecy != null && ageManager.IsUnlocked(prophecy.age)) Unlock(prophecy);
            }
        }

        private void OnEnable()
        {
            if (ageManager != null) ageManager.AgeUnlocked += OnAgeUnlocked;
        }

        private void OnDisable()
        {
            if (ageManager != null) ageManager.AgeUnlocked -= OnAgeUnlocked;
        }

        private void OnAgeUnlocked(Age age)
        {
            foreach (var prophecy in allProphecies)
            {
                if (prophecy != null && prophecy.age == age) Unlock(prophecy);
            }
        }

        public void Unlock(ProphecyData prophecy)
        {
            if (unlockedProphecies.Add(prophecy))
            {
                ProphecyUnlocked?.Invoke(prophecy);
            }
        }

        public bool IsUnlocked(ProphecyData prophecy) => unlockedProphecies.Contains(prophecy);

        /// <summary>Reapplies prophecies unlocked in a save file, matched by ProphecyData.displayName against allProphecies.</summary>
        public void RestoreFromSave(IEnumerable<string> savedUnlockedDisplayNames)
        {
            var nameSet = new HashSet<string>(savedUnlockedDisplayNames);
            foreach (var prophecy in allProphecies)
            {
                if (prophecy != null && nameSet.Contains(prophecy.displayName)) unlockedProphecies.Add(prophecy);
            }
        }
    }
}
