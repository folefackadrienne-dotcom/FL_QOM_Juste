using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KingdomOfGod.Alliance;
using KingdomOfGod.Buildings;
using KingdomOfGod.Miracles;
using KingdomOfGod.Missions;
using KingdomOfGod.Monetization;
using KingdomOfGod.Population;
using KingdomOfGod.Progression;
using KingdomOfGod.Resources;
using KingdomOfGod.SaveSystem;
using KingdomOfGod.Verses;
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
        [SerializeField] private TechTree techTree;
        [SerializeField] private LeaderManager leaderManager;
        [SerializeField] private PopulationSystem populationSystem;
        [SerializeField] private TempleSystem templeSystem;
        [SerializeField] private MissionManager missionManager;
        [SerializeField] private VerseManager verseManager;
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private EntitlementManager entitlementManager;

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
        private AudioSource voiceSource;
        private Coroutine crossfadeRoutine;
        private int duckRequests;
        private bool narrationActive;

        private MusicContext sceneContext = MusicContext.MainMenu;
        private MusicContext? allianceOverride;
        private float lastFaithValue;
        private float lastAllianceValue;
        private int lastPopulationValue;

        public MusicContext CurrentContext => allianceOverride ?? sceneContext;

        /// <summary>GDD "Direction Sonore" section 5: "Français (priorité)", English, and Hebrew are the 3 supported voice languages.</summary>
        public VoiceLanguage CurrentLanguage { get; private set; } = VoiceLanguage.French;

        public void SetLanguage(VoiceLanguage language) => CurrentLanguage = language;

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

            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.loop = false;
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
                allianceSystem.Repented += OnRepented;
            }

            if (buildingManager != null) buildingManager.BuildingPlaced += OnBuildingPlaced;

            if (resourceManager != null)
            {
                lastFaithValue = resourceManager.Get(ResourceType.Faith);
                resourceManager.ResourceChanged += OnResourceChanged;
            }

            if (techTree != null) techTree.TechUnlocked += OnTechUnlocked;

            if (leaderManager != null)
            {
                leaderManager.LeaderUnlocked += OnLeaderUnlocked;
                leaderManager.LeaderActivated += OnLeaderActivated;
            }

            if (populationSystem != null)
            {
                lastPopulationValue = populationSystem.Population;
                populationSystem.PopulationChanged += OnPopulationChanged;
                populationSystem.LoyaltyLow += OnLoyaltyLow;
                populationSystem.LoyaltyCritical += OnLoyaltyCritical;
            }

            if (templeSystem != null) templeSystem.LevelUpgraded += OnTempleLevelUpgraded;

            if (missionManager != null)
            {
                missionManager.MissionStarted += OnMissionStarted;
                missionManager.MissionCompleted += OnMissionCompleted;
            }

            if (verseManager != null)
            {
                verseManager.VerseUnlocked += OnVerseUnlocked;
                verseManager.VerseMemorized += OnVerseMemorized;
            }

            if (saveManager != null)
            {
                saveManager.Saved += OnSaved;
                saveManager.Loaded += OnLoaded;
            }

            if (entitlementManager != null)
            {
                entitlementManager.ProductPurchased += OnProductPurchased;
                entitlementManager.TierChanged += OnTierChanged;
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
                allianceSystem.Repented -= OnRepented;
            }

            if (buildingManager != null) buildingManager.BuildingPlaced -= OnBuildingPlaced;
            if (resourceManager != null) resourceManager.ResourceChanged -= OnResourceChanged;
            if (techTree != null) techTree.TechUnlocked -= OnTechUnlocked;

            if (leaderManager != null)
            {
                leaderManager.LeaderUnlocked -= OnLeaderUnlocked;
                leaderManager.LeaderActivated -= OnLeaderActivated;
            }

            if (populationSystem != null)
            {
                populationSystem.PopulationChanged -= OnPopulationChanged;
                populationSystem.LoyaltyLow -= OnLoyaltyLow;
                populationSystem.LoyaltyCritical -= OnLoyaltyCritical;
            }

            if (templeSystem != null) templeSystem.LevelUpgraded -= OnTempleLevelUpgraded;

            if (missionManager != null)
            {
                missionManager.MissionStarted -= OnMissionStarted;
                missionManager.MissionCompleted -= OnMissionCompleted;
            }

            if (verseManager != null)
            {
                verseManager.VerseUnlocked -= OnVerseUnlocked;
                verseManager.VerseMemorized -= OnVerseMemorized;
            }

            if (saveManager != null)
            {
                saveManager.Saved -= OnSaved;
                saveManager.Loaded -= OnLoaded;
            }

            if (entitlementManager != null)
            {
                entitlementManager.ProductPurchased -= OnProductPurchased;
                entitlementManager.TierChanged -= OnTierChanged;
            }
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

        /// <summary>"En cas de crise morale ou d'idolâtrie, le mixage devient plus étouffé" — Low Alliance overrides whatever context the scene would otherwise play, plus a one-shot sting for crossing into or out of the extreme bands.</summary>
        private void OnAllianceStandingChanged(AllianceStanding standing)
        {
            allianceOverride = standing == AllianceStanding.Low ? MusicContext.Crisis : (MusicContext?)null;
            PlayContext(CurrentContext);

            if (standing == AllianceStanding.Low) PlaySfx("Alliance - Entrée en Crise");
            else if (standing == AllianceStanding.High) PlaySfx("Alliance - Faveur Élevée");
        }

        /// <summary>"Passage de l'ombre à la lumière" — plays in addition to (not instead of) the generic Alliance-en-Hausse tick from OnAllianceValueChanged, since a deliberate repentance is a distinct, more significant moment.</summary>
        private void OnRepented()
        {
            PlaySfx("Alliance - Repentance / Restauration");
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

        /// <summary>One chime per tech tree, matching its character — the same "vary by category" treatment already used for Construction.</summary>
        private void OnTechUnlocked(TechNode node)
        {
            PlaySfx(node.category switch
            {
                TechTreeCategory.Military => "Progression - Technologie Militaire Débloquée",
                TechTreeCategory.Spiritual => "Progression - Technologie Spirituelle Débloquée",
                _ => "Progression - Technologie Économique Débloquée"
            });
        }

        private void OnLeaderUnlocked(LeaderData leader)
        {
            PlaySfx("Progression - Leader Débloqué");
        }

        private void OnLeaderActivated(LeaderData leader)
        {
            PlaySfx("Progression - Leader Actif");
        }

        private void OnPopulationChanged(int newValue)
        {
            if (newValue > lastPopulationValue) PlaySfx("Économie - Population en Hausse");
            else if (newValue < lastPopulationValue) PlaySfx("Économie - Population en Baisse");
            lastPopulationValue = newValue;
        }

        /// <summary>"Pénurie = murmures et baisse de loyauté" (GDD Economy) — loyalty dropping into the murmur band.</summary>
        private void OnLoyaltyLow()
        {
            PlaySfx("Économie - Murmures du Peuple");
        }

        private void OnLoyaltyCritical()
        {
            PlaySfx("Économie - Rébellion Imminente");
        }

        private void OnTempleLevelUpgraded(int level)
        {
            PlaySfx("Bâtiments - Temple Amélioré");
        }

        private void OnMissionStarted(MissionData mission)
        {
            PlaySfx("Missions - Mission Commencée");
        }

        private void OnMissionCompleted(MissionData mission)
        {
            PlaySfx("Missions - Mission Accomplie");
        }

        private void OnVerseUnlocked(VerseData verse)
        {
            PlaySfx("Versets - Verset Débloqué");
        }

        /// <summary>"Récompense : bonus permanent + accès Bibliothèque de la Torah" — the mini-game's completion moment.</summary>
        private void OnVerseMemorized(VerseData verse)
        {
            PlaySfx("Versets - Verset Mémorisé");
        }

        private void OnSaved(SaveData data)
        {
            PlaySfx("Sauvegarde - Partie Sauvegardée");
        }

        private void OnLoaded(SaveData data)
        {
            PlaySfx("Sauvegarde - Partie Chargée");
        }

        private void OnProductPurchased(ProductData product)
        {
            PlaySfx("Monétisation - Achat Réussi");
        }

        /// <summary>Plays in addition to (not instead of) Achat Réussi from OnProductPurchased, since crossing into Full Edition is a distinct, more significant moment — same layering as OnRepented over the generic Alliance-en-Hausse tick.</summary>
        private void OnTierChanged(EntitlementTier tier)
        {
            if (tier == EntitlementTier.FullEdition) PlaySfx("Monétisation - Édition Complète Débloquée");
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

        /// <summary>Plays a narrator or character line once, in <see cref="CurrentLanguage"/> (falling back to French if that language's clip is missing).</summary>
        public void PlayVoiceLine(VoiceLineData line)
        {
            if (line == null) return;

            var clip = PickLanguageClip(line.clipFrench, line.clipEnglish, line.clipHebrew);
            if (clip == null) return;

            voiceSource.loop = false;
            voiceSource.clip = clip;
            voiceSource.Play();
        }

        /// <summary>"Les versets mémorisés peuvent être écoutés en boucle avec une musique très douce en fond" — loops the reading and ducks the other audio layers for the duration.</summary>
        public void PlayVerseNarration(VerseData verse)
        {
            if (verse == null) return;

            var clip = PickLanguageClip(verse.narrationClipFrench, verse.narrationClipEnglish, verse.narrationClipHebrew);
            if (clip == null) return;

            voiceSource.loop = true;
            voiceSource.clip = clip;
            voiceSource.Play();

            if (!narrationActive)
            {
                narrationActive = true;
                SetDucked(true);
            }
        }

        public void StopNarration()
        {
            if (!narrationActive) return;

            voiceSource.Stop();
            narrationActive = false;
            SetDucked(false);
        }

        private AudioClip PickLanguageClip(AudioClip french, AudioClip english, AudioClip hebrew)
        {
            var preferred = CurrentLanguage switch
            {
                VoiceLanguage.English => english,
                VoiceLanguage.Hebrew => hebrew,
                _ => french
            };

            return preferred != null ? preferred : french;
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
