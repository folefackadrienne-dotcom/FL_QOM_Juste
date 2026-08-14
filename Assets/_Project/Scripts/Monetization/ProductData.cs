using UnityEngine;

namespace KingdomOfGod.Monetization
{
    /// <summary>
    /// One entry of the store catalog. GDD monetization rule: only FullEdition unlocks
    /// gameplay content; every other type is cosmetic/confort only (no pay-to-win).
    /// </summary>
    public enum ProductType
    {
        FullEdition,
        CosmeticPack,
        BattlePassSeason,
        AudioVersePack
    }

    [CreateAssetMenu(fileName = "Product_", menuName = "Kingdom of God/Monetization/Product", order = 90)]
    public class ProductData : ScriptableObject
    {
        [Tooltip("Must match the SKU configured in the store (App Store / Play Store / Unity IAP).")]
        public string storeProductId;
        public string displayName;
        [TextArea] public string description;
        public ProductType type;
        public Sprite icon;
    }
}
