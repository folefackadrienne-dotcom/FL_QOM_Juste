using System;
using System.Collections.Generic;
using KingdomOfGod.Alliance;
using KingdomOfGod.Collectibles;
using KingdomOfGod.Core;
using KingdomOfGod.Resources;
using KingdomOfGod.Verses;
using UnityEngine;

namespace KingdomOfGod.Miracles
{
    /// <summary>
    /// Gates and casts miracles per the GDD "Mécaniques des Miracles": checks Faith cost,
    /// required memorized verse/artifact/Alliance standing, and prior moral choices; drives
    /// the multi-turn prayer/charge gauge (steps 1-5 of the ritual) which can be accelerated
    /// with extra Faith or interrupted by an enemy attack; and applies post-cast consequences
    /// (Judgment miracles cast without Justice, and reliance on signs, both erode Alliance).
    /// Unlock was itself a real, callable method with nothing ever calling it — unlockedMiracles
    /// stayed permanently empty, so PrayerMenuUI/BattleHUDController's miracle lists (both filter
    /// from UnlockedMiracles) never had anything to show despite the whole ritual/VFX/SFX pipeline
    /// working. allMiracles (below) plus the Age-unlock wiring is the fix, the same pattern already
    /// used for Buildings/Missions/Tech/Artifacts/Leaders/Verses.
    /// </summary>
    public class MiracleManager : MonoBehaviour
    {
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private VerseManager verseManager;
        [SerializeField] private AllianceSystem allianceSystem;
        [SerializeField] private CollectionManager collectionManager;
        [SerializeField] private AgeManager ageManager;
        [SerializeField] private List<MiracleData> allMiracles = new List<MiracleData>();
        [SerializeField] private List<MiracleData> unlockedMiracles = new List<MiracleData>();

        [Tooltip("Faith cost and prayer duration multiplier applied in the 'dark ages' (Royaumes Divisés, Exil) — Dieu se tait parfois.")]
        [SerializeField] private float darkAgeCostMultiplier = 1.5f;

        [Tooltip("Alliance lost when a Judgment miracle is cast while Justice stock is below judgmentJusticeThreshold.")]
        [SerializeField] private float judgmentWithoutJusticePenalty = 10f;
        [SerializeField] private float judgmentJusticeThreshold = 20f;

        private readonly HashSet<string> fulfilledChoices = new HashSet<string>();
        private readonly HashSet<MiracleData> usedOnceMiracles = new HashSet<MiracleData>();

        public IReadOnlyList<MiracleData> UnlockedMiracles => unlockedMiracles;
        public IReadOnlyCollection<MiracleData> UsedOnceMiracles => usedOnceMiracles;

        /// <summary>The miracle currently being prayed for, or null if no ritual is in progress. Only one can be active at a time.</summary>
        public MiracleData ActiveMiracle { get; private set; }
        public int PrayerProgressTurns { get; private set; }
        public bool IsPraying => ActiveMiracle != null;

        public event Action<MiracleData> MiracleCast;
        public event Action<MiracleData> PrayerStarted;
        public event Action<MiracleData, int> PrayerProgressed;
        public event Action<MiracleData> PrayerInterrupted;
        public event Action<MiracleData> PrayerCancelled;

        private void Start()
        {
            if (ageManager == null) return;

            foreach (var miracle in allMiracles)
            {
                if (miracle != null && ageManager.IsUnlocked(miracle.age)) Unlock(miracle);
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
            foreach (var miracle in allMiracles)
            {
                if (miracle != null && miracle.age == age) Unlock(miracle);
            }
        }

        public void Unlock(MiracleData miracle)
        {
            if (!unlockedMiracles.Contains(miracle)) unlockedMiracles.Add(miracle);
        }

        /// <summary>Reapplies which usableOncePerCampaign miracles were already cast, loaded from a save file and matched by MiracleData.displayName against allMiracles — without this, reloading a save would let a once-per-campaign miracle (e.g. Soleil Arrêté) be cast again.</summary>
        public void RestoreFromSave(IEnumerable<string> savedUsedOnceDisplayNames)
        {
            var nameSet = new HashSet<string>(savedUsedOnceDisplayNames);
            foreach (var miracle in allMiracles)
            {
                if (miracle != null && nameSet.Contains(miracle.displayName)) usedOnceMiracles.Add(miracle);
            }
        }

        public void RecordMoralChoice(string choiceId) => fulfilledChoices.Add(choiceId);

        /// <summary>Dans les Âges sombres (Royaumes Divisés, Exil) les miracles deviennent plus difficiles à déclencher.</summary>
        public bool IsDarkAge => ageManager != null
            && (ageManager.CurrentAge == Age.DividedKingdoms || ageManager.CurrentAge == Age.ExileAndReturn);

        public float GetEffectiveFaithCost(MiracleData miracle)
        {
            return IsDarkAge ? miracle.faithCost * darkAgeCostMultiplier : miracle.faithCost;
        }

        public int GetEffectivePrayerDuration(MiracleData miracle)
        {
            int duration = Mathf.Max(1, miracle.prayerDurationTurns);
            return IsDarkAge ? Mathf.CeilToInt(duration * darkAgeCostMultiplier) : duration;
        }

        public bool CanCast(MiracleData miracle)
        {
            if (miracle == null || !unlockedMiracles.Contains(miracle)) return false;
            if (miracle.usableOncePerCampaign && usedOnceMiracles.Contains(miracle)) return false;
            if (resourceManager.Get(ResourceType.Faith) < GetEffectiveFaithCost(miracle)) return false;
            if (miracle.requiredVerse != null && (verseManager == null || !verseManager.IsMemorized(miracle.requiredVerse))) return false;
            if (!string.IsNullOrEmpty(miracle.requiredPriorChoiceId) && !fulfilledChoices.Contains(miracle.requiredPriorChoiceId)) return false;
            if (miracle.requiredArtifact != null && (collectionManager == null || !collectionManager.Owned.Contains(miracle.requiredArtifact))) return false;
            if (miracle.minAllianceRequired > 0f && (allianceSystem == null || allianceSystem.Value < miracle.minAllianceRequired)) return false;
            return true;
        }

        /// <summary>Instant cast, ignoring the prayer gauge — used where no per-turn ritual is tracked (e.g. legacy/simple contexts).</summary>
        public bool TryCast(MiracleData miracle)
        {
            if (IsPraying || !CanCast(miracle)) return false;
            if (!resourceManager.TrySpend(new[] { new ResourceAmount { type = ResourceType.Faith, amount = GetEffectiveFaithCost(miracle) } })) return false;

            ApplyCastSideEffects(miracle);
            return true;
        }

        /// <summary>Begins the prayer ritual (GDD steps 1-3): pays the Faith cost up front and starts the charge gauge. Only one miracle can be in preparation at a time.</summary>
        public bool BeginPrayer(MiracleData miracle)
        {
            if (IsPraying || !CanCast(miracle)) return false;
            if (!resourceManager.TrySpend(new[] { new ResourceAmount { type = ResourceType.Faith, amount = GetEffectiveFaithCost(miracle) } })) return false;

            ActiveMiracle = miracle;
            PrayerProgressTurns = 0;
            PrayerStarted?.Invoke(miracle);
            return true;
        }

        /// <summary>Advances the active prayer by one turn; resolves into a cast once the gauge is full.</summary>
        public bool AdvancePrayerTurn()
        {
            if (!IsPraying) return false;

            PrayerProgressTurns++;
            if (PrayerProgressTurns >= GetEffectivePrayerDuration(ActiveMiracle))
            {
                ResolvePrayer();
            }
            else
            {
                PrayerProgressed?.Invoke(ActiveMiracle, PrayerProgressTurns);
            }

            return true;
        }

        /// <summary>Spends extra Faith to instantly advance the gauge (huile d'onction, objets, etc.).</summary>
        public bool AccelerateWithFaith(float extraFaith)
        {
            if (!IsPraying) return false;
            if (!resourceManager.TrySpend(new[] { new ResourceAmount { type = ResourceType.Faith, amount = extraFaith } })) return false;

            int duration = GetEffectivePrayerDuration(ActiveMiracle);
            int turnsGained = Mathf.Max(1, Mathf.RoundToInt(extraFaith / Mathf.Max(1f, ActiveMiracle.faithCost) * duration));
            PrayerProgressTurns += turnsGained;

            if (PrayerProgressTurns >= duration)
            {
                ResolvePrayer();
            }
            else
            {
                PrayerProgressed?.Invoke(ActiveMiracle, PrayerProgressTurns);
            }

            return true;
        }

        /// <summary>The ritual was disturbed (e.g. the praying kingdom/army was attacked): the gauge regresses, and the miracle is cancelled outright (the Faith already spent is lost) if it drops below zero.</summary>
        public void InterruptPrayer(int turnsLost = 1)
        {
            if (!IsPraying) return;

            PrayerProgressTurns -= turnsLost;
            if (PrayerProgressTurns < 0)
            {
                var cancelled = ActiveMiracle;
                ActiveMiracle = null;
                PrayerProgressTurns = 0;
                PrayerCancelled?.Invoke(cancelled);
                return;
            }

            PrayerInterrupted?.Invoke(ActiveMiracle);
        }

        private void ResolvePrayer()
        {
            var miracle = ActiveMiracle;
            ActiveMiracle = null;
            PrayerProgressTurns = 0;
            ApplyCastSideEffects(miracle);
        }

        private void ApplyCastSideEffects(MiracleData miracle)
        {
            if (miracle.usableOncePerCampaign) usedOnceMiracles.Add(miracle);

            if (miracle.category == MiracleCategory.Judgment
                && resourceManager.Get(ResourceType.Justice) < judgmentJusticeThreshold)
            {
                allianceSystem?.Modify(-judgmentWithoutJusticePenalty, "miracle de jugement sans justice");
            }

            if (miracle.allianceCostOnCast > 0f)
            {
                allianceSystem?.Modify(-miracle.allianceCostOnCast, "dépendance aux miracles");
            }

            MiracleCast?.Invoke(miracle);
        }
    }
}
