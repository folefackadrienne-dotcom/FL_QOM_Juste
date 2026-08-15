using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomOfGod.Interaction
{
    /// <summary>Shared mouse-to-ground-plane raycast used by both KingdomInputController and BattleInputController — neither hex grid has a visual mesh/collider yet, so this intersects the mathematical y=0 plane instead of using Physics.Raycast.</summary>
    internal static class HexInputUtility
    {
        public static bool TryGetGroundPoint(Camera camera, out Vector3 point)
        {
            point = default;
            if (camera == null || Mouse.current == null) return false;

            var ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            var groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (!groundPlane.Raycast(ray, out float distance)) return false;

            point = ray.GetPoint(distance);
            return true;
        }
    }
}
