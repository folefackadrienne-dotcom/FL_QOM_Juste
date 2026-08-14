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

        [Header("Optional battle scene to load")]
        public string battleSceneName;
    }
}
