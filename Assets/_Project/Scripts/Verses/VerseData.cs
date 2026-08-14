using System.Collections.Generic;
using KingdomOfGod.Core;
using KingdomOfGod.Resources;
using UnityEngine;

namespace KingdomOfGod.Verses
{
    /// <summary>A memorizable scripture reference, its text, and the permanent bonus earned for memorizing it.</summary>
    [CreateAssetMenu(fileName = "Verse_", menuName = "Kingdom of God/Verse", order = 40)]
    public class VerseData : ScriptableObject
    {
        public string reference; // e.g. "Genèse 15:6"
        [TextArea] public string text;
        public Age age;
        [TextArea] public string contextComment;

        [Header("Reward")]
        public List<ResourceAmount> permanentBonus = new List<ResourceAmount>();
    }
}
