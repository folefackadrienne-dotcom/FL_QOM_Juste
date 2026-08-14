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
    }
}
