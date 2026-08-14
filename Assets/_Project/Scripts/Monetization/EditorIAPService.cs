using System;
using UnityEngine;

namespace KingdomOfGod.Monetization
{
    /// <summary>Editor/testing stand-in: every "purchase" succeeds immediately, no store involved.</summary>
    public class EditorIAPService : IIAPService
    {
        public void Initialize() { }

        public void Purchase(ProductData product, Action<bool> onComplete)
        {
            Debug.Log($"[EditorIAPService] Simulating purchase of '{product.storeProductId}'.");
            onComplete?.Invoke(true);
        }

        public void RestorePurchases(Action<bool> onComplete)
        {
            Debug.Log("[EditorIAPService] Simulating restore purchases (no-op).");
            onComplete?.Invoke(true);
        }
    }
}
