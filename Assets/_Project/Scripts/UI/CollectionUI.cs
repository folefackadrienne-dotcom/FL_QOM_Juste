using KingdomOfGod.Collectibles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>
    /// "Fiche" journal for the Collectibles system (docs/GDD.md "Collectibles": "Chaque objet a une
    /// fiche : texte biblique exact, contexte historique, commentaire éducatif, effet de jeu,
    /// illustration") — CollectionManager.Collect was a real, callable method with nothing ever
    /// calling it and no UI ever reading CollectionManager.Owned; both gaps are fixed together
    /// (Collect's caller lives on CollectionManager itself now, age-triggered). A locked entry shows
    /// its rarity/age instead of its fiche, matching the reveal-by-age discipline used for buildings/
    /// missions/tech elsewhere.
    /// </summary>
    public class CollectionUI : MonoBehaviour
    {
        [SerializeField] private CollectionManager collectionManager;
        [SerializeField] private UIThemeData theme;

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform listContainer;
        [SerializeField] private Button closeButton;

        [Header("Detail view")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text rarityText;
        [SerializeField] private TMP_Text detailText;

        private ArtifactData selected;

        private void Awake()
        {
            // collectionManager lives on the persistent Bootstrap GameManager, in a different scene
            // from this journal — Inspector references can't cross scenes, so fall back to the
            // running singleton when this field was left unassigned.
            if (collectionManager == null && GameManager.Instance != null)
            {
                collectionManager = GameManager.Instance.Collection;
            }
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (collectionManager != null) collectionManager.ArtifactCollected += OnArtifactCollected;
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (collectionManager != null) collectionManager.ArtifactCollected -= OnArtifactCollected;
        }

        public void Open()
        {
            panelRoot.SetActive(true);
            RefreshList();
            GameManager.Instance?.Audio.PlaySfx("Interface - Ouverture de Menu");
        }

        public void Close()
        {
            panelRoot.SetActive(false);
            GameManager.Instance?.Audio.PlaySfx("Interface - Fermeture de Menu");
        }

        private void OnArtifactCollected(ArtifactData artifact) => RefreshList();

        private void RefreshList()
        {
            if (listContainer == null || collectionManager == null) return;

            for (int i = listContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(listContainer.GetChild(i).gameObject);
            }

            ArtifactData firstEntry = null;
            foreach (var artifact in collectionManager.AllArtifacts)
            {
                if (artifact == null) continue;
                if (firstEntry == null) firstEntry = artifact;
                CreateArtifactButton(artifact);
            }

            if ((selected == null || !collectionManager.IsOwned(selected)) && firstEntry != null)
            {
                SelectArtifact(firstEntry);
            }
        }

        private void CreateArtifactButton(ArtifactData artifact)
        {
            bool owned = collectionManager.IsOwned(artifact);

            var go = new GameObject(artifact.displayName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(listContainer, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240f, 44f);

            var image = go.GetComponent<Image>();
            image.color = owned
                ? RarityColor(artifact.rarity)
                : new Color(0.3f, 0.3f, 0.3f, 0.6f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => SelectArtifact(artifact));

            var labelGO = new GameObject("Text", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = owned ? artifact.displayName : "??? (Non découvert)";
            label.fontSize = 16f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = theme != null ? theme.ivoryWhite : Color.white;

            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
        }

        /// <summary>Common→Rare→Epic→Legendary mapped onto the existing UITheme palette instead of inventing new colors — ivory/blue/gold/purple-red, ascending rarity.</summary>
        private Color RarityColor(Rarity rarity)
        {
            if (theme == null) return new Color(0.15f, 0.1f, 0.05f, 0.9f);

            return rarity switch
            {
                Rarity.Rare => theme.deepBlue,
                Rarity.Epic => theme.warmGold,
                Rarity.Legendary => theme.purpleRed,
                _ => new Color(0.15f, 0.1f, 0.05f, 0.9f)
            };
        }

        private void SelectArtifact(ArtifactData artifact)
        {
            selected = artifact;
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");

            bool owned = collectionManager != null && collectionManager.IsOwned(artifact);

            if (nameText != null) nameText.text = owned ? artifact.displayName : "??? (Non découvert)";
            if (rarityText != null) rarityText.text = owned ? artifact.rarity.ToString() : $"{artifact.rarity} — Âge {(int)artifact.age + 1}";
            if (detailText != null)
            {
                detailText.text = owned
                    ? $"{artifact.biblicalReference}\n\n{artifact.historicalContext}\n\n{artifact.educationalComment}\n\n{artifact.activeAbilityDescription}"
                    : string.Empty;
            }

            if (iconImage != null)
            {
                iconImage.sprite = owned ? artifact.icon : null;
                iconImage.color = owned && artifact.icon != null ? Color.white : RarityColor(artifact.rarity);
            }
        }
    }
}
