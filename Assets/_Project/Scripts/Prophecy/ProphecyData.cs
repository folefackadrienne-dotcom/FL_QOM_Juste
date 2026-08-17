using KingdomOfGod.Core;
using UnityEngine;

namespace KingdomOfGod.Prophecy
{
    /// <summary>A biblical prophecy declared in one Age and its fulfillment, for the "Journal Prophétique".</summary>
    [CreateAssetMenu(fileName = "Prophecy_", menuName = "Kingdom of God/Prophecy", order = 45)]
    public class ProphecyData : ScriptableObject
    {
        public string displayName;
        public Age age;
        public string reference;
        [TextArea] public string prophecyText;
        [TextArea] public string fulfillmentText;
    }
}
