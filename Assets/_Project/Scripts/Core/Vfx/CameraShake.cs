using UnityEngine;

namespace KingdomOfGod.Core.Vfx
{
    /// <summary>
    /// A few frames of random positional jitter on the active camera to sell the weight of a
    /// combat hit — lazily attaches itself to Camera.main the first time Shake is called, so no
    /// scene wiring is needed in either Kingdom or Battle. Runs in LateUpdate, after
    /// HexCameraController's Update has already applied the frame's pan/zoom, and only ever adds a
    /// self-cancelling offset on top of it (undoing last frame's jitter before adding this frame's)
    /// so panning during a shake doesn't fight or get overwritten by it.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        private float remaining;
        private float duration;
        private float magnitude;
        private Vector3 previousJitter;

        public static void Shake(float shakeDuration = 0.15f, float shakeMagnitude = 0.12f)
        {
            var cam = Camera.main;
            if (cam == null) return;

            var shake = cam.GetComponent<CameraShake>();
            if (shake == null) shake = cam.gameObject.AddComponent<CameraShake>();
            shake.duration = shakeDuration;
            shake.remaining = shakeDuration;
            shake.magnitude = shakeMagnitude;
        }

        private void LateUpdate()
        {
            if (remaining <= 0f)
            {
                if (previousJitter != Vector3.zero)
                {
                    transform.localPosition -= previousJitter;
                    previousJitter = Vector3.zero;
                }
                return;
            }

            transform.localPosition -= previousJitter;

            remaining -= Time.deltaTime;
            float damper = duration > 0f ? Mathf.Clamp01(remaining / duration) : 0f;
            previousJitter = (Vector3)Random.insideUnitCircle * magnitude * damper;
            transform.localPosition += previousJitter;
        }
    }
}
