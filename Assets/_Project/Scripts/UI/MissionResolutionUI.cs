using System.Collections.Generic;
using KingdomOfGod.Core;
using KingdomOfGod.Missions;
using KingdomOfGod.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>
    /// Resolution panel for the 5 non-Battle mission types — MissionListUI opens this right after
    /// MissionManager.StartMission for anything that isn't a MissionType.Battle mission, since
    /// nothing used to happen next for those: StartMission set ActiveMission and nothing else ever
    /// read it. Rebuilds its action buttons per mission.type on every Open() (same
    /// clear-then-repopulate pattern as BuildingPaletteUI.RefreshList): Construction/Survival show
    /// a resource requirement and one action button, MoralChoice/Diplomacy show two named-option
    /// buttons, Sandbox shows a single unconditional completion button.
    /// </summary>
    public class MissionResolutionUI : MonoBehaviour
    {
        [SerializeField] private MissionManager missionManager;
        [SerializeField] private UIThemeData theme;

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text detailText;
        [SerializeField] private Transform actionContainer;
        [SerializeField] private Button closeButton;

        private MissionData currentMission;

        private void Awake()
        {
            // missionManager lives on the persistent Bootstrap GameManager, in a different scene
            // from this panel — Inspector references can't cross scenes, so fall back to the
            // running singleton when this field was left unassigned.
            if (missionManager == null && GameManager.Instance != null)
            {
                missionManager = GameManager.Instance.Missions;
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

        public void Open(MissionData mission)
        {
            currentMission = mission;
            panelRoot.SetActive(true);
            GameManager.Instance?.Audio.PlaySfx("Interface - Ouverture de Menu");
            Refresh();
        }

        public void Close()
        {
            panelRoot.SetActive(false);
            GameManager.Instance?.Audio.PlaySfx("Interface - Fermeture de Menu");
        }

        private void Refresh()
        {
            if (currentMission == null) return;

            if (titleText != null) titleText.text = currentMission.displayName;
            if (summaryText != null) summaryText.text = currentMission.summary;
            if (detailText != null) detailText.text = string.Empty;

            ClearActions();

            switch (currentMission.type)
            {
                case MissionType.Construction:
                    SetDetail($"Coût : {FormatResources(currentMission.constructionCost)}");
                    CreateActionButton("Contribuer", OnConstructionClicked);
                    break;
                case MissionType.Survival:
                    SetDetail($"Ressources requises : {FormatResources(currentMission.survivalRequirement)}");
                    CreateActionButton("Vérifier", OnSurvivalClicked);
                    break;
                case MissionType.MoralChoice:
                    CreateActionButton(currentMission.optionA.label, () => OnMoralChoiceClicked(true));
                    CreateActionButton(currentMission.optionB.label, () => OnMoralChoiceClicked(false));
                    break;
                case MissionType.Diplomacy:
                    CreateActionButton(currentMission.optionA.label, () => OnDiplomacyClicked(true));
                    CreateActionButton(currentMission.optionB.label, () => OnDiplomacyClicked(false));
                    break;
                case MissionType.Sandbox:
                    CreateActionButton("Terminer", OnSandboxClicked);
                    break;
            }
        }

        private void OnConstructionClicked()
        {
            if (missionManager.TryResolveConstruction())
            {
                GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
                Close();
            }
            else
            {
                SetDetail("Ressources insuffisantes pour l'instant.");
                GameManager.Instance?.Audio.PlaySfx("Interface - Erreur / Action Impossible");
            }
        }

        private void OnSurvivalClicked()
        {
            if (missionManager.TryResolveSurvival())
            {
                GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
                Close();
            }
            else
            {
                SetDetail("Pas encore assez de ressources en réserve — continuez à accumuler.");
                GameManager.Instance?.Audio.PlaySfx("Interface - Erreur / Action Impossible");
            }
        }

        private void OnMoralChoiceClicked(bool chooseOptionA)
        {
            missionManager.ResolveMoralChoice(chooseOptionA);
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            Close();
        }

        private void OnDiplomacyClicked(bool chooseOptionA)
        {
            missionManager.ResolveDiplomacy(chooseOptionA);
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            Close();
        }

        private void OnSandboxClicked()
        {
            missionManager.ResolveSandbox();
            GameManager.Instance?.Audio.PlaySfx("Interface - Clic sur Parchemin");
            Close();
        }

        private void SetDetail(string text)
        {
            if (detailText != null) detailText.text = text;
        }

        private static string FormatResources(List<ResourceAmount> amounts)
        {
            if (amounts == null || amounts.Count == 0) return "aucune";

            var parts = new List<string>();
            foreach (var amount in amounts)
            {
                parts.Add($"{amount.type} {amount.amount:0}");
            }
            return string.Join(", ", parts);
        }

        private void ClearActions()
        {
            if (actionContainer == null) return;

            for (int i = actionContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(actionContainer.GetChild(i).gameObject);
            }
        }

        private void CreateActionButton(string label, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(actionContainer, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(260f, 44f);

            var image = go.GetComponent<Image>();
            image.color = theme != null ? theme.deepBlue : new Color(0.15f, 0.1f, 0.05f, 0.9f);

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            var labelGO = new GameObject("Text", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var text = labelGO.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 16f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = theme != null ? theme.ivoryWhite : Color.white;

            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
        }
    }
}
