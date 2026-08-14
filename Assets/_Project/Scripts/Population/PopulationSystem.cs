using System;
using UnityEngine;

namespace KingdomOfGod.Population
{
    /// <summary>Total population and loyalty (0-100%). Low loyalty triggers murmurs/rebellions; high loyalty boosts production.</summary>
    public class PopulationSystem : MonoBehaviour
    {
        [SerializeField] private int startingPopulation = 100;
        [SerializeField] private float startingLoyalty = 60f;
        [SerializeField] private float murmurThreshold = 30f;
        [SerializeField] private float rebellionThreshold = 10f;
        [SerializeField] private float productionBonusThreshold = 80f;

        public int Population { get; private set; }
        public float Loyalty { get; private set; }

        public event Action LoyaltyLow;
        public event Action LoyaltyCritical;
        public event Action<float> LoyaltyChanged;
        public event Action<int> PopulationChanged;

        private void Awake()
        {
            Population = startingPopulation;
            Loyalty = startingLoyalty;
        }

        public void ModifyLoyalty(float delta)
        {
            Loyalty = Mathf.Clamp(Loyalty + delta, 0f, 100f);
            LoyaltyChanged?.Invoke(Loyalty);

            if (Loyalty <= rebellionThreshold) LoyaltyCritical?.Invoke();
            else if (Loyalty <= murmurThreshold) LoyaltyLow?.Invoke();
        }

        public bool HasProductionBonus => Loyalty >= productionBonusThreshold;

        public void Grow(int amount)
        {
            Population = Mathf.Max(0, Population + amount);
            PopulationChanged?.Invoke(Population);
        }
    }
}
