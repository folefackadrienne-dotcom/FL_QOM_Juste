using System;
using System.Collections.Generic;
using System.Linq;
using KingdomOfGod.Core;
using KingdomOfGod.Resources;
using UnityEngine;

namespace KingdomOfGod.Collectibles
{
    /// <summary>
    /// Tracks owned artifacts and fires a bonus/cinematic hook when an age's set is completed.
    /// Collect was a real, callable method with nothing ever calling it — every artifact would have
    /// stayed uncollected forever. ageManager (below) is the missing trigger: reaching an artifact's
    /// Age reveals and collects it, the same "Age unlocks its content" pattern already used for
    /// buildings/missions/tech, needing no new per-artifact authoring (ArtifactData.age already exists).
    /// </summary>
    public class CollectionManager : MonoBehaviour
    {
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private AgeManager ageManager;
        [SerializeField] private List<ArtifactData> allArtifacts = new List<ArtifactData>();

        private readonly HashSet<ArtifactData> owned = new HashSet<ArtifactData>();

        public IReadOnlyCollection<ArtifactData> Owned => owned;
        public IReadOnlyList<ArtifactData> AllArtifacts => allArtifacts;
        public event Action<ArtifactData> ArtifactCollected;
        public event Action<Age> AgeCollectionCompleted;

        private void Start()
        {
            if (ageManager == null) return;

            foreach (var artifact in allArtifacts)
            {
                if (artifact != null && ageManager.IsUnlocked(artifact.age)) Collect(artifact);
            }
        }

        private void OnEnable()
        {
            if (ageManager != null) ageManager.AgeUnlocked += OnAgeUnlocked;
        }

        private void OnDisable()
        {
            if (ageManager != null) ageManager.AgeUnlocked -= OnAgeUnlocked;
        }

        private void OnAgeUnlocked(Age age)
        {
            foreach (var artifact in allArtifacts)
            {
                if (artifact != null && artifact.age == age) Collect(artifact);
            }
        }

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

        public bool IsOwned(ArtifactData artifact) => owned.Contains(artifact);

        public bool IsAgeCollectionComplete(Age age)
        {
            var artifactsOfAge = allArtifacts.Where(a => a.age == age).ToList();
            return artifactsOfAge.Count > 0 && artifactsOfAge.All(owned.Contains);
        }

        /// <summary>Reapplies owned artifacts loaded from a save file, matched by ArtifactData.displayName against allArtifacts — marks each owned directly, bypassing Collect's passiveBonus grant since the save's resource stock already reflects it.</summary>
        public void RestoreFromSave(IEnumerable<string> savedOwnedDisplayNames)
        {
            var nameSet = new HashSet<string>(savedOwnedDisplayNames);
            foreach (var artifact in allArtifacts)
            {
                if (nameSet.Contains(artifact.displayName)) owned.Add(artifact);
            }
        }
    }
}
