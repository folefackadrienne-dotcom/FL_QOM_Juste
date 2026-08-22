using UnityEngine;

namespace KingdomOfGod.Core.Vfx
{
    /// <summary>
    /// A soft radial-gradient blob under a billboard sprite so a flat 2D cutout reads as resting on
    /// the ground rather than floating — the texture is generated once at runtime (a radial alpha
    /// falloff, nothing hand-drawn) and shared by every shadow instance.
    /// </summary>
    public static class GroundShadow
    {
        private static Texture2D cachedTexture;

        public static void Attach(Transform parent, float radius, float verticalOffset = 0.02f)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "GroundShadow";
            var collider = go.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);

            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, verticalOffset, 0f);
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = CreateMaterial();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static Material CreateMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Transparent")
                ?? Shader.Find("Sprites/Default");
            var material = new Material(shader) { name = "GroundShadowMaterial" };
            var texture = GetOrCreateTexture();

            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f); // URP: 1 = Transparent
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return material;
        }

        private static Texture2D GetOrCreateTexture()
        {
            if (cachedTexture != null) return cachedTexture;

            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "GroundShadowGradient",
                wrapMode = TextureWrapMode.Clamp
            };

            var center = new Vector2(size / 2f, size / 2f);
            float maxDist = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                    float alpha = Mathf.Clamp01(1f - dist) * 0.35f;
                    texture.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
                }
            }

            texture.Apply();
            cachedTexture = texture;
            return texture;
        }
    }
}
