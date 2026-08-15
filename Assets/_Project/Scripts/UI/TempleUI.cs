using KingdomOfGod.Buildings;
using KingdomOfGod.Core;
using KingdomOfGod.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>
    /// Small persistent Temple widget in the Kingdom HUD (never closed, unlike the other panels):
    /// current level plus an "Améliorer" button that becomes interactable once
    /// TempleSystem.CanUpgrade() is true. TempleSystem.levels was an empty list before this round
    /// — TryUpgrade/CanUpgrade were real methods that could never do anything regardless of a UI
    /// calling them, since GetLevelData(CurrentLevel + 1) always returned null. Refreshes on
    /// ResourceManager.ResourceChanged and TempleSystem.LevelUpgraded so affordability tracks
    /// live. Doesn't play a success SFX itself on upgrade — AudioManager already reacts to
    /// TempleSystem.LevelUpgraded with "Bâtiments - Temple Amélioré", same as building placement.
    /// </summary>
    public class TempleUI : MonoBehaviour
    {
        [SerializeField] private TempleSystem templeSystem;
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Button upgradeButton;

        private void Awake()
        {
            // templeSystem/resourceManager live on the persistent Bootstrap GameManager, in a
            // different scene from this widget — Inspector references can't cross scenes, so
            // fall back to the running singleton when these fields were left unassigned.
            if (templeSystem == null && GameManager.Instance != null)
            {
                templeSystem = GameManager.Instance.Temple;
            }
            if (resourceManager == null && GameManager.Instance != null)
            {
                resourceManager = GameManager.Instance.Resources;
            }
        }

        private void OnEnable()
        {
            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);
            if (resourceManager != null) resourceManager.ResourceChanged += OnResourceChanged;
            if (templeSystem != null) templeSystem.LevelUpgraded += OnLevelUpgraded;
            Refresh();
        }

        private void OnDisable()
        {
            if (upgradeButton != null) upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
            if (resourceManager != null) resourceManager.ResourceChanged -= OnResourceChanged;
            if (templeSystem != null) templeSystem.LevelUpgraded -= OnLevelUpgraded;
        }

        private void OnResourceChanged(ResourceType type, float value) => Refresh();

        private void OnLevelUpgraded(int level) => Refresh();

        private void OnUpgradeClicked()
        {
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            if (!templeSystem.TryUpgrade())
            {
                GameManager.Instance?.Audio.PlaySfx("Interface - Erreur / Action Impossible");
            }
        }

        private void Refresh()
        {
            if (templeSystem == null) return;

            if (levelText != null) levelText.text = $"Temple — Niveau {templeSystem.CurrentLevel}";
            if (upgradeButton != null) upgradeButton.interactable = templeSystem.CanUpgrade();
        }
    }
}
