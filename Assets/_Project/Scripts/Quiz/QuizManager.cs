using System;
using System.Collections.Generic;
using System.Linq;
using KingdomOfGod.Core;
using KingdomOfGod.Resources;
using UnityEngine;

namespace KingdomOfGod.Quiz
{
    /// <summary>
    /// Every 5 turns (turnsBetweenQuestions), rolls one unanswered question from the current Age's
    /// pool and holds it until the player submits an answer through the HUD's quiz panel — non-
    /// intrusive by design, the same "player opens it when ready" posture as PrayerMenuUI/
    /// VerseJournalUI, never a forced pop-up. A wrong answer costs nothing and leaves the question
    /// back in the pool for a future roll; only a correct answer marks it answered (so re-showing
    /// a missed question later can't be farmed for Foi twice) and grants its Foi/points reward.
    /// Answering every question of an Age awards that Age's one-off Knowledge Badge bonus, the same
    /// "Age set completion" shape as CollectionManager.AgeCollectionCompleted for artifacts.
    /// </summary>
    public class QuizManager : MonoBehaviour
    {
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private AgeManager ageManager;
        [SerializeField] private KingdomTurnManager turnManager;
        [SerializeField] private List<QuizQuestionData> allQuestions = new List<QuizQuestionData>();

        [SerializeField] private int turnsBetweenQuestions = 5;
        [SerializeField] private float ageCompletionBonusFaith = 15f;

        private readonly HashSet<QuizQuestionData> answeredCorrectly = new HashSet<QuizQuestionData>();
        private readonly HashSet<Age> badgesEarned = new HashSet<Age>();
        private readonly System.Random rng = new System.Random();

        public int Score { get; private set; }
        public QuizQuestionData CurrentQuestion { get; private set; }
        public bool HasAvailableQuestion => CurrentQuestion != null;
        public IReadOnlyCollection<QuizQuestionData> AnsweredCorrectly => answeredCorrectly;
        public IReadOnlyCollection<Age> BadgesEarned => badgesEarned;

        public event Action<QuizQuestionData> QuestionAvailable;
        public event Action<QuizQuestionData, bool> QuestionAnswered;
        public event Action<Age> BadgeEarned;

        private void OnEnable()
        {
            if (turnManager != null) turnManager.TurnAdvanced += OnTurnAdvanced;
        }

        private void OnDisable()
        {
            if (turnManager != null) turnManager.TurnAdvanced -= OnTurnAdvanced;
        }

        private void OnTurnAdvanced(int turn)
        {
            if (turn % turnsBetweenQuestions != 0) return;
            RollNextQuestion();
        }

        private void RollNextQuestion()
        {
            if (CurrentQuestion != null || ageManager == null) return;

            var pool = allQuestions
                .Where(q => q != null && q.age == ageManager.CurrentAge && !answeredCorrectly.Contains(q))
                .ToList();
            if (pool.Count == 0) return;

            CurrentQuestion = pool[rng.Next(pool.Count)];
            QuestionAvailable?.Invoke(CurrentQuestion);
        }

        /// <summary>Submits an answer to the currently pending question. Correct answers pay out once and can't be re-earned; wrong answers leave the question available for a later roll.</summary>
        public void SubmitAnswer(int choiceIndex)
        {
            if (CurrentQuestion == null) return;

            var question = CurrentQuestion;
            bool correct = choiceIndex == question.correctAnswerIndex;

            if (correct && answeredCorrectly.Add(question))
            {
                resourceManager.Add(ResourceType.Faith, question.faithReward);
                Score += question.pointsReward;
                CheckAgeCompletion(question.age);
            }

            QuestionAnswered?.Invoke(question, correct);
            CurrentQuestion = null;
        }

        private void CheckAgeCompletion(Age age)
        {
            if (badgesEarned.Contains(age)) return;

            var ageQuestions = allQuestions.Where(q => q != null && q.age == age).ToList();
            if (ageQuestions.Count == 0 || !ageQuestions.All(answeredCorrectly.Contains)) return;

            badgesEarned.Add(age);
            resourceManager.Add(ResourceType.Faith, ageCompletionBonusFaith);
            BadgeEarned?.Invoke(age);
        }

        /// <summary>Reapplies quiz progress loaded from a save file, matched by QuizQuestionData.questionId against allQuestions — bypasses SubmitAnswer's reward grant since the save's resource stock and score already reflect it.</summary>
        public void RestoreFromSave(IEnumerable<string> savedAnsweredIds, int savedScore, IEnumerable<int> savedBadgeAges)
        {
            var idSet = new HashSet<string>(savedAnsweredIds);
            foreach (var question in allQuestions)
            {
                if (question != null && idSet.Contains(question.questionId)) answeredCorrectly.Add(question);
            }

            Score = savedScore;

            foreach (var age in savedBadgeAges) badgesEarned.Add((Age)age);
        }
    }
}
