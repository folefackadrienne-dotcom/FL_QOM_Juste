using System;
using System.Collections.Generic;
using KingdomOfGod.Alliance;
using KingdomOfGod.Resources;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KingdomOfGod.Missions
{
    /// <summary>Tracks the active mission, completed missions, and resolves it per MissionType — a battle (see MissionBattleSetup), a resource cost, a resource check, or a two-option choice.</summary>
    public class MissionManager : MonoBehaviour
    {
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private AllianceSystem allianceSystem;
        [SerializeField] private List<MissionData> allMissions = new List<MissionData>();

        private readonly HashSet<MissionData> completedMissions = new HashSet<MissionData>();

        public MissionData ActiveMission { get; private set; }
        public IReadOnlyCollection<MissionData> CompletedMissions => completedMissions;

        public event Action<MissionData> MissionStarted;
        public event Action<MissionData> MissionCompleted;
        public event Action<MissionData> MissionFailed;

        public bool IsCompleted(MissionData mission) => completedMissions.Contains(mission);

        /// <summary>Sets the active mission and, for a MissionType.Battle mission, loads the shared Battle scene — MissionBattleSetup picks ActiveMission back up there to configure the fight. Every other type is resolved in place via TryResolveConstruction/TryResolveSurvival/ResolveMoralChoice/ResolveDiplomacy/ResolveSandbox, typically driven by MissionResolutionUI.</summary>
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
            Complete(ActiveMission.rewards);
        }

        /// <summary>MissionType.Construction: spends ActiveMission.constructionCost and completes on success; returns false with no state change if unaffordable.</summary>
        public bool TryResolveConstruction()
        {
            if (ActiveMission == null || ActiveMission.type != MissionType.Construction) return false;
            if (!resourceManager.TrySpend(ActiveMission.constructionCost)) return false;

            Complete(ActiveMission.rewards);
            return true;
        }

        /// <summary>MissionType.Survival: completes if the player currently holds ActiveMission.survivalRequirement — a snapshot check, nothing is spent — otherwise leaves the mission active so the player can stock up and try again.</summary>
        public bool TryResolveSurvival()
        {
            if (ActiveMission == null || ActiveMission.type != MissionType.Survival) return false;
            if (!resourceManager.CanAfford(ActiveMission.survivalRequirement)) return false;

            Complete(ActiveMission.rewards);
            return true;
        }

        /// <summary>MissionType.MoralChoice: applies the chosen option's Alliance consequence, then completes with the mission's base rewards regardless of which option was picked — the choice's weight is spiritual, not material.</summary>
        public void ResolveMoralChoice(bool chooseOptionA)
        {
            if (ActiveMission == null || ActiveMission.type != MissionType.MoralChoice) return;

            var option = chooseOptionA ? ActiveMission.optionA : ActiveMission.optionB;
            if (allianceSystem != null) allianceSystem.Modify(option.allianceDelta, ActiveMission.missionId);

            Complete(ActiveMission.rewards);
        }

        /// <summary>MissionType.Diplomacy: grants the chosen option's own reward set instead of the mission's base rewards — the choice's weight is practical, not spiritual.</summary>
        public void ResolveDiplomacy(bool chooseOptionA)
        {
            if (ActiveMission == null || ActiveMission.type != MissionType.Diplomacy) return;

            var option = chooseOptionA ? ActiveMission.optionA : ActiveMission.optionB;
            Complete(option.rewardOverride);
        }

        /// <summary>MissionType.Sandbox: no condition to check — completes immediately with the mission's base rewards.</summary>
        public void ResolveSandbox()
        {
            if (ActiveMission == null || ActiveMission.type != MissionType.Sandbox) return;
            Complete(ActiveMission.rewards);
        }

        /// <summary>Clears the active mission without granting rewards — leaves it eligible to be started again (only Complete marks it completed).</summary>
        public void FailActiveMission()
        {
            if (ActiveMission == null) return;

            var mission = ActiveMission;
            ActiveMission = null;
            MissionFailed?.Invoke(mission);
        }

        private void Complete(List<ResourceAmount> rewards)
        {
            var mission = ActiveMission;
            completedMissions.Add(mission);
            foreach (var reward in rewards)
            {
                resourceManager.Add(reward.type, reward.amount);
            }

            ActiveMission = null;
            MissionCompleted?.Invoke(mission);
        }

        /// <summary>Reapplies completed missions loaded from a save file, matched by MissionData.missionId against allMissions — marks each completed directly, bypassing Complete's reward grant since the save's resource stock already reflects it.</summary>
        public void RestoreFromSave(IEnumerable<string> savedCompletedMissionIds)
        {
            var idSet = new HashSet<string>(savedCompletedMissionIds);
            foreach (var mission in allMissions)
            {
                if (idSet.Contains(mission.missionId)) completedMissions.Add(mission);
            }
        }
    }
}
