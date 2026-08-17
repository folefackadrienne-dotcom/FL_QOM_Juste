using KingdomOfGod.Battle;
using KingdomOfGod.Core;
using KingdomOfGod.Interaction;
using KingdomOfGod.Miracles;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>
    /// Top-level Battle HUD: shows the currently-selected unit's stats (BattleInputController.
    /// SelectionChanged), an End Turn button (BattleManager.EndPlayerPhase), a runtime-generated
    /// list of castable-miracle buttons (BattleManager.TryCastMiracle — no list-item prefab or
    /// art needed, these are flat UITheme-colored buttons built in code, same as
    /// ProjectSceneSetup.CreateButton but at runtime since the castable set isn't known until
    /// play), and a Victory/Defeat outcome panel (BattleManager.BattleEnded, previously fired
    /// into an empty void).
    /// </summary>
    public class BattleHUDController : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private BattleInputController inputController;
        [SerializeField] private MiracleManager miracleManager;
        [SerializeField] private UIThemeData theme;

        [SerializeField] private TMP_Text unitInfoText;
        [SerializeField] private Button endTurnButton;
        [SerializeField] private Transform miracleListContainer;
        [SerializeField] private GameObject outcomePanel;
        [SerializeField] private TMP_Text outcomeText;
        [SerializeField] private Button outcomeReturnButton;

        private void Awake()
        {
            // miracleManager lives on the persistent Bootstrap GameManager, in a different scene
            // from this HUD — Inspector references can't cross scenes, so fall back to the
            // running singleton when this field was left unassigned.
            if (miracleManager == null && GameManager.Instance != null)
            {
                miracleManager = GameManager.Instance.Miracles;
            }
        }

        private void OnEnable()
        {
            if (inputController != null) inputController.SelectionChanged += OnSelectionChanged;
            if (battleManager != null) battleManager.BattleEnded += OnBattleEnded;
            if (miracleManager != null) miracleManager.PrayerStarted += OnPrayerStarted;
            if (endTurnButton != null) endTurnButton.onClick.AddListener(OnEndTurnClicked);
            if (outcomeReturnButton != null) outcomeReturnButton.onClick.AddListener(OnReturnClicked);

            OnSelectionChanged(null);
            if (outcomePanel != null) outcomePanel.SetActive(false);
            RefreshMiracleList();
        }

        private void OnDisable()
        {
            if (inputController != null) inputController.SelectionChanged -= OnSelectionChanged;
            if (battleManager != null) battleManager.BattleEnded -= OnBattleEnded;
            if (miracleManager != null) miracleManager.PrayerStarted -= OnPrayerStarted;
            if (endTurnButton != null) endTurnButton.onClick.RemoveListener(OnEndTurnClicked);
            if (outcomeReturnButton != null) outcomeReturnButton.onClick.RemoveListener(OnReturnClicked);
        }

        private void OnSelectionChanged(UnitInstance unit)
        {
            if (unitInfoText == null) return;

            unitInfoText.text = unit == null
                ? string.Empty
                : $"{unit.Data.displayName}\nPV {unit.CurrentHealth}/{unit.Data.maxHealth}\n" +
                  $"Attaque {unit.Data.attack}   Défense {unit.Data.defense}\n" +
                  $"Déplacement {unit.Data.movement}   Portée {unit.Data.attackRange}" +
                  (unit.Data.canHeal ? $"\nSoin {unit.Data.healAmount} (cliquer un allié)" : string.Empty);
        }

        private void OnEndTurnClicked() => battleManager.EndPlayerPhase();

        private void OnBattleEnded(BattleOutcome outcome)
        {
            if (outcomePanel == null) return;

            outcomePanel.SetActive(true);
            if (outcomeText != null)
            {
                outcomeText.text = outcome == BattleOutcome.Victory ? "Victoire !" : "Défaite";
            }
        }

        private void OnReturnClicked() => SceneManager.LoadScene("Kingdom");

        /// <summary>"1 miracle par bataille" (GDD) — BattleManager.TryCastMiracle already enforces this, but hiding the list once a prayer has started keeps the UI honest about it.</summary>
        private void OnPrayerStarted(MiracleData miracle)
        {
            if (miracleListContainer != null) miracleListContainer.gameObject.SetActive(false);
        }

        private void RefreshMiracleList()
        {
            if (miracleListContainer == null || miracleManager == null) return;

            for (int i = miracleListContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(miracleListContainer.GetChild(i).gameObject);
            }

            foreach (var miracle in miracleManager.UnlockedMiracles)
            {
                if (miracleManager.CanCast(miracle)) CreateMiracleButton(miracle);
            }
        }

        private void CreateMiracleButton(MiracleData miracle)
        {
            var go = new GameObject(miracle.displayName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(miracleListContainer, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(220f, 40f);

            var image = go.GetComponent<Image>();
            image.color = theme != null ? theme.deepBlue : new Color(0.15f, 0.1f, 0.05f, 0.9f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => battleManager.TryCastMiracle(miracle));

            var labelGO = new GameObject("Text", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = miracle.displayName;
            label.fontSize = 16f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = theme != null ? theme.ivoryWhite : Color.white;

            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
        }
    }
}
