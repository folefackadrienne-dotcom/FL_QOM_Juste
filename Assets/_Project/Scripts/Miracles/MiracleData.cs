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

    /// <summary>
    /// Static definition of a miracle (e.g. Mer Rouge, Chute de Jéricho, Soleil arrêté).
    /// Activation is conditional: minimum Faith, and optionally a specific memorized verse
    /// or a prior moral choice — see GDD "Système de Miracles".
    /// </summary>
    [CreateAssetMenu(fileName = "Miracle_", menuName = "Kingdom of God/Miracle", order = 30)]
    public class MiracleData : ScriptableObject
    {
        public string displayName;
        [TextArea] public string effectDescription;
        public MiracleContext context;
        public float faithCost;
        public VerseData requiredVerse;
        [TextArea] public string requiredPriorChoiceId;
        public Sprite icon;
    }
}
