using System;
using System.Collections.Generic;
using KingdomOfGod.Resources;
using UnityEngine;

namespace KingdomOfGod.Progression
{
    /// <summary>
    /// Tracks which tech nodes have been researched and enforces prerequisite chains. TryUnlock was
    /// a real, callable method with no UI anywhere ever reaching it — see UI/TechScreenUI, which
    /// reads AllNodes (below) to list every node, not just the already-unlocked ones.
    /// </summary>
    public class TechTree : MonoBehaviour
    {
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private List<TechNode> allNodes = new List<TechNode>();

        private readonly HashSet<TechNode> unlocked = new HashSet<TechNode>();

        public event Action<TechNode> TechUnlocked;

        public IReadOnlyCollection<TechNode> UnlockedNodes => unlocked;
        public IReadOnlyList<TechNode> AllNodes => allNodes;

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
            TechUnlocked?.Invoke(node);
            return true;
        }

        /// <summary>Reapplies unlocked tech loaded from a save file, matched by TechNode.techId against allNodes — marks each unlocked directly, bypassing TryUnlock's resource spend since the save's resource stock already reflects it.</summary>
        public void RestoreFromSave(IEnumerable<string> savedUnlockedTechIds)
        {
            var idSet = new HashSet<string>(savedUnlockedTechIds);
            foreach (var node in allNodes)
            {
                if (idSet.Contains(node.techId)) unlocked.Add(node);
            }
        }
    }
}
