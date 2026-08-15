using UnityEngine;

namespace KingdomOfGod.Buildings
{
    /// <summary>Keeps a world-space label facing the active camera — used by BuildingManager's placeholder building visuals (no building art exists yet) so their name text stays readable as the player pans/zooms.</summary>
    public class BillboardLabel : MonoBehaviour
    {
        private void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null) return;
            transform.rotation = cam.transform.rotation;
        }
    }
}
