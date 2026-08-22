using System.Collections.Generic;
using KingdomOfGod.Core;
using KingdomOfGod.Quiz;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>
    /// "Question du Jour" panel: shows QuizManager.CurrentQuestion (if any) with up to 4 answer
    /// buttons, or a short "no question available yet" message otherwise — the player opens this
    /// whenever they like, nothing is ever forced onto them. Answer buttons are pre-created up to
    /// answerButtons.Count and hidden past the current question's answers.Count, since questions can
    /// have 3 or 4 choices.
    /// </summary>
    public class QuizUI : MonoBehaviour
    {
        [SerializeField] private QuizManager quizManager;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private GameObject questionGroup;
        [SerializeField] private TMP_Text questionText;
        [SerializeField] private List<Button> answerButtons = new List<Button>();
        [SerializeField] private List<TextMeshProUGUI> answerLabels = new List<TextMeshProUGUI>();
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private TMP_Text noQuestionText;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            // quizManager lives on the persistent Bootstrap GameManager, in a different scene from
            // this panel — Inspector references can't cross scenes, so fall back to the running
            // singleton when this field was left unassigned.
            if (quizManager == null && GameManager.Instance != null)
            {
                quizManager = GameManager.Instance.Quiz;
            }
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            for (int i = 0; i < answerButtons.Count; i++)
            {
                int index = i;
                if (answerButtons[i] != null) answerButtons[i].onClick.AddListener(() => OnAnswerClicked(index));
            }
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            for (int i = 0; i < answerButtons.Count; i++)
            {
                int index = i;
                if (answerButtons[i] != null) answerButtons[i].onClick.RemoveListener(() => OnAnswerClicked(index));
            }
        }

        public void Open()
        {
            panelRoot.SetActive(true);
            Refresh();
            GameManager.Instance?.Audio.PlaySfx("Interface - Ouverture de Menu");
        }

        public void Close()
        {
            panelRoot.SetActive(false);
            GameManager.Instance?.Audio.PlaySfx("Interface - Fermeture de Menu");
        }

        private void Refresh()
        {
            bool has = quizManager != null && quizManager.HasAvailableQuestion;
            if (noQuestionText != null) noQuestionText.gameObject.SetActive(!has);
            if (questionGroup != null) questionGroup.SetActive(has);
            if (!has) return;

            var question = quizManager.CurrentQuestion;
            if (questionText != null) questionText.text = question.questionText;
            if (resultText != null) resultText.text = "";

            for (int i = 0; i < answerButtons.Count; i++)
            {
                bool inRange = i < question.answers.Count;
                if (answerButtons[i] != null)
                {
                    answerButtons[i].gameObject.SetActive(inRange);
                    answerButtons[i].interactable = true;
                }
                if (inRange && i < answerLabels.Count && answerLabels[i] != null)
                {
                    answerLabels[i].text = question.answers[i];
                }
            }
        }

        private void OnAnswerClicked(int index)
        {
            if (quizManager == null || !quizManager.HasAvailableQuestion) return;

            var question = quizManager.CurrentQuestion;
            bool correct = index == question.correctAnswerIndex;
            string explanation = question.explanation;

            quizManager.SubmitAnswer(index);

            if (resultText != null)
            {
                resultText.text = (correct
                    ? $"Bonne réponse ! +{question.faithReward:0} Foi\n\n"
                    : "Pas tout à fait.\n\n") + explanation;
            }

            foreach (var button in answerButtons)
            {
                if (button != null) button.interactable = false;
            }
        }
    }
}
