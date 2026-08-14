using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomOfGod.Progression
{
    /// <summary>Tracks which leaders are unlocked and which one is currently active/in command.</summary>
    public class LeaderManager : MonoBehaviour
    {
        private readonly HashSet<LeaderData> unlockedLeaders = new HashSet<LeaderData>();

        public LeaderData ActiveLeader { get; private set; }

        public event Action<LeaderData> LeaderUnlocked;
        public event Action<LeaderData> LeaderActivated;

        public bool IsUnlocked(LeaderData leader) => unlockedLeaders.Contains(leader);

        public void Unlock(LeaderData leader)
        {
            if (unlockedLeaders.Add(leader))
            {
                LeaderUnlocked?.Invoke(leader);
            }
        }

        /// <summary>Puts an already-unlocked leader in command; does nothing for a leader that hasn't been unlocked yet.</summary>
        public void SetActiveLeader(LeaderData leader)
        {
            if (!unlockedLeaders.Contains(leader) || ActiveLeader == leader) return;

            ActiveLeader = leader;
            LeaderActivated?.Invoke(leader);
        }
    }
}
