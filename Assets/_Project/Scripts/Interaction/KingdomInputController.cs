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
    /// BuildingManager.TryPlace. No building-selection palette UI exists yet (needs the 39
    /// BuildingData icons, which need real art) — selectedBuilding is exposed both as an
    /// Inspector field for manual playtesting and as SelectBuilding(BuildingData) for a future
    /// palette to call.
    /// </summary>
    public class KingdomInputController : MonoBehaviour
    {
        [SerializeField] private BuildingManager buildingManager;
        [SerializeField] private HexGrid grid;
        [SerializeField] private Camera targetCamera;

        [Tooltip("Building placed on left-click, until a real selection palette exists. Assign one of the BuildingData assets to playtest placement.")]
        [SerializeField] private BuildingData selectedBuilding;

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

        public void SelectBuilding(BuildingData data) => selectedBuilding = data;

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
