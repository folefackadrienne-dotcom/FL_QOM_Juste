using System.Collections.Generic;
using KingdomOfGod.Core;
using KingdomOfGod.Miracles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>
    /// "Menu de prière" (GDD 5. Système de Miracles): a selection view lists castable miracles as
    /// runtime-generated buttons (KingdomTurnManager now gives Kingdom a real turn loop, so the
    /// full BeginPrayer/AdvancePrayerTurn/AccelerateWithFaith/InterruptPrayer ritual can run here
    /// instead of the old instant TryCast stopgap — Battle already drove the same ritual via its
    /// own TurnController); a ritual view replaces it once a prayer starts, showing progress and
    /// letting the player spend extra Foi to accelerate or abandon outright.
    /// </summary>
    public class PrayerMenuUI : MonoBehaviour
    {
        [SerializeField] private MiracleManager miracleManager;
        [SerializeField] private UIThemeData theme;
        [SerializeField] private GameObject panelRoot;

        [SerializeField] private GameObject selectionView;
        [SerializeField] private Transform listContainer;
        [SerializeField] private Button confirmButton;

        [SerializeField] private GameObject ritualView;
        [SerializeField] private TMP_Text ritualStatusText;
        [SerializeField] private Button accelerateButton;
        [SerializeField] private Button abandonButton;
        [SerializeField] private float accelerateFaithAmount = 5f;

        private MiracleData selectedMiracle;

        private void Awake()
        {
            // miracleManager lives on the persistent Bootstrap GameManager, in a different
            // scene from this menu — Inspector references can't cross scenes, so fall back to
            // the running singleton when this field was left unassigned.
            if (miracleManager == null && GameManager.Instance != null)
            {
                miracleManager = GameManager.Instance.Miracles;
            }
        }

        private void OnEnable()
        {
            if (miracleManager != null)
            {
                miracleManager.PrayerStarted += OnPrayerStateChanged;
                miracleManager.PrayerProgressed += OnPrayerProgressed;
                miracleManager.PrayerInterrupted += OnPrayerStateChanged;
                miracleManager.PrayerCancelled += OnPrayerStateChanged;
                miracleManager.MiracleCast += OnPrayerStateChanged;
            }
            if (accelerateButton != null) accelerateButton.onClick.AddListener(OnAccelerateClicked);
            if (abandonButton != null) abandonButton.onClick.AddListener(OnAbandonClicked);
        }

        private void OnDisable()
        {
            if (miracleManager != null)
            {
                miracleManager.PrayerStarted -= OnPrayerStateChanged;
                miracleManager.PrayerProgressed -= OnPrayerProgressed;
                miracleManager.PrayerInterrupted -= OnPrayerStateChanged;
                miracleManager.PrayerCancelled -= OnPrayerStateChanged;
                miracleManager.MiracleCast -= OnPrayerStateChanged;
            }
            if (accelerateButton != null) accelerateButton.onClick.RemoveListener(OnAccelerateClicked);
            if (abandonButton != null) abandonButton.onClick.RemoveListener(OnAbandonClicked);
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

        /// <summary>Switches between the selection list and the in-progress ritual view based on whether a prayer is currently underway.</summary>
        private void Refresh()
        {
            bool praying = miracleManager != null && miracleManager.IsPraying;

            if (selectionView != null) selectionView.SetActive(!praying);
            if (ritualView != null) ritualView.SetActive(praying);

            if (praying) UpdateRitualStatus();
            else RefreshList();
        }

        private void RefreshList()
        {
            if (listContainer == null) return;

            for (int i = listContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(listContainer.GetChild(i).gameObject);
            }

            selectedMiracle = null;
            if (confirmButton != null) confirmButton.interactable = false;

            foreach (var miracle in GetCastableMiracles())
            {
                CreateMiracleButton(miracle);
            }
        }

        private void CreateMiracleButton(MiracleData miracle)
        {
            var go = new GameObject(miracle.displayName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(listContainer, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(260f, 44f);

            var image = go.GetComponent<Image>();
            image.color = theme != null ? theme.deepBlue : new Color(0.15f, 0.1f, 0.05f, 0.9f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => Select(miracle));

            var labelGO = new GameObject("Text", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var label = labelGO.AddComponent<TextMeshProUGUI>();
            label.text = $"{miracle.displayName} ({miracleManager.GetEffectiveFaithCost(miracle):0} Foi)";
            label.fontSize = 15f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = theme != null ? theme.ivoryWhite : Color.white;

            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
        }

        public IReadOnlyList<MiracleData> GetCastableMiracles()
        {
            var castable = new List<MiracleData>();
            foreach (var miracle in miracleManager.UnlockedMiracles)
            {
                if (miracleManager.CanCast(miracle)) castable.Add(miracle);
            }
            return castable;
        }

        public void Select(MiracleData miracle)
        {
            selectedMiracle = miracle;
            if (confirmButton != null) confirmButton.interactable = miracle != null;
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
        }

        /// <summary>Begins the multi-turn ritual (pays the Foi cost up front) rather than resolving instantly — KingdomTurnManager.EndTurn advances it one turn at a time from here on.</summary>
        public void ConfirmCast()
        {
            if (selectedMiracle == null) return;

            if (miracleManager.BeginPrayer(selectedMiracle))
            {
                selectedMiracle = null;
                Refresh();
            }
            else
            {
                GameManager.Instance?.Audio.PlaySfx("Interface - Erreur / Action Impossible");
            }
        }

        private void OnAccelerateClicked()
        {
            if (miracleManager.AccelerateWithFaith(accelerateFaithAmount))
            {
                GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
                Refresh();
            }
            else
            {
                GameManager.Instance?.Audio.PlaySfx("Interface - Erreur / Action Impossible");
            }
        }

        /// <summary>Voluntary abandon reuses the same InterruptPrayer disruption as a Loyalty crisis — no free undo on a prayer already begun.</summary>
        private void OnAbandonClicked()
        {
            miracleManager.InterruptPrayer();
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            Refresh();
        }

        private void UpdateRitualStatus()
        {
            if (ritualStatusText == null) return;

            if (!miracleManager.IsPraying)
            {
                ritualStatusText.text = string.Empty;
                return;
            }

            var miracle = miracleManager.ActiveMiracle;
            int duration = miracleManager.GetEffectivePrayerDuration(miracle);
            ritualStatusText.text = $"Prière en cours : {miracle.displayName} ({miracleManager.PrayerProgressTurns}/{duration})";
        }

        private void OnPrayerStateChanged(MiracleData miracle) => Refresh();
        private void OnPrayerProgressed(MiracleData miracle, int turns) => Refresh();
    }
}
