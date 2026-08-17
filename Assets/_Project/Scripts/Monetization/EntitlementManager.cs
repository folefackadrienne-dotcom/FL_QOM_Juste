using System;
using System.Collections.Generic;
using KingdomOfGod.Core;
using UnityEngine;

namespace KingdomOfGod.Monetization
{
    /// <summary>
    /// Tracks what the player has purchased and gates content accordingly.
    /// Free tier: ages up to <see cref="freeAgeLimit"/> (GDD: "Accès aux 2-3 premiers
    /// âges"). Buying <see cref="fullEditionProduct"/> unlocks every age and Mode Libre.
    /// Cosmetic/Battle Pass products only ever add owned items, never gameplay power.
    /// GateChanged fires alongside TierChanged so AgeManager can retry a previously-refused
    /// UnlockAge the moment a purchase completes — without it, a player who finished a free-tier
    /// age's last mission before buying would stay stuck forever: nothing left to complete to
    /// retrigger the blocked check, and the purchase itself never nudged AgeManager to look again.
    /// </summary>
    public class EntitlementManager : MonoBehaviour, IContentGate
    {
        [SerializeField] private Age freeAgeLimit = Age.ExodusAndDesert;
        [SerializeField] private ProductData fullEditionProduct;

        private readonly HashSet<string> ownedProductIds = new HashSet<string>();
        private IIAPService iapService;

        public EntitlementTier Tier { get; private set; } = EntitlementTier.Free;
        public bool IsFullEdition => Tier == EntitlementTier.FullEdition;

        public event Action<EntitlementTier> TierChanged;
        public event Action<ProductData> ProductPurchased;

        /// <summary>IContentGate.GateChanged — fires alongside TierChanged so AgeManager can retry a previously-refused UnlockAge without this class knowing anything about ages.</summary>
        public event Action GateChanged;

        private void Awake()
        {
            // TODO: swap for a real store-backed IIAPService once Unity IAP (or another
            // store SDK) is installed and configured; nothing else in this class changes.
            iapService = new EditorIAPService();
            iapService.Initialize();
        }

        public bool CanUnlockAge(Age age) => IsFullEdition || age <= freeAgeLimit;

        public bool Owns(ProductData product) => ownedProductIds.Contains(product.storeProductId);

        public void PurchaseFullEdition(Action<bool> onComplete = null)
        {
            Purchase(fullEditionProduct, success =>
            {
                if (success) GrantFullEdition();
                onComplete?.Invoke(success);
            });
        }

        public void Purchase(ProductData product, Action<bool> onComplete = null)
        {
            iapService.Purchase(product, success =>
            {
                if (success)
                {
                    ownedProductIds.Add(product.storeProductId);
                    ProductPurchased?.Invoke(product);
                }
                onComplete?.Invoke(success);
            });
        }

        public void RestorePurchases(Action<bool> onComplete = null) => iapService.RestorePurchases(onComplete);

        private void GrantFullEdition()
        {
            if (Tier == EntitlementTier.FullEdition) return;

            Tier = EntitlementTier.FullEdition;
            TierChanged?.Invoke(Tier);
            GateChanged?.Invoke();
        }

        /// <summary>Reapplies a tier and owned product list loaded from a save file.</summary>
        public void RestoreFromSave(EntitlementTier tier, IEnumerable<string> savedProductIds)
        {
            Tier = tier;
            ownedProductIds.Clear();
            foreach (var id in savedProductIds) ownedProductIds.Add(id);
            TierChanged?.Invoke(Tier);
            GateChanged?.Invoke();
        }

        public IReadOnlyCollection<string> OwnedProductIds => ownedProductIds;
    }
}
