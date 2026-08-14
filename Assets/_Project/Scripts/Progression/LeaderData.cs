using System.Collections.Generic;
using KingdomOfGod.Core;
using UnityEngine;

namespace KingdomOfGod.Progression
{
    /// <summary>A legendary leader (David, Salomon, Josué, Débora...) unlockable with unique talents.</summary>
    [CreateAssetMenu(fileName = "Leader_", menuName = "Kingdom of God/Leader", order = 70)]
    public class LeaderData : ScriptableObject
    {
        public string displayName;
        public Age age;
        public Sprite portrait;
        [TextArea] public string biography;
        public List<string> uniqueTalents = new List<string>();
        [TextArea] public string unlockCondition;
    }
}
