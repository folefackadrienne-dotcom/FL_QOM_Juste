using System.Collections.Generic;
using KingdomOfGod.Core;
using KingdomOfGod.Interaction;
using KingdomOfGod.UI;
using UnityEngine;

namespace KingdomOfGod.Grid
{
    /// <summary>
    /// Runtime, code-only visual for a HexGrid: no tilemap/mesh asset existed anywhere before
    /// this, so clicks resolved to real HexCoordinates but nothing on screen ever showed a grid.
    /// Builds two combined meshes per HexCell.TerrainType present (a darker "border" hex and a
    /// smaller lighter "fill" hex on top, both flat-colored — the same UITheme-instead-of-art
    /// discipline as WorldMoodUI/BuildingPaletteUI) so terrain variety shows up automatically the
    /// moment something starts assigning HexCell.Terrain (all cells default to Plain today, no
    /// generator writes the other 7 TerrainType values yet). The fill tint for Plain cells comes
    /// from UIThemeData.GetAgeAccent(AgeManager.CurrentAge), a hook that already existed but had
    /// no visual consumer. Also tracks the mouse-hovered cell with a moving highlight tile reusing
    /// HexInputUtility's ground-plane raycast, so clicking finally has visual feedback.
    /// Fill tiles use a real texture per TerrainType when tileSet provides one — each hex fan
    /// unwraps to its own independent unit-circle UV, since the source art is a set of
    /// self-contained hex illustrations rather than a seamlessly tileable pattern. A TerrainType
    /// with no texture (or no tileSet at all) keeps the flat UITheme color it always had.
    /// </summary>
    public class HexGridRenderer : MonoBehaviour
    {
        [SerializeField] private HexGrid grid;
        [SerializeField] private UIThemeData theme;
        [SerializeField] private TerrainTileSet tileSet;
        [SerializeField] private AgeManager ageManager;
        [SerializeField] private Camera targetCamera;

        [SerializeField] private float borderScale = 0.98f;
        [SerializeField] private float fillScale = 0.86f;
        [SerializeField] private float hoverScale = 0.92f;

        private const float FillHeight = 0.01f;
        private const float HoverHeight = 0.02f;

        private GameObject hoverGO;

        private void Awake()
        {
            // grid/ageManager can live on the persistent Bootstrap GameManager in Kingdom's case
            // (Battle wires its local BattleGrid directly) — Inspector references can't cross
            // scenes, so fall back to the running singleton when left unassigned.
            if (grid == null && GameManager.Instance != null) grid = GameManager.Instance.Grid;
            if (ageManager == null && GameManager.Instance != null) ageManager = GameManager.Instance.Ages;
            if (targetCamera == null) targetCamera = Camera.main;
        }

        private void Start()
        {
            BuildGrid();
        }

        private void Update()
        {
            UpdateHover();
        }

        /// <summary>(Re)builds the tile meshes from the current grid contents. Safe to call again after HexGrid.GenerateHexagonalMap changes the map.</summary>
        public void BuildGrid()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            hoverGO = null;

            if (grid == null) return;

            var byTerrain = new Dictionary<TerrainType, List<Vector3>>();
            var allCenters = new List<Vector3>();
            foreach (var cell in grid.Cells.Values)
            {
                var world = cell.Coordinates.ToWorldPosition(grid.HexSize);
                allCenters.Add(world);

                if (!byTerrain.TryGetValue(cell.Terrain, out var list))
                {
                    list = new List<Vector3>();
                    byTerrain[cell.Terrain] = list;
                }
                list.Add(world);
            }

            var accent = GetAgeAccent();
            var borderColor = Multiply(accent, 0.5f);
            CreateLayer("HexBorderTiles", allCenters, grid.HexSize * borderScale, 0f, borderColor);

            foreach (var pair in byTerrain)
            {
                var layerName = $"HexFillTiles_{pair.Key}";
                var mesh = BuildCombinedHexMesh(pair.Value, grid.HexSize * fillScale, FillHeight);
                var texture = tileSet != null ? tileSet.GetTexture(pair.Key) : null;
                var material = texture != null ? CreateTexturedMaterial(texture) : CreateFlatMaterial(GetTerrainColor(pair.Key, accent));
                CreateTileObject(layerName, mesh, material);
            }

            BuildHoverTile();
        }

        private Color GetAgeAccent()
        {
            if (theme != null && ageManager != null) return theme.GetAgeAccent(ageManager.CurrentAge);
            return new Color(0.42f, 0.478f, 0.227f);
        }

        private Color GetTerrainColor(TerrainType terrain, Color plainAccent)
        {
            if (theme == null) return plainAccent;

            switch (terrain)
            {
                case TerrainType.Desert: return theme.ochre;
                case TerrainType.Hill: return Multiply(theme.ochre, 0.75f);
                case TerrainType.Mountain: return Color.Lerp(theme.ivoryWhite, theme.panelText, 0.55f);
                case TerrainType.Forest: return Multiply(theme.oliveGreen, 0.8f);
                case TerrainType.River: return theme.deepBlue;
                case TerrainType.Coast: return Color.Lerp(theme.deepBlue, theme.ivoryWhite, 0.5f);
                case TerrainType.Ruins: return Multiply(theme.purpleRed, 0.85f);
                case TerrainType.Plain:
                default: return plainAccent;
            }
        }

        private static Color Multiply(Color color, float factor) => new Color(color.r * factor, color.g * factor, color.b * factor, color.a);

        private void BuildHoverTile()
        {
            var mesh = BuildCombinedHexMesh(new List<Vector3> { Vector3.zero }, grid.HexSize * hoverScale, 0f);
            var color = theme != null ? theme.warmGold : new Color(0.831f, 0.627f, 0.09f);
            hoverGO = CreateTileObject("HexHoverTile", mesh, CreateFlatMaterial(color));
            hoverGO.SetActive(false);
        }

        private void UpdateHover()
        {
            if (targetCamera == null || grid == null || hoverGO == null) return;

            if (HexInputUtility.TryGetGroundPoint(targetCamera, out var point))
            {
                var coords = HexCoordinates.FromWorldPosition(point, grid.HexSize);
                if (grid.TryGetCell(coords, out _))
                {
                    var world = coords.ToWorldPosition(grid.HexSize);
                    hoverGO.transform.localPosition = new Vector3(world.x, HoverHeight, world.z);
                    hoverGO.SetActive(true);
                    return;
                }
            }

            hoverGO.SetActive(false);
        }

        private GameObject CreateLayer(string name, List<Vector3> centers, float size, float height, Color color)
        {
            if (centers == null || centers.Count == 0) return null;
            var mesh = BuildCombinedHexMesh(centers, size, height);
            return CreateTileObject(name, mesh, CreateFlatMaterial(color));
        }

        private GameObject CreateTileObject(string name, Mesh mesh, Material material)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(transform, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return go;
        }

        private static Material CreateFlatMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Standard");
            var material = new Material(shader) { name = "HexGridFlatColor" };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            return material;
        }

        private static Material CreateTexturedMaterial(Texture texture)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Texture")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Standard");
            var material = new Material(shader) { name = $"HexGridTerrainTexture_{texture.name}" };
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
            return material;
        }

        /// <summary>Fan-triangulated flat-top hexagons (angle offset 0°, matching HexCoordinates.ToWorldPosition) for every center, combined into one mesh so an entire tile layer is a single draw call.</summary>
        private static Mesh BuildCombinedHexMesh(List<Vector3> centers, float size, float y)
        {
            var vertices = new List<Vector3>(centers.Count * 6);
            var uvs = new List<Vector2>(centers.Count * 6);
            var triangles = new List<int>(centers.Count * 24);

            foreach (var center in centers)
            {
                AppendHex(vertices, uvs, triangles, center, size, y);
            }

            var mesh = new Mesh { name = "HexGridTiles" };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Each vertex's UV is its own position on the unit circle remapped to [0,1] — the same
        /// hex fan never samples the four corners of the UV square, so a source image only needs
        /// its hex illustration to reach at least this inscribed-circle footprint; any extra
        /// padding around the hex in the source image is simply never sampled.
        /// </summary>
        private static void AppendHex(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, Vector3 center, float size, float y)
        {
            int baseIndex = vertices.Count;
            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.Deg2Rad * (60f * i);
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);
                vertices.Add(new Vector3(center.x + size * cos, y, center.z + size * sin));
                uvs.Add(new Vector2(0.5f + cos * 0.5f, 0.5f + sin * 0.5f));
            }

            // Both winding orders are emitted for every triangle so the tile renders regardless
            // of which order ends up front-facing — there is no live Unity Editor in this
            // environment to verify winding visually, and the extra triangles are free at this
            // vertex count.
            for (int i = 1; i < 5; i++)
            {
                triangles.Add(baseIndex); triangles.Add(baseIndex + i); triangles.Add(baseIndex + i + 1);
                triangles.Add(baseIndex); triangles.Add(baseIndex + i + 1); triangles.Add(baseIndex + i);
            }
        }
    }
}
