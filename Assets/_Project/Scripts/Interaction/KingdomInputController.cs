using KingdomOfGod.Buildings;
using KingdomOfGod.Core;
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
    }
}
