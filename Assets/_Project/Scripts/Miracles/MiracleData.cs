using KingdomOfGod.Collectibles;
using KingdomOfGod.Core;
using KingdomOfGod.Verses;
using UnityEngine;

namespace KingdomOfGod.Miracles
{
    public enum MiracleContext
    {
        Battle,
        Kingdom,
        Both
    }

    /// <summary>The 5 gameplay families of miracles (GDD "Mécaniques des Miracles" section 2).</summary>
    public enum MiracleCategory
    {
        Deliverance,   // Délivrance — protection et sauvetage
        Judgment,      // Jugement — offensif, punition des ennemis
        Provision,     // Provision — économique / survie
        Power,         // Puissance — amplification temporaire
        Restoration    // Restauration — soin et rétablissement
    }

    /// <summary>
    /// Static definition of a miracle (e.g. Mer Rouge, Chute de Jéricho, Soleil arrêté).
    /// A cast always rests on the 4 pillars from the GDD: Faith cost, activation conditions
    /// (Faith level, memorized verse, required artifact, Alliance standing, prior choice),
    /// a prayer/charge duration, and consequences (a one-time-use limit and/or an Alliance
    /// cost for the most powerful miracles). See <see cref="Miracles.MiracleManager"/> for
    /// how these are enforced and resolved.
    /// </summary>
    [CreateAssetMenu(fileName = "Miracle_", menuName = "Kingdom of God/Miracle", order = 30)]
    public class MiracleData : ScriptableObject
    {
        public string displayName;
        [TextArea] public string effectDescription;
        public MiracleContext context;
        public MiracleCategory category;

        [Tooltip("Which Age's story this miracle belongs to — MiracleManager unlocks it the moment this Age does, the same AgeManager.AgeUnlocked pattern used for Verses/Artifacts/Leaders.")]
        public Age age;

        public float faithCost;

        [Range(1, 4)]
        [Tooltip("Prayer/charge duration in turns before the miracle resolves (GDD: 1 à 4 tours selon la puissance).")]
        public int prayerDurationTurns = 1;

        public VerseData requiredVerse;
        public ArtifactData requiredArtifact;

        [Range(0, 100)]
        [Tooltip("Minimum Alliance value required to cast (0 = no requirement).")]
        public float minAllianceRequired;

        [Tooltip("Can only be cast once per campaign (e.g. Soleil Arrêté).")]
        public bool usableOncePerCampaign;

        [Tooltip("Alliance lost after a successful cast — the people become too dependent on signs (0 = no cost).")]
        public float allianceCostOnCast;

        [TextArea] public string requiredPriorChoiceId;
        public Sprite icon;

        [Tooltip("Unique sonic signature for this miracle's cast (GDD Audio Design section 4: 'Chaque miracle a une signature sonore unique et mémorable'). Left empty for miracles without a bespoke signature yet.")]
        [TextArea] public string audioSignatureDescription;
    }
}
