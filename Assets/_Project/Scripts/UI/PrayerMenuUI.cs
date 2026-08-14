using System.Collections.Generic;
using KingdomOfGod.Core;
using KingdomOfGod.Miracles;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>
    /// "Menu de prière" (GDD 5. Système de Miracles): lists castable miracles, shows the
    /// associated verse, and confirms the cast. Population of the list (buttons per miracle,
    /// icons, verse text panel) is left to the concrete UI prefab.
    /// </summary>
    public class PrayerMenuUI : MonoBehaviour
    {
        [SerializeField] private MiracleManager miracleManager;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button confirmButton;

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

        public void Open() => panelRoot.SetActive(true);
        public void Close() => panelRoot.SetActive(false);

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
        }

        /// <summary>
        /// Casts instantly rather than going through MiracleManager's multi-turn prayer gauge,
        /// since the Kingdom scene has no turn-advance loop yet to drive it — Battle already
        /// drives the full ritual (BeginPrayer/AdvancePrayerTurn) via its TurnController.
        /// </summary>
        public void ConfirmCast()
        {
            if (selectedMiracle == null) return;

            if (miracleManager.TryCast(selectedMiracle))
            {
                Close();
            }
        }
    }
}
