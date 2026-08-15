using System;
using System.Collections.Generic;
using KingdomOfGod.Resources;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KingdomOfGod.Missions
{
    /// <summary>Tracks the active mission, completed missions, and grants rewards on completion.</summary>
    public class MissionManager : MonoBehaviour
    {
        [SerializeField] private ResourceManager resourceManager;

        private readonly HashSet<MissionData> completedMissions = new HashSet<MissionData>();

        public MissionData ActiveMission { get; private set; }

        public event Action<MissionData> MissionStarted;
        public event Action<MissionData> MissionCompleted;
        public event Action<MissionData> MissionFailed;

        public bool IsCompleted(MissionData mission) => completedMissions.Contains(mission);

        /// <summary>Sets the active mission and, for a MissionType.Battle mission, loads the shared Battle scene — MissionBattleSetup picks ActiveMission back up there to configure the fight.</summary>
        public void StartMission(MissionData mission)
        {
            ActiveMission = mission;
            MissionStarted?.Invoke(mission);

            if (mission.type == MissionType.Battle)
            {
                SceneManager.LoadScene("Battle");
            }
        }

        public void CompleteActiveMission()
        {
            if (ActiveMission == null) return;

            completedMissions.Add(ActiveMission);
            foreach (var reward in ActiveMission.rewards)
            {
                resourceManager.Add(reward.type, reward.amount);
            }

            var mission = ActiveMission;
            ActiveMission = null;
            MissionCompleted?.Invoke(mission);
        }

        /// <summary>Clears the active mission without granting rewards — leaves it eligible to be started again (only CompleteActiveMission marks it completed).</summary>
        public void FailActiveMission()
        {
            if (ActiveMission == null) return;

            var mission = ActiveMission;
            ActiveMission = null;
            MissionFailed?.Invoke(mission);
        }
    }
}
