using System;
using System.Collections.Generic;
using KingdomOfGod.Audio;
using UnityEngine;

namespace KingdomOfGod.Core
{
    /// <summary>One narrator line for a given Age, matched by AgeNarrationController.ageLines.</summary>
    [Serializable]
    public struct AgeNarrationLine
    {
        public Age age;
        public VoiceLineData line;
    }

    /// <summary>
    /// Plays a short narrator line each time an Age begins, and an epilogue once the campaign is
    /// complete — AudioManager.PlayVoiceLine already existed with nothing calling it for these.
    /// AgeManager.AgeUnlocked fires the moment a scene-generation-time Awake unlocks Age 1, before
    /// this controller (living in the Kingdom scene, loaded after Bootstrap) exists to hear it, so
    /// Start() plays whatever AgeManager.CurrentAge already is — covering both a new game (Age 1)
    /// and a loaded save (whichever Age it left off at) — while AgeUnlocked live-fires normally for
    /// every later transition reached during the same session.
    /// </summary>
    public class AgeNarrationController : MonoBehaviour
    {
        [SerializeField] private AgeManager ageManager;
        [SerializeField] private List<AgeNarrationLine> ageLines = new List<AgeNarrationLine>();
        [SerializeField] private VoiceLineData campaignEpilogue;

        private void Awake()
        {
            // ageManager lives on the persistent Bootstrap GameManager, in a different scene from
            // this controller — Inspector references can't cross scenes, so fall back to the
            // running singleton when this field was left unassigned.
            if (ageManager == null && GameManager.Instance != null)
            {
                ageManager = GameManager.Instance.Ages;
            }
        }

        private void Start()
        {
            if (ageManager != null) PlayLineFor(ageManager.CurrentAge);
        }

        private void OnEnable()
        {
            if (ageManager != null)
            {
                ageManager.AgeUnlocked += OnAgeUnlocked;
                ageManager.CampaignCompleted += OnCampaignCompleted;
            }
        }

        private void OnDisable()
        {
            if (ageManager != null)
            {
                ageManager.AgeUnlocked -= OnAgeUnlocked;
                ageManager.CampaignCompleted -= OnCampaignCompleted;
            }
        }

        private void OnAgeUnlocked(Age age) => PlayLineFor(age);

        private void OnCampaignCompleted() => GameManager.Instance?.Audio.PlayVoiceLine(campaignEpilogue);

        private void PlayLineFor(Age age)
        {
            foreach (var entry in ageLines)
            {
                if (entry.age == age)
                {
                    GameManager.Instance?.Audio.PlayVoiceLine(entry.line);
                    return;
                }
            }
        }
    }
}
