using UnityEngine;

namespace KingdomOfGod.Audio
{
    /// <summary>The SFX families from the GDD "Direction Sonore" section 4.</summary>
    public enum SfxCategory
    {
        Interface,
        Construction,
        Battle,
        Miracle,
        FaithAlliance
    }

    /// <summary>
    /// One short one-shot sound effect (as opposed to the looping <see cref="MusicThemeData"/>/
    /// <see cref="AmbientSoundscapeData"/> layers): a UI click, a validation chime, a building
    /// placement thud, etc. Clip is left unassigned until a real recording exists — see
    /// <see cref="Audio.AudioManager.PlaySfx(SfxCueData)"/> for how cues are triggered.
    /// </summary>
    [CreateAssetMenu(fileName = "Sfx_", menuName = "Kingdom of God/Audio/SFX Cue", order = 93)]
    public class SfxCueData : ScriptableObject
    {
        public string displayName;
        public SfxCategory category;
        [TextArea] public string description;
        public AudioClip clip;
    }
}
