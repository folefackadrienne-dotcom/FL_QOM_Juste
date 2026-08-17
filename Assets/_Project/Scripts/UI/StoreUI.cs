using KingdomOfGod.Core;
using KingdomOfGod.Monetization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>
    /// Boutique panel: EntitlementManager.PurchaseFullEdition/RestorePurchases were real, callable
    /// methods with zero callers anywhere — no Store screen existed at all, so a free-tier player
    /// could never cross EntitlementManager.freeAgeLimit (Age 2). AgeManager.AdvanceToNextAge would
    /// then silently fail its content-gate check forever, and the campaign could never reach
    /// CampaignCompleted. AudioManager already reacts to ProductPurchased/TierChanged with SFX
    /// (Monétisation - Achat Réussi / Édition Complète Débloquée); this panel only needed to call
    /// the two methods.
    /// </summary>
    public class StoreUI : MonoBehaviour
    {
        [SerializeField] private EntitlementManager entitlementManager;
        [SerializeField] private UIThemeData theme;

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text tierText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TMP_Text purchaseButtonLabel;
        [SerializeField] private Button restoreButton;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            // entitlementManager lives on the persistent Bootstrap GameManager, in a different
            // scene from this panel — Inspector references can't cross scenes, so fall back to
            // the running singleton when this field was left unassigned.
            if (entitlementManager == null && GameManager.Instance != null)
            {
                entitlementManager = GameManager.Instance.Entitlements;
            }
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (purchaseButton != null) purchaseButton.onClick.AddListener(OnPurchaseClicked);
            if (restoreButton != null) restoreButton.onClick.AddListener(OnRestoreClicked);
            if (entitlementManager != null) entitlementManager.TierChanged += OnTierChanged;

            Refresh();
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (purchaseButton != null) purchaseButton.onClick.RemoveListener(OnPurchaseClicked);
            if (restoreButton != null) restoreButton.onClick.RemoveListener(OnRestoreClicked);
            if (entitlementManager != null) entitlementManager.TierChanged -= OnTierChanged;
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

        private void OnTierChanged(EntitlementTier tier) => Refresh();

        private void OnPurchaseClicked()
        {
            if (entitlementManager == null) return;

            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            if (purchaseButton != null) purchaseButton.interactable = false;

            entitlementManager.PurchaseFullEdition(success =>
            {
                if (!success) GameManager.Instance?.Audio.PlaySfx("Interface - Erreur / Action Impossible");
                Refresh();
            });
        }

        private void OnRestoreClicked()
        {
            if (entitlementManager == null) return;

            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");

            entitlementManager.RestorePurchases(success =>
            {
                if (!success) GameManager.Instance?.Audio.PlaySfx("Interface - Erreur / Action Impossible");
                Refresh();
            });
        }

        private void Refresh()
        {
            if (entitlementManager == null) return;

            bool isFullEdition = entitlementManager.IsFullEdition;

            if (tierText != null)
            {
                tierText.text = isFullEdition
                    ? "Édition Complète"
                    : "Édition Gratuite";
            }
            if (descriptionText != null)
            {
                descriptionText.text = isFullEdition
                    ? "Tous les Âges et le Mode Libre sont débloqués. Merci de soutenir le royaume !"
                    : "Accès gratuit aux deux premiers Âges. L'Édition Complète débloque tous les "
                    + "Âges jusqu'au retour d'exil, ainsi que le Mode Libre — aucun avantage de "
                    + "puissance en jeu, uniquement du contenu.";
            }
            if (purchaseButton != null) purchaseButton.interactable = !isFullEdition;
            if (purchaseButtonLabel != null)
            {
                purchaseButtonLabel.text = isFullEdition ? "Édition Complète Débloquée" : "Acheter l'Édition Complète";
            }
        }
    }
}
