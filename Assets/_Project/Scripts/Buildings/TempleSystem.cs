using System;
using System.Collections.Generic;
using KingdomOfGod.Miracles;
using KingdomOfGod.Resources;
using UnityEngine;

namespace KingdomOfGod.Buildings
{
    [Serializable]
    public class TempleLevelData
    {
        [Range(1, 5)] public int level = 1;
        public float faithCapBonus;
        public List<ResourceAmount> upgradeCost = new List<ResourceAmount>();
        public List<MiracleData> miraclesUnlocked = new List<MiracleData>();
        public GameObject prefab;
    }

    /// <summary>The unique Temple building: levels 1-5, each raising the Faith cap and unlocking miracles.</summary>
    public class TempleSystem : MonoBehaviour
    {
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private List<TempleLevelData> levels = new List<TempleLevelData>();
        [SerializeField] private float baseFaithCap = 100f;

        public int CurrentLevel { get; private set; } = 1;

        public event Action<int> LevelUpgraded;

        public TempleLevelData GetLevelData(int level) => levels.Find(l => l.level == level);

        public bool CanUpgrade()
        {
            var next = GetLevelData(CurrentLevel + 1);
            return next != null && resourceManager.CanAfford(next.upgradeCost);
        }

        /// <summary>Upgrades the Temple, spending its sacrifice cost and raising the Faith cap.</summary>
        public bool TryUpgrade()
        {
            var next = GetLevelData(CurrentLevel + 1);
            if (next == null || !resourceManager.TrySpend(next.upgradeCost)) return false;

            CurrentLevel = next.level;
            float totalCap = baseFaithCap;
            for (int i = 1; i <= CurrentLevel; i++)
            {
                totalCap += GetLevelData(i)?.faithCapBonus ?? 0f;
            }
            resourceManager.SetCap(ResourceType.Faith, totalCap);

            LevelUpgraded?.Invoke(CurrentLevel);
            return true;
        }

        /// <summary>Reapplies a Temple level loaded from a save file: sets CurrentLevel and recomputes the Faith cap the same way TryUpgrade does, without spending the upgrade cost again — the save's resource stock already reflects it having been spent once, at the original upgrade.</summary>
        public void RestoreFromSave(int level)
        {
            CurrentLevel = level;
            float totalCap = baseFaithCap;
            for (int i = 1; i <= CurrentLevel; i++)
            {
                totalCap += GetLevelData(i)?.faithCapBonus ?? 0f;
            }
            resourceManager.SetCap(ResourceType.Faith, totalCap);
            LevelUpgraded?.Invoke(CurrentLevel);
        }
    }
}
