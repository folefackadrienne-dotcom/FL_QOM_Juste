using UnityEngine;

namespace KingdomOfGod.Core.Vfx
{
    /// <summary>
    /// A flat, slowly pulsing gold ring dropped on a hex cell — the only visual feedback for
    /// "this is the cell the cursor/selection currently means" today comes from HUD text, nothing
    /// marks the actual tile. Texture generated once at runtime (radial ring alpha), shared by every
    /// ring instance. Used both as a continuously-updated hover marker (KingdomInputController) and
    /// an event-driven selection marker (BattleInputController).
    /// </summary>
    public class HexSelectionRing : MonoBehaviour
    {
        private static Texture2D cachedTexture;
        private MeshRenderer ringRenderer;

        public static HexSelectionRing Create(Transform parent, float radius, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "HexSelectionRing";
            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);

            go.transform.SetParent(parent, false);
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateMaterial(color);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            var ring = go.AddComponent<HexSelectionRing>();
            ring.ringRenderer = renderer;
            return ring;
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        public void MoveTo(Vector3 worldPosition, float verticalOffset = 0.03f)
        {
            transform.position = new Vector3(worldPosition.x, worldPosition.y + verticalOffset, worldPosition.z);
        }

        public void SetColor(Color color)
        {
            if (ringRenderer != null) ringRenderer.sharedMaterial.color = color;
        }

        private void Update()
        {
            float pulse = 0.75f + Mathf.Sin(Time.time * 3f) * 0.25f;
            if (ringRenderer != null)
            {
                var color = ringRenderer.sharedMaterial.color;
                color.a = pulse;
                ringRenderer.sharedMaterial.color = color;
            }
        }

        private static Material CreateMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Transparent")
                ?? Shader.Find("Sprites/Default");
            var material = new Material(shader) { name = "HexSelectionRingMaterial" };
            var texture = GetOrCreateTexture();

            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f); // URP: 1 = Transparent
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.color = color;
            return material;
        }

        private static Texture2D GetOrCreateTexture()
        {
            if (cachedTexture != null) return cachedTexture;

            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "HexSelectionRingGradient",
                wrapMode = TextureWrapMode.Clamp
            };

            var center = new Vector2(size / 2f, size / 2f);
            float maxDist = size / 2f;
            const float ringInner = 0.72f;
            const float ringOuter = 0.92f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                    float alpha;
                    if (dist < ringInner || dist > 1f)
                    {
                        alpha = 0f;
                    }
                    else if (dist < ringOuter)
                    {
                        alpha = Mathf.InverseLerp(ringInner, ringOuter, dist);
                    }
                    else
                    {
                        alpha = Mathf.InverseLerp(1f, ringOuter, dist);
                    }
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            cachedTexture = texture;
            return texture;
        }
    }
}
