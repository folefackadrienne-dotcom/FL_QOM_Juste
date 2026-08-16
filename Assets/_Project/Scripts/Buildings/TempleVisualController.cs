using KingdomOfGod.Core;
using KingdomOfGod.Grid;
using KingdomOfGod.UI;
using TMPro;
using UnityEngine;

namespace KingdomOfGod.Buildings
{
    /// <summary>
    /// TempleSystem was pure data/logic — CurrentLevel, TryUpgrade, LevelUpgraded — with no world
    /// position and nothing that ever instantiated TempleLevelData.prefab (always null; no Temple
    /// art has been produced). Fixed at the map center (0,0), the one cell TerrainGenerator already
    /// guarantees is never Mountain (it's the hardcoded CapturePoint objective for
    /// Mission_Age5_03_DavidRoiAHebronPuisJerusalem) — reserved here via HexCell.IsReserved so
    /// BuildingManager can never let the player build over it. A code-generated placeholder per
    /// docs/ArtDirection.md ("imposant, lumineux, couvert d'or et de motifs sacrés"): a golden body
    /// that grows taller with each of the 5 levels, topped by a rotated cube "capstone" in
    /// UITheme.divineLight standing in for the "lumineux" gold-and-sacred-motifs finish.
    /// </summary>
    public class TempleVisualController : MonoBehaviour
    {
        private static readonly HexCoordinates TemplePosition = new HexCoordinates(0, 0);

        [SerializeField] private TempleSystem templeSystem;
        [SerializeField] private HexGrid hexGrid;
        [SerializeField] private UIThemeData theme;

        private GameObject visualRoot;

        private void Awake()
        {
            // templeSystem/hexGrid live on the persistent Bootstrap GameManager, in a different
            // scene from this controller — Inspector references can't cross scenes, so fall back
            // to the running singleton when these fields were left unassigned.
            if (templeSystem == null && GameManager.Instance != null)
            {
                templeSystem = GameManager.Instance.Temple;
            }
            if (hexGrid == null && GameManager.Instance != null)
            {
                hexGrid = GameManager.Instance.Grid;
            }
        }

        private void Start()
        {
            ReserveTempleCell();
            BuildVisual();
        }

        private void OnEnable()
        {
            if (templeSystem != null) templeSystem.LevelUpgraded += OnLevelUpgraded;
        }

        private void OnDisable()
        {
            if (templeSystem != null) templeSystem.LevelUpgraded -= OnLevelUpgraded;
        }

        private void OnLevelUpgraded(int level) => BuildVisual();

        private void ReserveTempleCell()
        {
            if (hexGrid != null && hexGrid.TryGetCell(TemplePosition, out var cell))
            {
                cell.IsReserved = true;
            }
        }

        private void BuildVisual()
        {
            if (visualRoot != null) Destroy(visualRoot);
            if (templeSystem == null || hexGrid == null) return;

            int level = Mathf.Clamp(templeSystem.CurrentLevel, 1, 5);
            var worldPosition = TemplePosition.ToWorldPosition(hexGrid.HexSize);

            visualRoot = new GameObject($"Temple (Niveau {level})");
            visualRoot.transform.SetParent(transform, false);
            visualRoot.transform.position = worldPosition;

            float bodyHeight = 1.5f + level * 1.0f;
            float footprint = hexGrid.HexSize * 1.3f;

            var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.transform.SetParent(visualRoot.transform, false);
            body.transform.localPosition = new Vector3(0f, bodyHeight * 0.5f, 0f);
            body.transform.localScale = new Vector3(footprint, bodyHeight * 0.5f, footprint);
            StripCollider(body);

            var bodyRenderer = body.GetComponent<MeshRenderer>();
            var bodyColor = theme != null
                ? Color.Lerp(theme.ochre, theme.warmGold, (level - 1) / 4f)
                : Color.Lerp(new Color(0.800f, 0.467f, 0.133f), new Color(0.831f, 0.627f, 0.090f), (level - 1) / 4f);
            bodyRenderer.sharedMaterial = CreateFlatMaterial(bodyColor);
            bodyRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            bodyRenderer.receiveShadows = false;

            var capstone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            capstone.transform.SetParent(visualRoot.transform, false);
            capstone.transform.localPosition = new Vector3(0f, bodyHeight + 0.4f, 0f);
            capstone.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            capstone.transform.localScale = Vector3.one * (0.5f + level * 0.15f);
            StripCollider(capstone);

            var capstoneRenderer = capstone.GetComponent<MeshRenderer>();
            capstoneRenderer.sharedMaterial = CreateFlatMaterial(theme != null ? theme.divineLight : new Color(0.957f, 0.769f, 0.188f));
            capstoneRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            capstoneRenderer.receiveShadows = false;

            var labelGO = new GameObject("NameLabel");
            labelGO.transform.SetParent(visualRoot.transform, false);
            labelGO.transform.localPosition = new Vector3(0f, bodyHeight + 1.3f, 0f);
            labelGO.transform.localScale = Vector3.one * 0.18f;
            var label = labelGO.AddComponent<TextMeshPro>();
            label.text = $"Temple — Niveau {level}";
            label.fontSize = 4f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = theme != null ? theme.panelText : new Color(0.2f, 0.133f, 0.078f);
            labelGO.AddComponent<BillboardLabel>();
        }

        private static void StripCollider(GameObject go)
        {
            var collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
        }

        private static Material CreateFlatMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Standard");
            var material = new Material(shader) { name = "TemplePlaceholderFlatColor" };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            return material;
        }
    }
}
