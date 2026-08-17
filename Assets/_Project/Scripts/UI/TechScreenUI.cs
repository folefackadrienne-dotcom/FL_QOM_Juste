using System.Collections.Generic;
using System.Text;
using KingdomOfGod.Core;
using KingdomOfGod.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>
    /// Technology tree screen: TechTree.TryUnlock was a real, callable method with nothing ever
    /// calling it, and no UI existed anywhere to reach the ~93 authored TechNode assets across the
    /// 3 trees (Économique/Militaire/Spirituel). Category tabs split the list into thirds — same
    /// non-scrolling tall-list convention as MissionListUI's 35 missions, no ScrollRect exists
    /// anywhere in this project. Selecting a node shows its branch/cost/prerequisites, and
    /// TryUnlock's result refreshes both the list (colored by state) and the detail view.
    /// AudioManager already reacts to TechTree.TechUnlocked with a category-specific chime
    /// ("Progression - Technologie Économique/Militaire/Spirituelle Débloquée") — this screen only
    /// needed to call TryUnlock.
    /// </summary>
    public class TechScreenUI : MonoBehaviour
    {
        [SerializeField] private TechTree techTree;
        [SerializeField] private UIThemeData theme;

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform listContainer;
        [SerializeField] private Button closeButton;

        [Header("Category tabs")]
        [SerializeField] private Button economicTabButton;
        [SerializeField] private Button militaryTabButton;
        [SerializeField] private Button spiritualTabButton;

        [Header("Detail view")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text categoryText;
        [SerializeField] private TMP_Text detailText;
        [SerializeField] private Button unlockButton;
        [SerializeField] private TMP_Text unlockButtonLabel;

        private TechTreeCategory currentCategory = TechTreeCategory.Economic;
        private TechNode selected;

        private void Awake()
        {
            // techTree lives on the persistent Bootstrap GameManager, in a different scene from
            // this screen — Inspector references can't cross scenes, so fall back to the running
            // singleton when this field was left unassigned.
            if (techTree == null && GameManager.Instance != null)
            {
                techTree = GameManager.Instance.Tech;
            }
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (unlockButton != null) unlockButton.onClick.AddListener(UnlockSelected);
            if (economicTabButton != null) economicTabButton.onClick.AddListener(SelectEconomic);
            if (militaryTabButton != null) militaryTabButton.onClick.AddListener(SelectMilitary);
            if (spiritualTabButton != null) spiritualTabButton.onClick.AddListener(SelectSpiritual);
            if (techTree != null) techTree.TechUnlocked += OnTechUnlocked;
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (unlockButton != null) unlockButton.onClick.RemoveListener(UnlockSelected);
            if (economicTabButton != null) economicTabButton.onClick.RemoveListener(SelectEconomic);
            if (militaryTabButton != null) militaryTabButton.onClick.RemoveListener(SelectMilitary);
            if (spiritualTabButton != null) spiritualTabButton.onClick.RemoveListener(SelectSpiritual);
            if (techTree != null) techTree.TechUnlocked -= OnTechUnlocked;
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

        private void SelectEconomic() => SelectCategory(TechTreeCategory.Economic);
        private void SelectMilitary() => SelectCategory(TechTreeCategory.Military);
        private void SelectSpiritual() => SelectCategory(TechTreeCategory.Spiritual);

        private void SelectCategory(TechTreeCategory category)
        {
            currentCategory = category;
            selected = null;
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            RefreshList();
        }

        private void OnTechUnlocked(TechNode node) => RefreshList();

        private void RefreshList()
        {
            if (listContainer == null || techTree == null) return;

            for (int i = listContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(listContainer.GetChild(i).gameObject);
            }

            TechNode firstEntry = null;
            foreach (var node in techTree.AllNodes)
            {
                if (node == null || node.category != currentCategory) continue;
                if (firstEntry == null) firstEntry = node;
                CreateNodeButton(node);
            }

            if (selected == null && firstEntry != null)
            {
                selected = firstEntry;
                UpdateDetail(firstEntry);
            }
            else if (selected != null)
            {
                UpdateDetail(selected);
            }
        }

        private void CreateNodeButton(TechNode node)
        {
            bool unlocked = techTree.IsUnlocked(node);
            bool canUnlock = techTree.CanUnlock(node);

            var go = new GameObject(node.displayName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(listContainer, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240f, 40f);

            var image = go.GetComponent<Image>();
            image.color = unlocked
                ? (theme != null ? theme.divineLight : new Color(0.957f, 0.769f, 0.188f))
                : canUnlock
                    ? (theme != null ? theme.deepBlue : new Color(0.15f, 0.1f, 0.05f, 0.9f))
                    : new Color(0.3f, 0.3f, 0.3f, 0.6f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => SelectNode(node));

            var labelGO = new GameObject("Text", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = node.displayName;
            label.fontSize = 13f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = theme != null ? theme.ivoryWhite : Color.white;

            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
        }

        private void SelectNode(TechNode node)
        {
            selected = node;
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            UpdateDetail(node);
        }

        private void UpdateDetail(TechNode node)
        {
            bool unlocked = techTree.IsUnlocked(node);
            bool canUnlock = techTree.CanUnlock(node);

            if (nameText != null) nameText.text = node.displayName;
            if (categoryText != null) categoryText.text = $"{node.category} — {node.branch}";

            if (detailText != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine(node.effectDescription);

                if (node.cost.Count > 0)
                {
                    sb.Append("Coût : ");
                    for (int i = 0; i < node.cost.Count; i++)
                    {
                        if (i > 0) sb.Append(", ");
                        sb.Append($"{node.cost[i].amount} {node.cost[i].type}");
                    }
                    sb.AppendLine();
                }

                if (node.prerequisiteIds.Count > 0)
                {
                    var names = new List<string>();
                    foreach (var id in node.prerequisiteIds)
                    {
                        var prereq = FindNode(id);
                        names.Add(prereq != null ? prereq.displayName : id);
                    }
                    sb.AppendLine("Prérequis : " + string.Join(", ", names));
                }

                if (!string.IsNullOrEmpty(node.additionalRequirement))
                {
                    sb.Append(node.additionalRequirement);
                }

                detailText.text = sb.ToString();
            }

            if (unlockButton != null)
            {
                unlockButton.gameObject.SetActive(!unlocked);
                unlockButton.interactable = canUnlock;
            }
            if (unlockButtonLabel != null) unlockButtonLabel.text = unlocked ? "Débloqué" : "Débloquer";
        }

        private TechNode FindNode(string techId)
        {
            foreach (var node in techTree.AllNodes)
            {
                if (node != null && node.techId == techId) return node;
            }
            return null;
        }

        private void UnlockSelected()
        {
            if (selected == null || techTree == null) return;

            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            if (!techTree.TryUnlock(selected))
            {
                GameManager.Instance?.Audio.PlaySfx("Interface - Erreur / Action Impossible");
            }
        }
    }
}
