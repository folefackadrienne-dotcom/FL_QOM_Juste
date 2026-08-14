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
        }

        /// <summary>Reapplies a tier and owned product list loaded from a save file.</summary>
        public void RestoreFromSave(EntitlementTier tier, IEnumerable<string> savedProductIds)
        {
            Tier = tier;
            ownedProductIds.Clear();
            foreach (var id in savedProductIds) ownedProductIds.Add(id);
            TierChanged?.Invoke(Tier);
        }

        public IReadOnlyCollection<string> OwnedProductIds => ownedProductIds;
    }
}
