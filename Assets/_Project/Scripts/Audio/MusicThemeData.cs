using UnityEngine;

namespace KingdomOfGod.Audio
{
    /// <summary>The situational music layers from the GDD "Direction Sonore" table (section 2).</summary>
    public enum MusicContext
    {
        MainMenu,       // Menu principal
        Exploration,    // Exploration / Paix
        Construction,
        Battle,
        Miracle,
        Crisis,         // Crise / Idolâtrie
        Repentance,     // Repentance / Restauration
        Temple          // Temple / Moments sacrés
    }

    /// <summary>
    /// One situational music layer: the instrumentation and mood for a given gameplay context
    /// (e.g. Battle or Miracle), and the leitmotif that most often carries it. Clip is left
    /// unassigned until a real composition exists — see <see cref="Audio.AudioManager"/> for how
    /// contexts are selected and crossfaded at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "Music_", menuName = "Kingdom of God/Audio/Music Theme", order = 90)]
    public class MusicThemeData : ScriptableObject
    {
        public string displayName;
        public MusicContext context;
        [TextArea] public string instrumentation;
        [TextArea] public string moodDescription;
        public LeitmotifData primaryLeitmotif;
        public AudioClip clip;
    }
}
