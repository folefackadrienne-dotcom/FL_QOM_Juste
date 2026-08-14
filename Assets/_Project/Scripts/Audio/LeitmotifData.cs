using KingdomOfGod.Core;
using UnityEngine;

namespace KingdomOfGod.Audio
{
    /// <summary>
    /// A recurring musical theme tied to a figure or era (GDD "Direction Sonore" — Thèmes
    /// récurrents), e.g. the Thème de l'Alliance or the Thème de David. Reused across whichever
    /// <see cref="MusicThemeData"/> contexts that story beat resurfaces in, rather than owned by
    /// a single one. Clip is left unassigned until a real composition exists.
    /// </summary>
    [CreateAssetMenu(fileName = "Leitmotif_", menuName = "Kingdom of God/Audio/Leitmotif", order = 91)]
    public class LeitmotifData : ScriptableObject
    {
        public string displayName;
        public Age originAge;
        [TextArea] public string description;
        [TextArea] public string recurrenceDescription;
        public AudioClip clip;
    }
}
