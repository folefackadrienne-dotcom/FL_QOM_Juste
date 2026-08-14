using System.Collections.Generic;
using KingdomOfGod.Resources;
using UnityEngine;

namespace KingdomOfGod.Progression
{
    /// <summary>The 3 technology branches (GDD: Militaire, Spirituelle, Civile).</summary>
    public enum TechBranch
    {
        Military,
        Spiritual,
        Civil
    }

    [CreateAssetMenu(fileName = "Tech_", menuName = "Kingdom of God/Tech Node", order = 80)]
    public class TechNode : ScriptableObject
    {
        public string displayName;
        public TechBranch branch;
        [TextArea] public string effectDescription;
        public List<ResourceAmount> cost = new List<ResourceAmount>();
        public List<TechNode> prerequisites = new List<TechNode>();
    }

    /// <summary>Tracks which tech nodes have been researched and enforces prerequisite chains.</summary>
    public class TechTree : MonoBehaviour
    {
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private List<TechNode> allNodes = new List<TechNode>();

        private readonly HashSet<TechNode> unlocked = new HashSet<TechNode>();

        public bool IsUnlocked(TechNode node) => unlocked.Contains(node);

        public bool CanUnlock(TechNode node)
        {
            if (IsUnlocked(node)) return false;
            if (!resourceManager.CanAfford(node.cost)) return false;
            foreach (var prereq in node.prerequisites)
            {
                if (!unlocked.Contains(prereq)) return false;
            }
            return true;
        }

        public bool TryUnlock(TechNode node)
        {
            if (!CanUnlock(node) || !resourceManager.TrySpend(node.cost)) return false;
            unlocked.Add(node);
            return true;
        }
    }
}
