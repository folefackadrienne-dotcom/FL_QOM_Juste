using System;
using System.Collections.Generic;
using KingdomOfGod.Missions;
using UnityEngine;

namespace KingdomOfGod.Core
{
    /// <summary>
    /// Tracks which age the player is currently in and which ages have been unlocked.
    /// AdvanceToNextAge was a real, callable method with nothing ever calling it — the campaign had
    /// no notion of "done with this age, move on to the next" at all. missionManager (below) is the
    /// missing trigger: completing an age's last remaining mission calls AdvanceToNextAge, using
    /// MissionManager.AreAllMissionsComplete as the criterion.
    ///
    /// A free-tier player who finished the current age's last mission before buying the Full
    /// Edition would otherwise stay stuck forever: UnlockAge's content-gate refusal only fires
    /// AgeLocked into the void, and with no missions left to complete, OnMissionCompleted never
    /// fires again to retrigger the check. Subscribing to contentGate.GateChanged closes that gap —
    /// the moment a purchase flips the gate, AgeManager retries the advance on its own.
    /// </summary>
    public class AgeManager : MonoBehaviour
    {
        [SerializeField] private Age startingAge = Age.Patriarchs;
        [SerializeField] private MissionManager missionManager;

        [Tooltip("Optional component implementing IContentGate (e.g. EntitlementManager). "
            + "Leave empty to unlock ages freely, with no monetization restriction.")]
        [SerializeField] private MonoBehaviour contentGateBehaviour;

        private IContentGate contentGate;
        private readonly HashSet<Age> unlockedAges = new HashSet<Age>();

        public Age CurrentAge { get; private set; }

        public event Action<Age> AgeChanged;
        public event Action<Age> AgeUnlocked;
        public event Action<Age> AgeLocked;
        public event Action CampaignCompleted;

        private void Awake()
        {
            contentGate = contentGateBehaviour as IContentGate;

            CurrentAge = startingAge;
            UnlockAge(startingAge);
        }

        private void OnEnable()
        {
            if (missionManager != null) missionManager.MissionCompleted += OnMissionCompleted;
            if (contentGate != null) contentGate.GateChanged += OnGateChanged;
        }

        private void OnDisable()
        {
            if (missionManager != null) missionManager.MissionCompleted -= OnMissionCompleted;
            if (contentGate != null) contentGate.GateChanged -= OnGateChanged;
        }

        private void OnMissionCompleted(MissionData mission)
        {
            if (mission.age == CurrentAge && missionManager.AreAllMissionsComplete(CurrentAge))
            {
                AdvanceToNextAge();
            }
        }

        /// <summary>Retries the advance a purchase may have just unblocked — a no-op unless the current age's missions were already all complete and UnlockAge had previously refused the next one.</summary>
        private void OnGateChanged()
        {
            if (missionManager != null && missionManager.AreAllMissionsComplete(CurrentAge))
            {
                AdvanceToNextAge();
            }
        }

        public bool IsUnlocked(Age age) => unlockedAges.Contains(age);

        public void UnlockAge(Age age)
        {
            if (unlockedAges.Contains(age)) return;

            if (contentGate != null && !contentGate.CanUnlockAge(age))
            {
                AgeLocked?.Invoke(age);
                return;
            }

            unlockedAges.Add(age);
            AgeUnlocked?.Invoke(age);
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

        /// <summary>Reapplies age progress loaded from a save file: replays UnlockAge (idempotent, still content-gated) for each previously-unlocked age, then sets the current age — call after EntitlementManager.RestoreFromSave so the gate check reflects the restored tier.</summary>
        public void RestoreFromSave(IEnumerable<Age> savedUnlockedAges, Age savedCurrentAge)
        {
            foreach (var age in savedUnlockedAges)
            {
                UnlockAge(age);
            }
            SetCurrentAge(savedCurrentAge);
        }

        /// <summary>Advances to the next age in the chronology and unlocks it, or fires CampaignCompleted once past the Exile and Return.</summary>
        public void AdvanceToNextAge()
        {
            int next = (int)CurrentAge + 1;
            if (next > (int)Age.ExileAndReturn)
            {
                CampaignCompleted?.Invoke();
                return;
            }

            var nextAge = (Age)next;
            UnlockAge(nextAge);
            SetCurrentAge(nextAge);
        }
    }
}
