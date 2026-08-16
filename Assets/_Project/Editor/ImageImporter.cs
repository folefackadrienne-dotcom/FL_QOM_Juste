using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace KingdomOfGod.EditorTools
{
    /// <summary>
    /// Bulk-imports illustration files named "image &lt;Nom&gt;" (e.g. "image Moïse.png") as
    /// Sprites and suggests assignments across every content type that has a Sprite field —
    /// LeaderData.portrait, AntagonistData.portrait, UnitData.icon, BuildingData.icon,
    /// MiracleData.icon, ArtifactData.icon, ProductData.icon — by matching the extracted name
    /// against each asset's displayName (accent/case-insensitive, whole-word substring allowed
    /// either direction). Deliberately many-to-many like SfxImporter: the same imported image can
    /// be suggested — and assigned — to every asset whose name matches, since one "Moïse"
    /// illustration can legitimately serve both a LeaderData portrait and, say, an ArtifactData
    /// icon named after him. Targets are driven by AssetDatabase.FindAssets("t:&lt;TypeName&gt;")
    /// and SerializedObject.FindProperty by field name rather than per-type code, so a future
    /// content type with a Sprite field just needs an entry in TargetTypes below.
    /// </summary>
    public class ImageImporter : EditorWindow
    {
        private const string StagingFolder = "Assets/_Project/Art/_Import";
        private const string FinalFolder = "Assets/_Project/Art/Sprites";

        private struct TargetType
        {
            public string typeName;
            public string folder;
            public string spriteField;
            public string label;
        }

        private static readonly TargetType[] TargetTypes =
        {
            new TargetType { typeName = "LeaderData", folder = "Assets/_Project/ScriptableObjects/Leaders", spriteField = "portrait", label = "Leaders" },
            new TargetType { typeName = "AntagonistData", folder = "Assets/_Project/ScriptableObjects/Antagonists", spriteField = "portrait", label = "Antagonistes" },
            new TargetType { typeName = "UnitData", folder = "Assets/_Project/ScriptableObjects/Units", spriteField = "icon", label = "Unités" },
            new TargetType { typeName = "BuildingData", folder = "Assets/_Project/ScriptableObjects/Buildings", spriteField = "icon", label = "Bâtiments" },
            new TargetType { typeName = "MiracleData", folder = "Assets/_Project/ScriptableObjects/Miracles", spriteField = "icon", label = "Miracles" },
            new TargetType { typeName = "ArtifactData", folder = "Assets/_Project/ScriptableObjects/Artifacts", spriteField = "icon", label = "Artefacts" },
            new TargetType { typeName = "ProductData", folder = "Assets/_Project/ScriptableObjects/Monetization", spriteField = "icon", label = "Produits" },
        };

        private class ImportedImage
        {
            public Sprite sprite;
            public string normalizedName;
            public string rawName;
        }

        private class Row
        {
            public UnityEngine.Object asset;
            public string spriteField;
            public string typeLabel;
            public string displayName;
            public Sprite sprite;
            public bool skip;
        }

        private string sourceFolder = "";
        private readonly List<ImportedImage> importedImages = new List<ImportedImage>();
        private readonly List<Row> rows = new List<Row>();
        private Vector2 scroll;

        [MenuItem("Kingdom of God/Setup/Import Images")]
        public static void Open() => GetWindow<ImageImporter>("Import Images");

        private void OnGUI()
        {
            GUILayout.Label("Import des illustrations", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. Choisis le dossier où sont tes fichiers \"image <Nom>.png/.jpg\" (ex. " +
                "\"image Moïse.png\") et clique Importer — chaque fiche ayant un champ " +
                "portrait/icône (Leaders, Antagonistes, Unités, Bâtiments, Miracles, Artefacts, " +
                "Produits) reçoit une suggestion si son nom correspond. La même image peut être " +
                "suggérée — et assignée — à plusieurs fiches si son nom correspond à plusieurs " +
                "endroits.\n" +
                "2. VÉRIFIE la liste — les lignes sans image n'ont trouvé aucune correspondance ; " +
                "glisse une image dessus à la main si besoin, ou décoche pour ignorer.\n" +
                "3. Clique Assigner pour écrire chaque sprite dans le bon champ.",
                MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            sourceFolder = EditorGUILayout.TextField("Dossier source", sourceFolder);
            if (GUILayout.Button("Parcourir...", GUILayout.Width(90)))
            {
                var picked = EditorUtility.OpenFolderPanel("Dossier des images", sourceFolder, "");
                if (!string.IsNullOrEmpty(picked)) sourceFolder = picked;
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("1. Importer et suggérer"))
            {
                ImportAndSuggest();
            }

            if (rows.Count == 0) return;

            int matchedCount = rows.Count(r => r.sprite != null);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{matchedCount}/{rows.Count} fiches avec une suggestion — vérifie avant d'assigner :");
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(440));
            string lastLabel = null;
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].typeLabel != lastLabel)
                {
                    lastLabel = rows[i].typeLabel;
                    EditorGUILayout.Space(4);
                    EditorGUILayout.LabelField(lastLabel, EditorStyles.boldLabel);
                }
                DrawRow(rows[i]);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            if (GUILayout.Button("2. Assigner", GUILayout.Height(32)))
            {
                AssignSprites();
            }
        }

        private void DrawRow(Row row)
        {
            EditorGUILayout.BeginHorizontal("box");

            bool keep = EditorGUILayout.ToggleLeft(GUIContent.none, !row.skip, GUILayout.Width(20));
            row.skip = !keep;

            EditorGUILayout.LabelField(row.displayName, GUILayout.Width(260));
            row.sprite = (Sprite)EditorGUILayout.ObjectField(row.sprite, typeof(Sprite), false, GUILayout.Width(180));

            EditorGUILayout.EndHorizontal();
        }

        private static string NormalizeText(string s)
        {
            string decomposed = s.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            }
            string result = sb.ToString().Normalize(NormalizationForm.FormC);
            return Regex.Replace(result, @"\s+", " ").Trim();
        }

        private static string ExtractNameFromFilename(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            return Regex.Replace(name, @"^image[\s_\-:]*", "", RegexOptions.IgnoreCase).Trim();
        }

        private static string CleanFileName(string name)
        {
            var words = Regex.Split(NormalizeText(name), @"[^a-z0-9]+").Where(w => w.Length > 0);
            return string.Join("", words.Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1)));
        }

        /// <summary>Exact match, or one name containing the other as a whole word — same forgiving logic as VoiceNarrationImporter's reference matching, since a filename can legitimately be a superset ("Moïse le Législateur") or subset of a displayName.</summary>
        private static bool NamesMatch(string a, string b)
        {
            if (a.Length == 0 || b.Length == 0) return false;
            if (a == b) return true;
            return Regex.IsMatch(a, $@"(^|\s){Regex.Escape(b)}(\s|$)") ||
                   Regex.IsMatch(b, $@"(^|\s){Regex.Escape(a)}(\s|$)");
        }

        private void ImportAndSuggest()
        {
            rows.Clear();
            importedImages.Clear();

            if (string.IsNullOrEmpty(sourceFolder) || !Directory.Exists(sourceFolder))
            {
                Debug.LogError("Kingdom of God: dossier source introuvable.");
                return;
            }

            if (!AssetDatabase.IsValidFolder(StagingFolder))
            {
                Directory.CreateDirectory(StagingFolder);
                AssetDatabase.Refresh();
            }

            var sourceFiles = Directory.GetFiles(sourceFolder)
                .Where(f => Path.GetFileNameWithoutExtension(f).StartsWith("image", StringComparison.OrdinalIgnoreCase))
                .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                    || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                    || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f)
                .ToList();

            foreach (var sourcePath in sourceFiles)
            {
                string destPath = AssetDatabase.GenerateUniqueAssetPath($"{StagingFolder}/{Path.GetFileName(sourcePath)}");
                File.Copy(sourcePath, destPath, overwrite: false);
                AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceSynchronousImport);

                // Dropped image files default to TextureImporterType.Default — force Sprite so
                // AssetDatabase.LoadAssetAtPath<Sprite> below (and every portrait/icon field this
                // tool writes into) actually resolves instead of silently staying null.
                if (AssetImporter.GetAtPath(destPath) is TextureImporter importer)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.SaveAndReimport();
                }

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(destPath);
                if (sprite == null) continue;

                string rawName = ExtractNameFromFilename(Path.GetFileName(sourcePath));
                importedImages.Add(new ImportedImage { sprite = sprite, rawName = rawName, normalizedName = NormalizeText(rawName) });
            }

            var used = new HashSet<Sprite>();
            foreach (var target in TargetTypes)
            {
                var guids = AssetDatabase.FindAssets($"t:{target.typeName}", new[] { target.folder });
                var assets = guids
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .OrderBy(p => p)
                    .Select(AssetDatabase.LoadMainAssetAtPath)
                    .Where(a => a != null)
                    .ToList();

                foreach (var asset in assets)
                {
                    var so = new SerializedObject(asset);
                    var nameProp = so.FindProperty("displayName");
                    var spriteProp = so.FindProperty(target.spriteField);
                    if (nameProp == null || spriteProp == null) continue;

                    string displayName = nameProp.stringValue;
                    string normalizedDisplayName = NormalizeText(displayName);

                    Sprite match = spriteProp.objectReferenceValue as Sprite;
                    if (match == null)
                    {
                        foreach (var image in importedImages)
                        {
                            if (NamesMatch(image.normalizedName, normalizedDisplayName))
                            {
                                match = image.sprite;
                                break;
                            }
                        }
                    }

                    if (match != null) used.Add(match);
                    rows.Add(new Row
                    {
                        asset = asset,
                        spriteField = target.spriteField,
                        typeLabel = target.label,
                        displayName = displayName,
                        sprite = match
                    });
                }
            }

            int unused = importedImages.Count(i => !used.Contains(i.sprite));
            Debug.Log($"Kingdom of God: {rows.Count(r => r.sprite != null)}/{rows.Count} fiches ont une suggestion " +
                $"({importedImages.Count} image(s) importée(s), {unused} non utilisée(s) — assignables à la main).");
        }

        private void AssignSprites()
        {
            if (!AssetDatabase.IsValidFolder(FinalFolder))
            {
                Directory.CreateDirectory(FinalFolder);
                AssetDatabase.Refresh();
            }

            var moved = new HashSet<Sprite>();
            int assigned = 0;
            foreach (var row in rows)
            {
                if (row.skip || row.asset == null || row.sprite == null) continue;

                string currentPath = AssetDatabase.GetAssetPath(row.sprite);
                if (!moved.Contains(row.sprite) && currentPath.StartsWith(StagingFolder))
                {
                    string ext = Path.GetExtension(currentPath);
                    string rawName = ExtractNameFromFilename(Path.GetFileName(currentPath));
                    string finalPath = AssetDatabase.GenerateUniqueAssetPath($"{FinalFolder}/Image_{CleanFileName(rawName)}{ext}");

                    string moveError = AssetDatabase.MoveAsset(currentPath, finalPath);
                    if (!string.IsNullOrEmpty(moveError))
                    {
                        Debug.LogWarning($"Kingdom of God: déplacement/renommage de {currentPath} échoué ({moveError}).");
                    }
                    moved.Add(row.sprite);
                }

                var so = new SerializedObject(row.asset);
                so.FindProperty(row.spriteField).objectReferenceValue = row.sprite;
                so.ApplyModifiedProperties();
                assigned++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Kingdom of God: {assigned} sprite(s) assigné(s) à partir de {moved.Count} fichier(s) distinct(s) (réutilisation incluse).");
        }
    }
}
