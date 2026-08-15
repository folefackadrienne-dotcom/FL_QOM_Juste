using System;
using System.Collections.Generic;
using KingdomOfGod.Battle;
using KingdomOfGod.Core;
using KingdomOfGod.Resources;
using UnityEngine;

namespace KingdomOfGod.Missions
{
    public enum MissionType
    {
        Battle,
        Construction,
        MoralChoice,
        Diplomacy,
        Survival,
        Sandbox
    }

    /// <summary>
    /// One side of a MissionType.MoralChoice or MissionType.Diplomacy resolution. MoralChoice
    /// missions use allianceDelta and ignore rewardOverride (both options grant the mission's base
    /// rewards — the weight of the choice is spiritual); Diplomacy missions use rewardOverride and
    /// ignore allianceDelta (the weight of the choice is practical: which reward set you get).
    /// </summary>
    [Serializable]
    public class MissionChoiceOption
    {
        public string label;
        public int allianceDelta;
        public List<ResourceAmount> rewardOverride = new List<ResourceAmount>();
    }

    /// <summary>One mission in the campaign (e.g. "Le Sacrifice d'Isaac", "La Chute de Jéricho").</summary>
    [CreateAssetMenu(fileName = "Mission_", menuName = "Kingdom of God/Mission", order = 60)]
    public class MissionData : ScriptableObject
    {
        public string missionId;
        public string displayName;
        [TextArea] public string summary;
        public Age age;
        public MissionType type;

        public VictoryCondition victoryCondition;
        public List<ResourceAmount> rewards = new List<ResourceAmount>();

        [Header("Optional hand-authored battle roster — MissionBattleSetup falls back to a generic squad on each side when left empty")]
        public List<UnitData> playerUnits = new List<UnitData>();
        public List<UnitData> enemyUnits = new List<UnitData>();

        [Header("MissionType.Construction — resources spent by MissionManager.TryResolveConstruction on completion")]
        public List<ResourceAmount> constructionCost = new List<ResourceAmount>();

        [Header("MissionType.Survival — resources that must currently be held (checked, not spent) for MissionManager.TryResolveSurvival to succeed")]
        public List<ResourceAmount> survivalRequirement = new List<ResourceAmount>();

        [Header("MissionType.MoralChoice / MissionType.Diplomacy — two-option resolution")]
        public MissionChoiceOption optionA = new MissionChoiceOption();
        public MissionChoiceOption optionB = new MissionChoiceOption();
    }
}
