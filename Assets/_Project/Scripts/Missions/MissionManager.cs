using System;
using System.Collections.Generic;
using KingdomOfGod.Resources;
using UnityEngine;

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

        public bool IsCompleted(MissionData mission) => completedMissions.Contains(mission);

        public void StartMission(MissionData mission)
        {
            ActiveMission = mission;
            MissionStarted?.Invoke(mission);
        }

        public void CompleteActiveMission()
        {
            if (ActiveMission == null) return;

            completedMissions.Add(ActiveMission);
            foreach (var reward in ActiveMission.rewards)
            {
                resourceManager.Add(reward.type, reward.amount);
            }

            MissionCompleted?.Invoke(ActiveMission);
            ActiveMission = null;
        }
    }
}
