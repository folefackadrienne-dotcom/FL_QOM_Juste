using KingdomOfGod.Alliance;
using KingdomOfGod.Core;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>
    /// "Le monde répond visuellement à l'Alliance : plus lumineux quand le joueur est fidèle,
    /// plus terne et hostile quand il s'en éloigne" (docs/ArtDirection.md section 7) — tints a
    /// full-screen overlay by AllianceStanding, the same event AudioManager already reacts to
    /// for music/SFX (Alliance - Entrée en Crise / Faveur Élevée).
    /// </summary>
    public class WorldMoodUI : MonoBehaviour
    {
        [SerializeField] private AllianceSystem allianceSystem;
        [SerializeField] private UIThemeData theme;
        [SerializeField] private Image moodOverlay;

        private void OnEnable()
        {
            // allianceSystem lives on the persistent Bootstrap GameManager, in a different scene
            // from this HUD — Inspector references can't cross scenes, so fall back to the
            // running singleton when this field was left unassigned.
            if (allianceSystem == null && GameManager.Instance != null)
            {
                allianceSystem = GameManager.Instance.Alliance;
            }

            if (allianceSystem == null || moodOverlay == null || theme == null) return;

            allianceSystem.StandingChanged += OnStandingChanged;
            OnStandingChanged(allianceSystem.Standing);
        }

        private void OnDisable()
        {
            if (allianceSystem != null) allianceSystem.StandingChanged -= OnStandingChanged;
        }

        private void OnStandingChanged(AllianceStanding standing)
        {
            moodOverlay.color = standing switch
            {
                AllianceStanding.High => WithAlpha(theme.divineLight, 0.12f),
                AllianceStanding.Low => WithAlpha(theme.crisisRed, 0.20f),
                _ => WithAlpha(theme.ochre, 0f)
            };
        }

        private static Color WithAlpha(Color color, float alpha) => new Color(color.r, color.g, color.b, alpha);
    }
}
