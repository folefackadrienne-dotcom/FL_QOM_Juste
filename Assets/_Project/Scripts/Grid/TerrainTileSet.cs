using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomOfGod.Grid
{
    /// <summary>
    /// Optional per-TerrainType texture lookup for HexGridRenderer's fill tiles — kept separate
    /// from UIThemeData (UI colors only) since this is grid/world-rendering content. A TerrainType
    /// with no entry (or a null texture) falls back to HexGridRenderer's existing flat
    /// UIThemeData color, so partial art coverage (e.g. only a few terrains textured) degrades
    /// gracefully instead of leaving unmapped types blank.
    /// </summary>
    [CreateAssetMenu(fileName = "TerrainTileSet", menuName = "Kingdom of God/Terrain Tile Set", order = 20)]
    public class TerrainTileSet : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public TerrainType terrain;
            public Texture2D texture;
        }

        public List<Entry> entries = new List<Entry>();

        public Texture2D GetTexture(TerrainType terrain)
        {
            foreach (var entry in entries)
            {
                if (entry.terrain == terrain) return entry.texture;
            }
            return null;
        }
    }
}
