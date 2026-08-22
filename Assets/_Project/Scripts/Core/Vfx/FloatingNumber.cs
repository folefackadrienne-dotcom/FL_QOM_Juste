using System.Collections;
using KingdomOfGod.Core;
using TMPro;
using UnityEngine;

namespace KingdomOfGod.Core.Vfx
{
    /// <summary>Spawns a world-space "-12"/"+6" style popup that rises and fades before self-destroying — turns a silent stat change (damage, heal, resource gain) into something the player actually sees happen where it happened.</summary>
    public static class FloatingNumber
    {
        public static void Spawn(Vector3 worldPosition, string text, Color color, float fontSize = 5f)
        {
            var go = new GameObject("FloatingNumber");
            go.transform.position = worldPosition;

            var label = go.AddComponent<TextMeshPro>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = color;

            go.AddComponent<BillboardLabel>();
            var motion = go.AddComponent<FloatingNumberMotion>();
            motion.Begin();
        }
    }

    /// <summary>Drives one FloatingNumber instance's rise-and-fade, then destroys its GameObject.</summary>
    public class FloatingNumberMotion : MonoBehaviour
    {
        [SerializeField] private float riseDistance = 0.8f;
        [SerializeField] private float duration = 1.0f;

        private TextMeshPro label;
        private Vector3 startPosition;

        public void Begin()
        {
            label = GetComponent<TextMeshPro>();
            startPosition = transform.position;
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            float elapsed = 0f;
            var baseColor = label != null ? label.color : Color.white;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                transform.position = startPosition + new Vector3(0f, riseDistance * t, 0f);
                if (label != null)
                {
                    var faded = baseColor;
                    faded.a = 1f - t;
                    label.color = faded;
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
