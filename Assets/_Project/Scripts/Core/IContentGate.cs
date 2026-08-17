using System;

namespace KingdomOfGod.Core
{
    /// <summary>
    /// Optional check consulted before an age is unlocked. Lets a monetization layer
    /// restrict content (e.g. free tier = first 2-3 ages) without AgeManager knowing
    /// anything about purchases, stores, or entitlements.
    /// </summary>
    public interface IContentGate
    {
        bool CanUnlockAge(Age age);

        /// <summary>Fires whenever CanUnlockAge's answer for the current age might have flipped from false to true (e.g. a purchase completed) — lets AgeManager retry an unlock that was previously refused, without knowing why it changed.</summary>
        event Action GateChanged;
    }
}
