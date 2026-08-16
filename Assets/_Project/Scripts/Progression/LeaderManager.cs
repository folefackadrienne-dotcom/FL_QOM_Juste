using System;
using System.Collections.Generic;
using KingdomOfGod.Core;
using KingdomOfGod.Missions;
using UnityEngine;

namespace KingdomOfGod.Progression
{
    /// <summary>
    /// Tracks which leaders are unlocked and which one is currently active/in command. Unlock and
    /// SetActiveLeader were real, callable methods with nothing ever calling them — allLeaders (below,
    /// populated by ProjectSceneSetup) plus this class's own Start/OnAgeUnlocked/OnMissionCompleted are
    /// the missing trigger: a leader unlocks the moment LeaderData.unlockMission completes (if set), or
    /// the moment LeaderData.age unlocks otherwise — matching each of the 10 leaders' unlockCondition
    /// text ("Débloqué à la mission «...»" vs "Débloqué au début de l'Âge N"). Activating one (picking
    /// who's "in command") is still a player choice made from the Leader screen.
    /// </summary>
    public class LeaderManager : MonoBehaviour
    {
        [SerializeField] private AgeManager ageManager;
        [SerializeField] private MissionManager missionManager;
        [SerializeField] private List<LeaderData> allLeaders = new List<LeaderData>();

        private readonly HashSet<LeaderData> unlockedLeaders = new HashSet<LeaderData>();

        public LeaderData ActiveLeader { get; private set; }
        public IReadOnlyList<LeaderData> AllLeaders => allLeaders;
        public IReadOnlyCollection<LeaderData> UnlockedLeaders => unlockedLeaders;

        public event Action<LeaderData> LeaderUnlocked;
        public event Action<LeaderData> LeaderActivated;

        private void Start()
        {
            if (ageManager == null) return;

            foreach (var leader in allLeaders)
            {
                if (leader != null && leader.unlockMission == null && ageManager.IsUnlocked(leader.age))
                {
                    Unlock(leader);
                }
            }
        }

        private void OnEnable()
        {
            if (ageManager != null) ageManager.AgeUnlocked += OnAgeUnlocked;
            if (missionManager != null) missionManager.MissionCompleted += OnMissionCompleted;
        }

        private void OnDisable()
        {
            if (ageManager != null) ageManager.AgeUnlocked -= OnAgeUnlocked;
            if (missionManager != null) missionManager.MissionCompleted -= OnMissionCompleted;
        }

        private void OnAgeUnlocked(Age age)
        {
            foreach (var leader in allLeaders)
            {
                if (leader != null && leader.unlockMission == null && leader.age == age)
                {
                    Unlock(leader);
                }
            }
        }

        private void OnMissionCompleted(MissionData mission)
        {
            foreach (var leader in allLeaders)
            {
                if (leader != null && leader.unlockMission == mission)
                {
                    Unlock(leader);
                }
            }
        }

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

        /// <summary>Reapplies leader progress loaded from a save file, matched by LeaderData.displayName against allLeaders — marks each unlocked directly (Start/OnAgeUnlocked/OnMissionCompleted will have already re-unlocked age/mission-gated leaders reached again this session, so this only adds any that requires state Start() can't see) and restores the active leader without replaying SetActiveLeader's already-active guard.</summary>
        public void RestoreFromSave(IEnumerable<string> savedUnlockedDisplayNames, string savedActiveDisplayName)
        {
            var nameSet = new HashSet<string>(savedUnlockedDisplayNames);
            foreach (var leader in allLeaders)
            {
                if (leader != null && nameSet.Contains(leader.displayName)) unlockedLeaders.Add(leader);
            }

            if (string.IsNullOrEmpty(savedActiveDisplayName)) return;

            var active = allLeaders.Find(l => l != null && l.displayName == savedActiveDisplayName);
            if (active != null && unlockedLeaders.Contains(active))
            {
                ActiveLeader = active;
                LeaderActivated?.Invoke(active);
            }
        }
    }
}
