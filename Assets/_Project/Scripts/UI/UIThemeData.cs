using System;
using System.Collections.Generic;
using KingdomOfGod.Core;
using UnityEngine;

namespace KingdomOfGod.UI
{
    [Serializable]
    public struct AgeAccent
    {
        public Age age;
        public Color accent;
    }

    /// <summary>
    /// The concrete color values behind docs/ArtDirection.md section 2 ("Palette de couleurs").
    /// Colors don't need external art files to be real — unlike sprites/meshes/audio clips, this
    /// is the one piece of the visual identity brief this project can actually apply today.
    /// </summary>
    [CreateAssetMenu(fileName = "UITheme", menuName = "Kingdom of God/UI Theme", order = 60)]
    public class UIThemeData : ScriptableObject
    {
        [Header("Palette dominante")]
        public Color ochre = new Color(0.800f, 0.467f, 0.133f);
        public Color warmGold = new Color(0.831f, 0.627f, 0.090f);
        public Color deepBlue = new Color(0.090f, 0.196f, 0.310f);
        public Color purpleRed = new Color(0.420f, 0.118f, 0.235f);
        public Color oliveGreen = new Color(0.420f, 0.478f, 0.227f);
        public Color ivoryWhite = new Color(0.961f, 0.941f, 0.882f);

        [Header("Couleurs sémantiques")]
        [Tooltip("Lumière divine (miracles, présence de Dieu) — toujours or chaud lumineux.")]
        public Color divineLight = new Color(0.957f, 0.769f, 0.188f);
        [Tooltip("Idolâtrie / malédiction / Alliance basse.")]
        public Color crisisRed = new Color(0.361f, 0.102f, 0.102f);
        [Tooltip("Restauration / repentance — teinte de transition entre crisisRed et divineLight.")]
        public Color restorationAmber = new Color(0.788f, 0.545f, 0.231f);

        [Header("Chrome UI")]
        [Tooltip("Bordures dorées fines.")]
        public Color goldBorder = new Color(0.831f, 0.627f, 0.090f);
        [Tooltip("Fond semi-transparent parchemin/pierre claire — repli de couleur plate tant que panelBackgroundSprite n'est pas assignée.")]
        public Color parchmentBackground = new Color(0.922f, 0.871f, 0.753f, 0.92f);
        [Tooltip("Texture parchemin 9-slice pour le fond des panneaux (AddParchmentBackground) — remplace parchmentBackground une fois assignée.")]
        public Sprite panelBackgroundSprite;
        public Color panelText = new Color(0.200f, 0.133f, 0.078f);
        [Tooltip("Bordure 9-slice dorée ornementée (étoiles de David, menorahs) — bande épaisse (~160px sur une texture source 1408x768), pensée pour les grands panneaux (AddParchmentBackground). Non utilisée sur les petits boutons (240x50) : une bordure aussi épaisse les mangerait entièrement, donc CreateButton garde le repli Outline même quand ce champ est assigné.")]
        public Sprite goldBorderSprite;
        [Tooltip("Image de fond plein écran du Menu Principal — remplace l'aplat deepBlue une fois assignée.")]
        public Sprite menuBackgroundSprite;

        [Header("Ambiance par Âge — pas encore branché sur un rendu visuel, prêt pour quand HexGrid en aura un")]
        public List<AgeAccent> ageAccents = new List<AgeAccent>();

        public Color GetAgeAccent(Age age)
        {
            foreach (var entry in ageAccents)
            {
                if (entry.age == age) return entry.accent;
            }
            return ochre;
        }
    }
}
