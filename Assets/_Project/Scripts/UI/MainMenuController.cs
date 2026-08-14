using KingdomOfGod.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>Wires the main menu's New Game / Continue buttons to scene transitions and save state.</summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private string kingdomSceneName = "Kingdom";

        private void Start()
        {
            bool hasSave = GameManager.Instance != null && GameManager.Instance.Save.HasLocalSave();
            if (continueButton != null) continueButton.interactable = hasSave;
        }

        public void OnNewGame() => SceneManager.LoadScene(kingdomSceneName, LoadSceneMode.Single);

        public void OnContinue()
        {
            GameManager.Instance?.Save.LoadLocal();
            SceneManager.LoadScene(kingdomSceneName, LoadSceneMode.Single);
        }
    }
}
