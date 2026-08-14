using System;

namespace KingdomOfGod.Monetization
{
    /// <summary>
    /// Thin seam over the store SDK. Swap <see cref="EditorIAPService"/> for a real
    /// implementation (e.g. backed by Unity IAP / com.unity.purchasing) once the store
    /// is configured; EntitlementManager only talks to this interface.
    /// </summary>
    public interface IIAPService
    {
        void Initialize();
        void Purchase(ProductData product, Action<bool> onComplete);
        void RestorePurchases(Action<bool> onComplete);
    }
}
