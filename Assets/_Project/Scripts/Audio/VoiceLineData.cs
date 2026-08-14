using KingdomOfGod.Progression;
using KingdomOfGod.Verses;
using UnityEngine;

namespace KingdomOfGod.Audio
{
    /// <summary>The 3 languages from the GDD "Direction Sonore" section 5, French first per "Français (priorité)".</summary>
    public enum VoiceLanguage
    {
        French,
        English,
        Hebrew
    }

    /// <summary>Who is speaking a <see cref="VoiceLineData"/> line (GDD section 5: "Narrateur principal" vs "Voix des personnages importants").</summary>
    public enum VoiceRole
    {
        Narrator,
        Character
    }

    /// <summary>
    /// One spoken line — narrator or character, never both — with a clip per supported language.
    /// Only "phrases clés" are expected to be voiced (GDD: characters are "pas forcément 100 %
    /// doublés"), not full dialogue. Clip fields are left unassigned until a real recording
    /// exists; <see cref="Audio.AudioManager.PlayVoiceLine"/> picks the clip matching the current
    /// language, falling back to French if that language's clip is missing.
    /// </summary>
    [CreateAssetMenu(fileName = "Voice_", menuName = "Kingdom of God/Audio/Voice Line", order = 94)]
    public class VoiceLineData : ScriptableObject
    {
        public string displayName;
        public VoiceRole role;

        [Tooltip("Leave empty for the narrator or an unattributed character line.")]
        public LeaderData speaker;

        [Tooltip("Optional biblical source when the line quotes a verse already in the game (e.g. a leader's signature line).")]
        public VerseData relatedVerse;

        [TextArea] public string lineText;

        public AudioClip clipFrench;
        public AudioClip clipEnglish;
        public AudioClip clipHebrew;
    }
}
