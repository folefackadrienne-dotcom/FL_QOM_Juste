using System.Collections.Generic;
using KingdomOfGod.Core;
using UnityEngine;

namespace KingdomOfGod.Quiz
{
    /// <summary>A biblical quiz question tied to one Age, its 3-4 answer choices, and the reward for answering it correctly.</summary>
    [CreateAssetMenu(fileName = "Quiz_", menuName = "Kingdom of God/Quiz Question", order = 95)]
    public class QuizQuestionData : ScriptableObject
    {
        [Tooltip("Stable id used for save matching — independent of the asset's file name.")]
        public string questionId;
        public Age age;
        [TextArea] public string questionText;
        public List<string> answers = new List<string>();
        public int correctAnswerIndex;
        public string sourceReference; // e.g. "Genèse 15:6"
        [TextArea] public string explanation;

        [Header("Reward")]
        public float faithReward;
        public int pointsReward;
    }
}
