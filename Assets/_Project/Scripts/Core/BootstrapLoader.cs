using UnityEngine;
using UnityEngine.SceneManagement;

namespace KingdomOfGod.Core
{
    /// <summary>Lives in the Bootstrap scene next to GameManager; once the persistent managers exist, hands off to the main menu.</summary>
    public class BootstrapLoader : MonoBehaviour
    {
        [SerializeField] private string nextSceneName = "MainMenu";

        private void Start()
        {
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }
}
