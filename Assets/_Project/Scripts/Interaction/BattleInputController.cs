using KingdomOfGod.Battle;
using KingdomOfGod.Core;
using KingdomOfGod.Grid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomOfGod.Interaction
{
    /// <summary>
    /// Click-to-select/move/attack input for the Battle scene: first click on an own unit selects
    /// it, a second click resolves through BattleManager — an enemy-occupied cell attacks, an
    /// empty cell moves, anything else deselects. battleManager/battleGrid are local to the
    /// Battle scene (created together in ProjectSceneSetup.CreateBattleScene), so unlike the
    /// Kingdom-side controller this needs no cross-scene GameManager.Instance fallback.
    /// </summary>
    public class BattleInputController : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private BattleGrid battleGrid;
        [SerializeField] private Camera targetCamera;

        public UnitInstance SelectedUnit { get; private set; }
        public event System.Action<UnitInstance> SelectionChanged;

        private void Awake()
        {
            if (targetCamera == null) targetCamera = Camera.main;
        }

        private void Update()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
            if (battleManager == null || battleGrid == null) return;
            if (!HexInputUtility.TryGetGroundPoint(targetCamera, out var point)) return;

            var coords = HexCoordinates.FromWorldPosition(point, battleGrid.Grid.HexSize);
            var unitAtCell = battleGrid.GetUnitAt(coords);

            if (SelectedUnit == null)
            {
                if (unitAtCell != null && unitAtCell.Allegiance == Allegiance.Player)
                {
                    Select(unitAtCell);
                }
                return;
            }

            if (unitAtCell != null && unitAtCell != SelectedUnit && unitAtCell.Allegiance == Allegiance.Player)
            {
                Select(unitAtCell);
                return;
            }

            bool acted = unitAtCell != null
                ? battleManager.TryAttack(SelectedUnit, unitAtCell)
                : battleManager.TryMove(SelectedUnit, coords);

            if (!acted)
            {
                GameManager.Instance?.Audio.PlaySfx("Interface - Erreur / Action Impossible");
            }

            Select(null);
        }

        private void Select(UnitInstance unit)
        {
            SelectedUnit = unit;
            SelectionChanged?.Invoke(unit);
        }
    }
}
