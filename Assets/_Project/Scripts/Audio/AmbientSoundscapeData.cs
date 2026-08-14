using UnityEngine;

namespace KingdomOfGod.Audio
{
    /// <summary>
    /// A layered environmental soundscape (GDD "Direction Sonore" — Ambiances), e.g. Désert or
    /// Temple: the list of sound layers a designer should record/source for that setting. Clip
    /// is left unassigned until real recordings exist.
    /// </summary>
    [CreateAssetMenu(fileName = "Ambient_", menuName = "Kingdom of God/Audio/Ambient Soundscape", order = 92)]
    public class AmbientSoundscapeData : ScriptableObject
    {
        public string displayName;
        [TextArea] public string layerDescription;

        [Tooltip("Ambience volume drops automatically during important dialogue or a miracle in progress.")]
        public bool duckedDuringDialogueOrMiracle = true;

        public AudioClip clip;
    }
}
