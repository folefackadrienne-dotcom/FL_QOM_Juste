using KingdomOfGod.Audio;
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

        [Tooltip("Narrator intro line (docs/AudioDesign.md \"Narrateur principal\") — played once when the title screen appears. AudioManager.PlayVoiceLine already existed with nothing anywhere ever calling it.")]
        [SerializeField] private VoiceLineData narratorIntro;

        private void Start()
        {
            bool hasSave = GameManager.Instance != null && GameManager.Instance.Save.HasLocalSave();
            if (continueButton != null) continueButton.interactable = hasSave;

            GameManager.Instance?.Audio.PlayVoiceLine(narratorIntro);
        }

        public void OnNewGame()
        {
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            SceneManager.LoadScene(kingdomSceneName, LoadSceneMode.Single);
        }

        public void OnContinue()
        {
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");

            var data = GameManager.Instance?.Save.LoadLocal();
            if (data != null) GameManager.Instance?.SaveCoordinator.Apply(data);

            SceneManager.LoadScene(kingdomSceneName, LoadSceneMode.Single);
        }
    }
}
