using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomOfGod.Core
{
    /// <summary>Tracks which age the player is currently in and which ages have been unlocked.</summary>
    public class AgeManager : MonoBehaviour
    {
        [SerializeField] private Age startingAge = Age.Patriarchs;

        private readonly HashSet<Age> unlockedAges = new HashSet<Age>();

        public Age CurrentAge { get; private set; }

        public event Action<Age> AgeChanged;
        public event Action<Age> AgeUnlocked;

        private void Awake()
        {
            CurrentAge = startingAge;
            UnlockAge(startingAge);
        }

        public bool IsUnlocked(Age age) => unlockedAges.Contains(age);

        public void UnlockAge(Age age)
        {
            if (unlockedAges.Add(age))
            {
                AgeUnlocked?.Invoke(age);
            }
        }

        public void SetCurrentAge(Age age)
        {
            if (!IsUnlocked(age))
            {
                Debug.LogWarning($"Cannot switch to {age}: not unlocked yet.");
                return;
            }

            if (CurrentAge == age) return;

            CurrentAge = age;
            AgeChanged?.Invoke(age);
        }

        /// <summary>Advances to the next age in the chronology and unlocks it.</summary>
        public void AdvanceToNextAge()
        {
            int next = (int)CurrentAge + 1;
            if (next > (int)Age.ExileAndReturn)
            {
                Debug.Log("Campaign complete: no age beyond the Exile and Return.");
                return;
            }

            var nextAge = (Age)next;
            UnlockAge(nextAge);
            SetCurrentAge(nextAge);
        }
    }
}
