using UnityEngine;

namespace KingdomOfGod.Core.Vfx
{
    /// <summary>Small continuous bob + scale pulse for battle unit billboards, so a flat sprite standing still doesn't read as a static cardboard cutout — buildings stay put (nothing alive to animate), only units get this.</summary>
    public class IdleBob : MonoBehaviour
    {
        [SerializeField] private float bobAmplitude = 0.04f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float scalePulseAmount = 0.03f;

        private Vector3 basePosition;
        private Vector3 baseScale;
        private float phaseOffset;

        // Awake, not Start: guaranteed to run before PopInScale's Start (which zeroes this same
        // transform's scale to animate it back up) even though the two components' own Start-call
        // order isn't guaranteed — Awake-before-any-Start is.
        private void Awake()
        {
            basePosition = transform.localPosition;
            baseScale = transform.localScale;
            phaseOffset = Random.Range(0f, Mathf.PI * 2f); // desync units spawned at the same time
        }

        private void Update()
        {
            float wave = Mathf.Sin(Time.time * bobSpeed + phaseOffset);
            transform.localPosition = basePosition + new Vector3(0f, wave * bobAmplitude, 0f);
            transform.localScale = baseScale * (1f + wave * scalePulseAmount);
        }
    }
}
