using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KingdomOfGod.Resources
{
    [Serializable]
    public struct ResourceAmount
    {
        public ResourceType type;
        public float amount;
    }

    /// <summary>Central store for the 7 kingdom resources: stock, per-resource caps, and spend/gain events.</summary>
    public class ResourceManager : MonoBehaviour
    {
        [SerializeField] private List<ResourceAmount> startingStock = new List<ResourceAmount>();
        [SerializeField] private float defaultCap = 999f;

        private readonly Dictionary<ResourceType, float> stock = new Dictionary<ResourceType, float>();
        private readonly Dictionary<ResourceType, float> caps = new Dictionary<ResourceType, float>();

        public event Action<ResourceType, float> ResourceChanged;

        private void Awake()
        {
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                stock[type] = 0f;
                caps[type] = defaultCap;
            }

            foreach (var entry in startingStock)
            {
                stock[entry.type] = entry.amount;
            }
        }

        public float Get(ResourceType type) => stock[type];

        public float GetCap(ResourceType type) => caps[type];

        public void SetCap(ResourceType type, float cap)
        {
            caps[type] = cap;
            stock[type] = Mathf.Min(stock[type], cap);
        }

        public void Add(ResourceType type, float amount)
        {
            if (amount < 0f)
            {
                Spend(type, -amount);
                return;
            }

            stock[type] = Mathf.Min(stock[type] + amount, caps[type]);
            ResourceChanged?.Invoke(type, stock[type]);
        }

        public bool CanAfford(IEnumerable<ResourceAmount> cost)
        {
            return cost.All(entry => stock[entry.type] >= entry.amount);
        }

        /// <summary>Attempts to spend a full bundle of resources atomically; returns false and spends nothing if unaffordable.</summary>
        public bool TrySpend(IEnumerable<ResourceAmount> cost)
        {
            var costList = cost as IList<ResourceAmount> ?? cost.ToList();
            if (!CanAfford(costList)) return false;

            foreach (var entry in costList)
            {
                Spend(entry.type, entry.amount);
            }

            return true;
        }

        public void Spend(ResourceType type, float amount)
        {
            stock[type] = Mathf.Max(0f, stock[type] - amount);
            ResourceChanged?.Invoke(type, stock[type]);
        }

        /// <summary>Overwrites stock directly from a save file — bypasses Add/Spend's cap clamp, since a saved amount can legitimately exceed the default cap once storage-bonus buildings/Temple levels have been restored. Call after RestoreFromSave on BuildingManager/TempleSystem so those bonuses are already applied.</summary>
        public void RestoreStock(IEnumerable<ResourceAmount> savedStock)
        {
            foreach (var entry in savedStock)
            {
                stock[entry.type] = entry.amount;
                ResourceChanged?.Invoke(entry.type, stock[entry.type]);
            }
        }
    }
}
