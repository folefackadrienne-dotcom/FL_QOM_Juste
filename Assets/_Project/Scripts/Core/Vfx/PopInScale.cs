using System.Collections;
using UnityEngine;

namespace KingdomOfGod.Core.Vfx
{
    /// <summary>Scales a freshly spawned billboard up from nothing over a short ease-out — breaks the "instantly there" flatness of a placed building/unit popping into existence at full size.</summary>
    public class PopInScale : MonoBehaviour
    {
        [SerializeField] private float duration = 0.35f;

        // Captured in Awake, not Start: Unity guarantees every component's Awake on a GameObject
        // runs before any of their Start calls, but not Start-vs-Start order between different
        // components on the same object — IdleBob (added alongside this on the same sprite) reads
        // this object's scale in its own Awake too, so whichever of the two happened to run its
        // Start first would otherwise risk capturing an already-zeroed scale.
        private void Awake()
        {
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            var targetScale = transform.localScale;
            transform.localScale = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f); // ease-out cubic
                transform.localScale = targetScale * eased;
                yield return null;
            }

            transform.localScale = targetScale;
        }
    }
}
