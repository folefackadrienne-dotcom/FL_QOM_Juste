using System.Collections.Generic;
using KingdomOfGod.Core;
using KingdomOfGod.Resources;
using TMPro;
using UnityEngine;

namespace KingdomOfGod.UI
{
    [System.Serializable]
    public struct ResourceLabel
    {
        public ResourceType type;
        public TMP_Text valueText;
    }

    /// <summary>HUD strip showing all 7 resources; subscribes to ResourceManager.ResourceChanged.</summary>
    public class ResourceBarUI : MonoBehaviour
    {
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private List<ResourceLabel> labels = new List<ResourceLabel>();

        private void OnEnable()
        {
            // resourceManager lives on the persistent Bootstrap GameManager, in a different
            // scene from this HUD — Inspector references can't cross scenes, so fall back to
            // the running singleton when this field was left unassigned.
            if (resourceManager == null && GameManager.Instance != null)
            {
                resourceManager = GameManager.Instance.Resources;
            }

            resourceManager.ResourceChanged += OnResourceChanged;
            foreach (var label in labels)
            {
                OnResourceChanged(label.type, resourceManager.Get(label.type));
            }
        }

        private void OnDisable()
        {
            resourceManager.ResourceChanged -= OnResourceChanged;
        }

        private void OnResourceChanged(ResourceType type, float value)
        {
            foreach (var label in labels)
            {
                if (label.type == type && label.valueText != null)
                {
                    label.valueText.text = Mathf.FloorToInt(value).ToString();
                }
            }
        }
    }
}
