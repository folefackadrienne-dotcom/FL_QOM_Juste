using KingdomOfGod.Core;
using KingdomOfGod.UI;
using UnityEngine;

namespace KingdomOfGod.Miracles
{
    /// <summary>
    /// Code-generated placeholder VFX for the prayer ritual — MiracleManager.PrayerStarted/
    /// MiracleCast/PrayerCancelled were already fully wired to SFX (see AudioManager), but nothing
    /// visual ever consumed them. A single world-space ParticleSystem built entirely from Unity's
    /// built-in default particle material (Resources.GetBuiltinResource) so no texture/art needs
    /// to be produced first: a soft golden glow loops while a prayer is in progress
    /// (docs/ArtDirection.md: "montée lumineuse pendant la prière"), bursts once on MiracleCast,
    /// and stops on PrayerCancelled. Anchored at a fixed point above the map's world origin since
    /// prayer isn't tied to a specific unit or building position in either Kingdom or Battle.
    /// </summary>
    public class MiracleVfxController : MonoBehaviour
    {
        [SerializeField] private MiracleManager miracleManager;
        [SerializeField] private UIThemeData theme;
        [SerializeField] private Vector3 anchorPosition = new Vector3(0f, 2f, 0f);

        private ParticleSystem prayerGlow;

        private void Awake()
        {
            // miracleManager lives on the persistent Bootstrap GameManager, in a different scene
            // from this controller in Kingdom's case (Battle resolves the same way) — Inspector
            // references can't cross scenes, so fall back to the running singleton when left
            // unassigned.
            if (miracleManager == null && GameManager.Instance != null)
            {
                miracleManager = GameManager.Instance.Miracles;
            }

            BuildParticleSystem();
        }

        private void OnEnable()
        {
            if (miracleManager != null)
            {
                miracleManager.PrayerStarted += OnPrayerStarted;
                miracleManager.MiracleCast += OnMiracleCast;
                miracleManager.PrayerCancelled += OnPrayerCancelled;
            }
        }

        private void OnDisable()
        {
            if (miracleManager != null)
            {
                miracleManager.PrayerStarted -= OnPrayerStarted;
                miracleManager.MiracleCast -= OnMiracleCast;
                miracleManager.PrayerCancelled -= OnPrayerCancelled;
            }
        }

        private void BuildParticleSystem()
        {
            var go = new GameObject("PrayerGlowVfx");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = anchorPosition;

            prayerGlow = go.AddComponent<ParticleSystem>();

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = Resources.GetBuiltinResource<Material>("Default-Particle.mat");
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            var color = theme != null ? theme.divineLight : new Color(0.957f, 0.769f, 0.188f);

            var main = prayerGlow.main;
            main.playOnAwake = false;
            main.loop = true;
            main.startColor = color;
            main.startLifetime = 1.5f;
            main.startSpeed = 0.6f;
            main.startSize = 0.35f;
            main.maxParticles = 200;

            var emission = prayerGlow.emission;
            emission.rateOverTime = 8f;

            var shape = prayerGlow.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.4f;

            var colorOverLifetime = prayerGlow.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;

            prayerGlow.Stop();
        }

        private void OnPrayerStarted(MiracleData miracle) => prayerGlow.Play();

        /// <summary>One burst of extra particles marking the miracle's resolution, then the ambient loop stops (existing particles still fade out naturally — ParticleSystem.Stop() only halts new emission).</summary>
        private void OnMiracleCast(MiracleData miracle)
        {
            prayerGlow.Emit(60);
            prayerGlow.Stop();
        }

        private void OnPrayerCancelled(MiracleData miracle) => prayerGlow.Stop();
    }
}
