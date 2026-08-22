using KingdomOfGod.Buildings;
using KingdomOfGod.Core;
using KingdomOfGod.Core.Vfx;
using KingdomOfGod.Grid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomOfGod.Interaction
{
    /// <summary>
    /// Click-to-place input for the Kingdom scene's territory grid: resolves a mouse click to a
    /// HexCoordinates via HexInputUtility, then places selectedBuilding through
    /// BuildingManager.TryPlace. Driven by BuildingPaletteUI's selection buttons — icons on those
    /// buttons still wait on real art (BuildingData.icon), but the click-to-place loop itself no
    /// longer needs a hand-set Inspector field.
    /// </summary>
    public class KingdomInputController : MonoBehaviour
    {
        [SerializeField] private BuildingManager buildingManager;
        [SerializeField] private HexGrid grid;
        [SerializeField] private Camera targetCamera;

        [SerializeField] private BuildingData selectedBuilding;

        private static readonly Color ValidPlacementColor = new Color(0.42f, 0.78f, 0.4f, 1f);
        private static readonly Color InvalidPlacementColor = new Color(0.8f, 0.25f, 0.25f, 1f);
        private static readonly Color NeutralHoverColor = new Color(0.957f, 0.769f, 0.188f, 1f);

        private HexSelectionRing hoverRing;

        public BuildingData SelectedBuilding => selectedBuilding;
        public event System.Action<BuildingData> BuildingSelected;
        public event System.Action<HexCoordinates> CellClicked;

        private void Awake()
        {
            // buildingManager/grid live on the persistent Bootstrap GameManager, in a different
            // scene from this controller — Inspector references can't cross scenes, so fall back
            // to the running singleton when these fields were left unassigned.
            if (buildingManager == null && GameManager.Instance != null)
            {
                buildingManager = GameManager.Instance.Buildings;
            }
            if (grid == null && GameManager.Instance != null)
            {
                grid = GameManager.Instance.Grid;
            }
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        public void SelectBuilding(BuildingData data)
        {
            selectedBuilding = data;
            BuildingSelected?.Invoke(data);
        }

        private void Update()
        {
            UpdateHoverRing();

            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
            if (buildingManager == null || grid == null) return;
            if (!HexInputUtility.TryGetGroundPoint(targetCamera, out var point)) return;

            var coords = HexCoordinates.FromWorldPosition(point, grid.HexSize);
            CellClicked?.Invoke(coords);

            if (selectedBuilding == null) return;

            if (!buildingManager.TryPlace(selectedBuilding, coords, out _))
            {
                GameManager.Instance?.Audio.PlaySfx("Interface - Erreur / Action Impossible");
            }
        }

        /// <summary>Marks the hex cell under the cursor every frame — the only feedback for "this is where a click will land" used to be the HUD's text, nothing pointed at the actual tile. Tinted green/red once a building is selected, based on CanPlace, or a neutral gold otherwise.</summary>
        private void UpdateHoverRing()
        {
            if (grid == null || !HexInputUtility.TryGetGroundPoint(targetCamera, out var point))
            {
                hoverRing?.SetVisible(false);
                return;
            }

            if (hoverRing == null)
            {
                // Parented to the grid rather than this controller's own transform — the controller
                // lives on the camera rig (ProjectSceneSetup.CreateKingdomScene), which pans/zooms
                // every frame, and there's no reason to inherit that when the grid itself never moves.
                hoverRing = HexSelectionRing.Create(grid.transform, grid.HexSize * 0.9f, NeutralHoverColor);
            }

            var coords = HexCoordinates.FromWorldPosition(point, grid.HexSize);
            if (!grid.TryGetCell(coords, out _))
            {
                hoverRing.SetVisible(false);
                return;
            }

            hoverRing.SetVisible(true);
            hoverRing.MoveTo(coords.ToWorldPosition(grid.HexSize));

            if (selectedBuilding != null && buildingManager != null)
            {
                hoverRing.SetColor(buildingManager.CanPlace(selectedBuilding, coords)
                    ? ValidPlacementColor
                    : InvalidPlacementColor);
            }
            else
            {
                hoverRing.SetColor(NeutralHoverColor);
            }
        }
    }
}
