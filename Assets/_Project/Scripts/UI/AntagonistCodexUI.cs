using System.Collections.Generic;
using KingdomOfGod.Battle;
using KingdomOfGod.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>
    /// Read-only codex of major boss antagonists (Pharaon, Goliath, Jézabel...) — AntagonistData had
    /// no manager and no UI at all: BattleManager only ever reads it to pick a boss-specific SFX cue
    /// (Assets/_Project/Scripts/Battle/BattleManager.cs), the narrative content itself was never
    /// shown anywhere. No unlock/collection state to track here (unlike Leaders/Artifacts) — an
    /// antagonist is revealed the moment its Age unlocks, same as everything else Age-gated, so this
    /// stays a plain list + detail view with no backing manager.
    /// </summary>
    public class AntagonistCodexUI : MonoBehaviour
    {
        [SerializeField] private AgeManager ageManager;
        [SerializeField] private UIThemeData theme;

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform listContainer;
        [SerializeField] private Button closeButton;

        [SerializeField] private List<AntagonistData> allAntagonists = new List<AntagonistData>();

        [Header("Detail view")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text roleText;
        [SerializeField] private TMP_Text detailText;

        private AntagonistData selected;

        private void Awake()
        {
            // ageManager lives on the persistent Bootstrap GameManager, in a different scene from
            // this codex — Inspector references can't cross scenes, so fall back to the running
            // singleton when this field was left unassigned.
            if (ageManager == null && GameManager.Instance != null)
            {
                ageManager = GameManager.Instance.Ages;
            }
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
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

        private void RefreshList()
        {
            if (listContainer == null) return;

            for (int i = listContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(listContainer.GetChild(i).gameObject);
            }

            AntagonistData firstEntry = null;
            foreach (var antagonist in allAntagonists)
            {
                if (antagonist == null) continue;
                if (ageManager != null && !ageManager.IsUnlocked(antagonist.age)) continue;

                if (firstEntry == null) firstEntry = antagonist;
                CreateAntagonistButton(antagonist);
            }

            if ((selected == null || (ageManager != null && !ageManager.IsUnlocked(selected.age))) && firstEntry != null)
            {
                SelectAntagonist(firstEntry);
            }
        }

        private void CreateAntagonistButton(AntagonistData antagonist)
        {
            var go = new GameObject(antagonist.displayName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(listContainer, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(240f, 44f);

            var image = go.GetComponent<Image>();
            image.color = theme != null ? theme.deepBlue : new Color(0.15f, 0.1f, 0.05f, 0.9f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => SelectAntagonist(antagonist));

            var labelGO = new GameObject("Text", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = antagonist.displayName;
            label.fontSize = 16f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = theme != null ? theme.ivoryWhite : Color.white;

            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
        }

        private void SelectAntagonist(AntagonistData antagonist)
        {
            selected = antagonist;
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");

            if (nameText != null) nameText.text = antagonist.displayName;
            if (roleText != null) roleText.text = antagonist.role;
            if (detailText != null)
            {
                detailText.text = $"{antagonist.encounterDescription}\n\n"
                    + $"{antagonist.uniqueMechanicName} : {antagonist.uniqueMechanicDescription}\n\n"
                    + $"Condition de victoire : {antagonist.victoryCondition}";
            }

            if (portraitImage != null)
            {
                portraitImage.sprite = antagonist.portrait;
                portraitImage.color = antagonist.portrait != null
                    ? Color.white
                    : (theme != null ? theme.purpleRed : new Color(0.35f, 0.1f, 0.1f));
            }
        }
    }
}
