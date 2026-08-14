using System;
using System.Collections.Generic;
using System.Linq;
using KingdomOfGod.Core;
using KingdomOfGod.Resources;
using UnityEngine;

namespace KingdomOfGod.Collectibles
{
    /// <summary>Tracks owned artifacts and fires a bonus/cinematic hook when an age's set is completed.</summary>
    public class CollectionManager : MonoBehaviour
    {
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private List<ArtifactData> allArtifacts = new List<ArtifactData>();

        private readonly HashSet<ArtifactData> owned = new HashSet<ArtifactData>();

        public IReadOnlyCollection<ArtifactData> Owned => owned;
        public event Action<ArtifactData> ArtifactCollected;
        public event Action<Age> AgeCollectionCompleted;

        public void Collect(ArtifactData artifact)
        {
            if (!owned.Add(artifact)) return;

            foreach (var bonus in artifact.passiveBonus)
            {
                resourceManager.Add(bonus.type, bonus.amount);
            }

            ArtifactCollected?.Invoke(artifact);

            if (IsAgeCollectionComplete(artifact.age))
            {
                AgeCollectionCompleted?.Invoke(artifact.age);
            }
        }

        public bool IsAgeCollectionComplete(Age age)
        {
            var artifactsOfAge = allArtifacts.Where(a => a.age == age).ToList();
            return artifactsOfAge.Count > 0 && artifactsOfAge.All(owned.Contains);
        }
    }
}
