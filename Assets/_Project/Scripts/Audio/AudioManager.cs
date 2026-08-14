using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KingdomOfGod.Alliance;
using KingdomOfGod.Buildings;
using KingdomOfGod.Miracles;
using KingdomOfGod.Resources;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KingdomOfGod.Audio
{
    /// <summary>
    /// Drives the layered music/ambience from the GDD "Direction Sonore": crossfades between
    /// situational <see cref="MusicThemeData"/> layers as the player moves between scenes, prays
    /// a miracle, or falls in/out of Alliance standing, and ducks ambience while a miracle is in
    /// preparation. AudioClip fields on the theme/soundscape data are left unassigned until real
    /// compositions exist — this component only drives the mixing logic ("mixage dynamique").
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private MiracleManager miracleManager;
        [SerializeField] private AllianceSystem allianceSystem;
        [SerializeField] private BuildingManager buildingManager;
        [SerializeField] private ResourceManager resourceManager;

        [SerializeField] private List<MusicThemeData> musicThemes = new List<MusicThemeData>();
        [SerializeField] private List<AmbientSoundscapeData> ambientSoundscapes = new List<AmbientSoundscapeData>();
        [SerializeField] private List<SfxCueData> sfxCues = new List<SfxCueData>();

        [SerializeField] private float crossfadeSeconds = 2f;
        [SerializeField] private float duckedVolumeScale = 0.35f;

        private AudioSource musicSourceA;
        private AudioSource musicSourceB;
        private AudioSource activeMusicSource;
        private AudioSource ambientSource;
        private AudioSource sfxSource;
        private Coroutine crossfadeRoutine;
        private int duckRequests;

        private MusicContext sceneContext = MusicContext.MainMenu;
        private MusicContext? allianceOverride;
        private float lastFaithValue;
        private float lastAllianceValue;

        public MusicContext CurrentContext => allianceOverride ?? sceneContext;

        private void Awake()
        {
            musicSourceA = gameObject.AddComponent<AudioSource>();
            musicSourceB = gameObject.AddComponent<AudioSource>();
            musicSourceA.loop = true;
            musicSourceB.loop = true;
            activeMusicSource = musicSourceA;

            ambientSource = GetComponent<AudioSource>();
            ambientSource.loop = true;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (miracleManager != null)
            {
                miracleManager.PrayerStarted += OnPrayerStarted;
                miracleManager.MiracleCast += OnMiracleCast;
                miracleManager.PrayerCancelled += OnPrayerCancelled;
                miracleManager.PrayerInterrupted += OnPrayerInterrupted;
            }

            if (allianceSystem != null)
            {
                lastAllianceValue = allianceSystem.Value;
                allianceSystem.StandingChanged += OnAllianceStandingChanged;
                allianceSystem.ValueChanged += OnAllianceValueChanged;
            }

            if (buildingManager != null) buildingManager.BuildingPlaced += OnBuildingPlaced;

            if (resourceManager != null)
            {
                lastFaithValue = resourceManager.Get(ResourceType.Faith);
                resourceManager.ResourceChanged += OnResourceChanged;
            }
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (miracleManager != null)
            {
                miracleManager.PrayerStarted -= OnPrayerStarted;
                miracleManager.MiracleCast -= OnMiracleCast;
                miracleManager.PrayerCancelled -= OnPrayerCancelled;
                miracleManager.PrayerInterrupted -= OnPrayerInterrupted;
            }

            if (allianceSystem != null)
            {
                allianceSystem.StandingChanged -= OnAllianceStandingChanged;
                allianceSystem.ValueChanged -= OnAllianceValueChanged;
            }

            if (buildingManager != null) buildingManager.BuildingPlaced -= OnBuildingPlaced;
            if (resourceManager != null) resourceManager.ResourceChanged -= OnResourceChanged;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            sceneContext = scene.name switch
            {
                "MainMenu" => MusicContext.MainMenu,
                "Battle" => MusicContext.Battle,
                "Kingdom" => MusicContext.Exploration,
                _ => sceneContext
            };

            PlayContext(CurrentContext);
        }

        /// <summary>"En cas de crise morale ou d'idolâtrie, le mixage devient plus étouffé" — Low Alliance overrides whatever context the scene would otherwise play.</summary>
        private void OnAllianceStandingChanged(AllianceStanding standing)
        {
            allianceOverride = standing == AllianceStanding.Low ? MusicContext.Crisis : (MusicContext?)null;
            PlayContext(CurrentContext);
        }

        /// <summary>"Quand la jauge de Foi augmente : note claire et chaude / quand elle baisse : dissonance légère" — same treatment applied to the Alliance gauge.</summary>
        private void OnAllianceValueChanged(float newValue)
        {
            if (newValue > lastAllianceValue) PlaySfx("Foi & Alliance - Alliance en Hausse");
            else if (newValue < lastAllianceValue) PlaySfx("Foi & Alliance - Alliance en Baisse");
            lastAllianceValue = newValue;
        }

        private void OnResourceChanged(ResourceType type, float newValue)
        {
            if (type != ResourceType.Faith) return;

            if (newValue > lastFaithValue) PlaySfx("Foi & Alliance - Foi en Hausse");
            else if (newValue < lastFaithValue) PlaySfx("Foi & Alliance - Foi en Baisse");
            lastFaithValue = newValue;
        }

        private void OnPrayerStarted(MiracleData miracle)
        {
            SetDucked(true);
            PlaySfx("Miracle - Début de Prière");
            PlayContext(MusicContext.Miracle);
        }

        /// <summary>Doesn't un-duck or leave the Miracle context — the ritual is still in progress, just set back a turn.</summary>
        private void OnPrayerInterrupted(MiracleData miracle)
        {
            PlaySfx("Miracle - Interruption");
        }

        private void OnMiracleCast(MiracleData miracle)
        {
            SetDucked(false);
            PlaySfx("Miracle - Déclenchement");
            PlayContext(CurrentContext);
        }

        private void OnPrayerCancelled(MiracleData miracle)
        {
            SetDucked(false);
            PlaySfx("Miracle - Annulation");
            PlayContext(CurrentContext);
        }

        /// <summary>"Quand un bâtiment important est terminé : petite fanfare douce + chœur très léger" — Spiritual/Special buildings get the fanfare cue, everything else the plain placement cue.</summary>
        private void OnBuildingPlaced(BuildingInstance instance)
        {
            bool important = instance.Data.category == BuildingCategory.Spiritual || instance.Data.category == BuildingCategory.Special;
            PlaySfx(important ? "Construction - Bâtiment Important Terminé" : "Construction - Placement d'un Bâtiment");
        }

        /// <summary>Lets a future dialogue/cutscene system ask for the ambience to duck alongside miracles without the two stepping on each other's un-duck.</summary>
        public void SetDialogueDucked(bool ducked) => SetDucked(ducked);

        private void SetDucked(bool ducked)
        {
            duckRequests = Mathf.Max(0, duckRequests + (ducked ? 1 : -1));
            ambientSource.volume = duckRequests > 0 ? duckedVolumeScale : 1f;
        }

        public void PlayContext(MusicContext context)
        {
            var theme = musicThemes.FirstOrDefault(t => t.context == context);
            if (theme == null || theme.clip == null) return;

            var next = activeMusicSource == musicSourceA ? musicSourceB : musicSourceA;
            next.clip = theme.clip;
            next.volume = 0f;
            next.Play();

            if (crossfadeRoutine != null) StopCoroutine(crossfadeRoutine);
            crossfadeRoutine = StartCoroutine(Crossfade(activeMusicSource, next));
            activeMusicSource = next;
        }

        public void PlayAmbient(string displayName)
        {
            var soundscape = ambientSoundscapes.FirstOrDefault(a => a.displayName == displayName);
            if (soundscape == null || soundscape.clip == null) return;

            ambientSource.clip = soundscape.clip;
            ambientSource.Play();
        }

        public void PlaySfx(SfxCueData cue)
        {
            if (cue == null || cue.clip == null) return;
            sfxSource.PlayOneShot(cue.clip);
        }

        public void PlaySfx(string displayName)
        {
            PlaySfx(sfxCues.FirstOrDefault(s => s.displayName == displayName));
        }

        private IEnumerator Crossfade(AudioSource from, AudioSource to)
        {
            float duration = Mathf.Max(0.01f, crossfadeSeconds);
            float fromStartVolume = from.isPlaying ? from.volume : 0f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float ratio = Mathf.Clamp01(t / duration);
                to.volume = ratio;
                from.volume = fromStartVolume * (1f - ratio);
                yield return null;
            }

            from.Stop();
        }
    }
}
