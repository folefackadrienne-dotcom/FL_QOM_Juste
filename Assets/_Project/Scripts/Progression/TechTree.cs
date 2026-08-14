using System.Collections.Generic;
using KingdomOfGod.Resources;
using UnityEngine;

namespace KingdomOfGod.Progression
{
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

            foreach (var prereqId in node.prerequisiteIds)
            {
                var prereqNode = allNodes.Find(n => n.techId == prereqId);
                if (prereqNode == null || !unlocked.Contains(prereqNode)) return false;
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
