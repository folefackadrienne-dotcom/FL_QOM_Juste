using UnityEngine;

namespace KingdomOfGod.Core.Vfx
{
    /// <summary>
    /// One-shot particle bursts (construction dust, combat sparks) built from Unity's built-in
    /// particle material — the same Resources.GetBuiltinResource technique MiracleVfxController
    /// already uses for the prayer glow, so no texture needs to be produced first. The GameObject
    /// destroys itself once every particle has finished its lifetime.
    /// </summary>
    public static class OneShotParticles
    {
        public static void Burst(Vector3 worldPosition, Color color, int count = 14, float size = 0.18f,
            float speed = 1.6f, float lifetime = 0.6f)
        {
            var go = new GameObject("OneShotParticles");
            go.transform.position = worldPosition;

            var ps = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = Resources.GetBuiltinResource<Material>("Default-Particle.mat");
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.startColor = color;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.maxParticles = count + 1;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;

            ps.Play();
            Object.Destroy(go, lifetime + 0.5f);
        }
    }
}
