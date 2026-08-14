using System.Collections.Generic;
using KingdomOfGod.Core;
using KingdomOfGod.Resources;
using UnityEngine;

namespace KingdomOfGod.Collectibles
{
    public enum Rarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>A collectible biblical artifact (e.g. Bâton de Moïse, Frondes de David) with a passive bonus and lore.</summary>
    [CreateAssetMenu(fileName = "Artifact_", menuName = "Kingdom of God/Artifact", order = 50)]
    public class ArtifactData : ScriptableObject
    {
        public string displayName;
        public string biblicalReference;
        [TextArea] public string historicalContext;
        [TextArea] public string educationalComment;
        public Rarity rarity;
        public Age age;
        public Sprite icon;

        [Header("Effect")]
        public List<ResourceAmount> passiveBonus = new List<ResourceAmount>();
        [TextArea] public string activeAbilityDescription;
    }
}
