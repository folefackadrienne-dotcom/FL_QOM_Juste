using System.Collections.Generic;
using KingdomOfGod.Resources;
using UnityEngine;

namespace KingdomOfGod.Progression
{
    /// <summary>The 3 top-level technology trees (GDD: Économique, Militaire, Spirituel), each split into 5 branches.</summary>
    public enum TechTreeCategory
    {
        Economic,
        Military,
        Spiritual
    }

    /// <summary>
    /// One node in a technology tree. Prerequisites are referenced by <see cref="techId"/> string
    /// rather than direct ScriptableObject references, so the ~90 nodes across the 3 trees can be
    /// authored as plain data without GUID cross-references between assets; <see cref="TechTree"/>
    /// resolves ids to nodes at runtime against the full node list it's given.
    /// </summary>
    [CreateAssetMenu(fileName = "Tech_", menuName = "Kingdom of God/Tech Node", order = 80)]
    public class TechNode : ScriptableObject
    {
        public string techId;
        public string displayName;
        public TechTreeCategory category;
        public string branch;
        [TextArea] public string effectDescription;
        public string recommendedAge;
        public List<string> prerequisiteIds = new List<string>();

        [Tooltip("Qualitative gate beyond the tech prerequisite chain, e.g. \"Justice élevée\", \"Haute Alliance\" — not yet enforced in code, kept as design data.")]
        [TextArea] public string additionalRequirement;

        public List<ResourceAmount> cost = new List<ResourceAmount>();
    }
}
