using KingdomOfGod.Alliance;
using KingdomOfGod.Core;
using KingdomOfGod.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>
    /// Small persistent Alliance widget in the Kingdom HUD (never closed, like TempleWidget):
    /// current Alliance value/standing plus a "Se Repentir" button. AllianceSystem.TryRepent's own
    /// doc comment calls the mechanic "always available", but it had zero callers anywhere — a
    /// player whose Alliance fell Low (weaker miracles) had no deliberate way to recover it, only
    /// passive gains from mission rewards. Doesn't play a success SFX itself — AudioManager already
    /// reacts to AllianceSystem.Repented with "Alliance - Repentance / Restauration".
    /// </summary>
    public class RepentanceUI : MonoBehaviour
    {
        [SerializeField] private AllianceSystem allianceSystem;
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button repentButton;

        private void Awake()
        {
            // allianceSystem/resourceManager live on the persistent Bootstrap GameManager, in a
            // different scene from this widget — Inspector references can't cross scenes, so
            // fall back to the running singleton when these fields were left unassigned.
            if (allianceSystem == null && GameManager.Instance != null)
            {
                allianceSystem = GameManager.Instance.Alliance;
            }
            if (resourceManager == null && GameManager.Instance != null)
            {
                resourceManager = GameManager.Instance.Resources;
            }
        }

        private void OnEnable()
        {
            if (repentButton != null) repentButton.onClick.AddListener(OnRepentClicked);
            if (allianceSystem != null) allianceSystem.ValueChanged += OnAllianceValueChanged;
            if (resourceManager != null) resourceManager.ResourceChanged += OnResourceChanged;
            Refresh();
        }

        private void OnDisable()
        {
            if (repentButton != null) repentButton.onClick.RemoveListener(OnRepentClicked);
            if (allianceSystem != null) allianceSystem.ValueChanged -= OnAllianceValueChanged;
            if (resourceManager != null) resourceManager.ResourceChanged -= OnResourceChanged;
        }

        private void OnAllianceValueChanged(float value) => Refresh();

        private void OnResourceChanged(ResourceType type, float value) => Refresh();

        private void OnRepentClicked()
        {
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            if (!allianceSystem.TryRepent())
            {
                GameManager.Instance?.Audio.PlaySfx("Interface - Erreur / Action Impossible");
            }
        }

        private void Refresh()
        {
            if (allianceSystem == null) return;

            if (statusText != null) statusText.text = $"Alliance — {(int)allianceSystem.Value} ({StandingLabel(allianceSystem.Standing)})";
            if (repentButton != null) repentButton.interactable = allianceSystem.CanRepent();
        }

        private static string StandingLabel(AllianceStanding standing) => standing switch
        {
            AllianceStanding.Low => "Basse",
            AllianceStanding.High => "Haute",
            _ => "Moyenne"
        };
    }
}
