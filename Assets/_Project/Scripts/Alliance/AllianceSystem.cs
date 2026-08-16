using System;
using KingdomOfGod.Resources;
using UnityEngine;

namespace KingdomOfGod.Alliance
{
    public enum AllianceStanding
    {
        Low,    // malédictions, invasions, rébellions, prophètes de jugement
        Medium,
        High    // miracles plus puissants, événements positifs, loyauté haute
    }

    /// <summary>
    /// The 0-100 Alliance gauge (jauge d'Alliance): the core moral/spiritual meter driven by
    /// obedience, justice, prayer and verse memorization, and eroded by idolatry, injustice and pride.
    /// The player can always repent to restore it, at a cost.
    /// </summary>
    public class AllianceSystem : MonoBehaviour
    {
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private float startingValue = 50f;
        [SerializeField] private float lowThreshold = 30f;
        [SerializeField] private float highThreshold = 70f;
        [SerializeField] private ResourceAmount repentanceCost = new ResourceAmount { type = ResourceType.Gold, amount = 50f };
        [SerializeField] private float repentanceRestoreAmount = 20f;

        public float Value { get; private set; }
        public AllianceStanding Standing { get; private set; }

        /// <summary>Alliance multiplies the power of miracles (GDD "Mécaniques des Miracles" section 8): stronger while High, dampened while Low.</summary>
        public float MiraclePowerMultiplier => Standing switch
        {
            AllianceStanding.High => 1.5f,
            AllianceStanding.Low => 0.75f,
            _ => 1f
        };

        public event Action<float> ValueChanged;
        public event Action<AllianceStanding> StandingChanged;

        /// <summary>Fired specifically on a successful repentance, distinct from ValueChanged, so listeners (e.g. audio) can tell a deliberate act of repentance apart from any other Alliance gain.</summary>
        public event Action Repented;

        private void Awake()
        {
            Value = startingValue;
            Standing = ComputeStanding(Value);
        }

        public void Modify(float delta, string reason = null)
        {
            Value = Mathf.Clamp(Value + delta, 0f, 100f);
            ValueChanged?.Invoke(Value);

            var newStanding = ComputeStanding(Value);
            if (newStanding != Standing)
            {
                Standing = newStanding;
                StandingChanged?.Invoke(Standing);
            }
        }

        private AllianceStanding ComputeStanding(float value)
        {
            if (value <= lowThreshold) return AllianceStanding.Low;
            return value >= highThreshold ? AllianceStanding.High : AllianceStanding.Medium;
        }

        /// <summary>Spends the repentance cost to restore some Alliance after a fall — mechanic is always available.</summary>
        public bool TryRepent()
        {
            if (!resourceManager.TrySpend(new[] { repentanceCost })) return false;

            Modify(repentanceRestoreAmount, "repentance");
            Repented?.Invoke();
            return true;
        }

        /// <summary>Sets Value directly from a save file and recomputes Standing — no resource cost involved here, so unlike the other restore methods this one doesn't need to bypass a side effect, just the startingValue set in Awake.</summary>
        public void RestoreFromSave(float savedValue)
        {
            Value = Mathf.Clamp(savedValue, 0f, 100f);
            ValueChanged?.Invoke(Value);
            Standing = ComputeStanding(Value);
            StandingChanged?.Invoke(Standing);
        }
    }
}
