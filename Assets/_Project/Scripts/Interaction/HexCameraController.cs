using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomOfGod.Interaction
{
    /// <summary>Free RTS-style camera for the Kingdom/Battle hex grids — WASD/arrow-key pan, scroll-wheel zoom. "PC = interface dense, tooltips riches, caméra libre" (docs/ArtDirection.md section UI).</summary>
    public class HexCameraController : MonoBehaviour
    {
        [SerializeField] private float panSpeed = 12f;
        [SerializeField] private float zoomSpeed = 8f;
        [SerializeField] private float minHeight = 4f;
        [SerializeField] private float maxHeight = 30f;

        private void Update()
        {
            if (Keyboard.current != null)
            {
                Vector3 move = Vector3.zero;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) move += Vector3.forward;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) move += Vector3.back;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) move += Vector3.left;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) move += Vector3.right;

                if (move != Vector3.zero)
                {
                    transform.position += move.normalized * (panSpeed * Time.deltaTime);
                }
            }

            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > Mathf.Epsilon)
                {
                    var position = transform.position;
                    position.y = Mathf.Clamp(position.y - scroll * zoomSpeed * Time.deltaTime, minHeight, maxHeight);
                    transform.position = position;
                }
            }
        }
    }
}
