using KingdomOfGod.Miracles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomOfGod.UI
{
    /// <summary>One row of PrayerMenuUI's castable-miracles list — instantiated once per miracle, see PrayerMenuUI.RefreshList.</summary>
    public class MiracleListItemUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private Image icon;
        [SerializeField] private Button selectButton;

        public void Bind(MiracleData miracle, PrayerMenuUI owner)
        {
            if (nameLabel != null) nameLabel.text = miracle.displayName;
            if (icon != null) icon.sprite = miracle.icon;

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => owner.Select(miracle));
            }
        }
    }
}
