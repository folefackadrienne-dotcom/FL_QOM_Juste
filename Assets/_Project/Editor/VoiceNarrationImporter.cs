using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using KingdomOfGod.Verses;
using UnityEditor;
using UnityEngine;

namespace KingdomOfGod.EditorTools
{
    /// <summary>
    /// Bulk-imports verse narration recordings named "audio &lt;Reference&gt;" (e.g.
    /// "audio Genese 12,2-3.mp3") and assigns them to VerseData.narrationClip* by matching the
    /// reference extracted from the filename against VerseData.reference (accent- and
    /// comma/colon-insensitive), rather than guessing from file order. Still a two-step flow —
    /// import first (so each row has a real, previewable AudioClip), review/override/skip as
    /// needed, THEN assign — since a wrong pairing here means mislabeling actual scripture.
    /// Assigned clips are renamed from "audio Genese 12,2-3" to
    /// "Narration_Genese12-2-3_French" so the project stays readable afterward.
    /// </summary>
    public class VoiceNarrationImporter : EditorWindow
    {
        private const string StagingFolder = "Assets/_Project/Audio/Voice/_Import";
        private const string FinalFolder = "Assets/_Project/Audio/Voice";
        private const string VersesFolder = "Assets/_Project/ScriptableObjects/Verses";

        private enum NarrationLanguage { French, English, Hebrew }

        private class Row
        {
            public VerseData verse;
            public AudioClip clip;
            public bool skip;
        }

        private string sourceFolder = "";
        private NarrationLanguage language = NarrationLanguage.French;
        private readonly List<Row> rows = new List<Row>();
        private int verseCount;
        private Vector2 scroll;

        [MenuItem("Kingdom of God/Setup/Import Voice Narrations")]
        public static void Open() => GetWindow<VoiceNarrationImporter>("Import Narrations");

        private void OnGUI()
        {
            GUILayout.Label("Import des narrations de versets", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. Choisis le dossier où sont tes fichiers \"audio <Référence>.mp3/.wav\" (ex. " +
                "\"audio Genese 12,2-3.mp3\") et clique Importer — chaque fichier est associé au " +
                "verset dont la référence correspond (accents/virgule-deux-points ignorés).\n" +
                "2. VÉRIFIE la liste — les lignes sans clip n'ont trouvé aucune correspondance, et " +
                "les fichiers non reconnus apparaissent en bas ; glisse-les à la main sur le bon " +
                "verset si besoin, écoute avec ▶ avant de valider.\n" +
                "3. Clique Assigner pour écrire chaque clip dans le bon VerseData et le renommer.",
                MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            sourceFolder = EditorGUILayout.TextField("Dossier source", sourceFolder);
            if (GUILayout.Button("Parcourir...", GUILayout.Width(90)))
            {
                var picked = EditorUtility.OpenFolderPanel("Dossier des narrations", sourceFolder, "");
                if (!string.IsNullOrEmpty(picked)) sourceFolder = picked;
            }
            EditorGUILayout.EndHorizontal();

            language = (NarrationLanguage)EditorGUILayout.EnumPopup("Langue", language);

            if (GUILayout.Button("1. Importer et associer aux versets"))
            {
                ImportAndPair();
            }

            if (rows.Count == 0) return;

            int matchedCount = rows.Count(r => r.verse != null && r.clip != null);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{matchedCount}/{verseCount} versets appariés automatiquement — vérifie avant d'assigner :");
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(420));
            for (int i = 0; i < rows.Count; i++)
            {
                DrawRow(rows[i]);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            if (GUILayout.Button("2. Assigner et renommer", GUILayout.Height(32)))
            {
                AssignAndRename();
            }
        }

        private void DrawRow(Row row)
        {
            EditorGUILayout.BeginHorizontal("box");

            bool keep = EditorGUILayout.ToggleLeft(GUIContent.none, !row.skip, GUILayout.Width(20));
            row.skip = !keep;

            string label = row.verse != null ? row.verse.reference : "(fichier non reconnu — glisse-le sur le bon verset ci-dessus)";
            EditorGUILayout.LabelField(label, GUILayout.Width(320));
            row.clip = (AudioClip)EditorGUILayout.ObjectField(row.clip, typeof(AudioClip), false, GUILayout.Width(220));

            GUI.enabled = row.clip != null;
            if (GUILayout.Button("▶", GUILayout.Width(28))) AudioPreview.Play(row.clip);
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>Extracts the reference from a filename like "audio Genese 12,2-3.mp3" -> "genese 12:2-3".</summary>
        private static string ExtractReferenceFromFilename(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            name = Regex.Replace(name, @"^audio[\s_\-]*", "", RegexOptions.IgnoreCase);
            return NormalizeReference(name);
        }

        /// <summary>Lowercases, strips accents, and folds ","/":" so "Genèse 12:2-3" and "Genese 12,2-3" compare equal.</summary>
        private static string NormalizeReference(string s)
        {
            string decomposed = s.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            }
            string result = sb.ToString().Normalize(NormalizationForm.FormC).Replace(",", ":").Replace("_", " ");
            return Regex.Replace(result, @"\s+", " ").Trim();
        }

        private void ImportAndPair()
        {
            rows.Clear();

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
                .Where(f => Path.GetFileNameWithoutExtension(f).StartsWith("audio", StringComparison.OrdinalIgnoreCase))
                .Where(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f)
                .ToList();

            var imported = new List<(string normalizedReference, AudioClip clip)>();
            foreach (var sourcePath in sourceFiles)
            {
                string destPath = AssetDatabase.GenerateUniqueAssetPath($"{StagingFolder}/{Path.GetFileName(sourcePath)}");
                File.Copy(sourcePath, destPath, overwrite: false);
                AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceSynchronousImport);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(destPath);
                if (clip == null) continue;

                imported.Add((ExtractReferenceFromFilename(Path.GetFileName(sourcePath)), clip));
            }

            var verseGuids = AssetDatabase.FindAssets("t:VerseData", new[] { VersesFolder });
            var verses = verseGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => (int)AssetDatabase.LoadAssetAtPath<VerseData>(path).age)
                .ThenBy(path => path)
                .Select(AssetDatabase.LoadAssetAtPath<VerseData>)
                .ToList();
            verseCount = verses.Count;

            var unclaimed = new List<(string normalizedReference, AudioClip clip)>(imported);
            foreach (var verse in verses)
            {
                string normalizedVerseRef = NormalizeReference(verse.reference);
                int matchIndex = unclaimed.FindIndex(c => c.normalizedReference == normalizedVerseRef);
                AudioClip matchedClip = null;
                if (matchIndex >= 0)
                {
                    matchedClip = unclaimed[matchIndex].clip;
                    unclaimed.RemoveAt(matchIndex);
                }
                rows.Add(new Row { verse = verse, clip = matchedClip });
            }

            foreach (var leftover in unclaimed)
            {
                rows.Add(new Row { verse = null, clip = leftover.clip });
            }

            int matched = rows.Count(r => r.verse != null && r.clip != null);
            Debug.Log($"Kingdom of God: {matched}/{verses.Count} versets appariés automatiquement par nom de fichier" +
                (unclaimed.Count > 0 ? $", {unclaimed.Count} fichier(s) non reconnu(s) — à assigner à la main." : "."));
        }

        private void AssignAndRename()
        {
            if (!AssetDatabase.IsValidFolder(FinalFolder))
            {
                Directory.CreateDirectory(FinalFolder);
                AssetDatabase.Refresh();
            }

            string fieldName = language switch
            {
                NarrationLanguage.English => "narrationClipEnglish",
                NarrationLanguage.Hebrew => "narrationClipHebrew",
                _ => "narrationClipFrench"
            };

            int assigned = 0;
            foreach (var row in rows)
            {
                if (row.skip || row.verse == null || row.clip == null) continue;

                string safeReference = row.verse.reference.Replace(" ", "").Replace(":", "-").Replace(",", "");
                string currentPath = AssetDatabase.GetAssetPath(row.clip);
                string ext = Path.GetExtension(currentPath);
                string finalPath = AssetDatabase.GenerateUniqueAssetPath($"{FinalFolder}/Narration_{safeReference}_{language}{ext}");

                string moveError = AssetDatabase.MoveAsset(currentPath, finalPath);
                if (!string.IsNullOrEmpty(moveError))
                {
                    Debug.LogWarning($"Kingdom of God: déplacement/renommage de {currentPath} échoué ({moveError}) — verset ignoré.");
                    continue;
                }

                var so = new SerializedObject(row.verse);
                so.FindProperty(fieldName).objectReferenceValue = row.clip;
                so.ApplyModifiedProperties();
                assigned++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Kingdom of God: {assigned} narrations assignées et renommées.");
        }
    }

    /// <summary>Editor-only audio preview via UnityEditor's internal AudioUtil — the standard, long-stable reflection trick since Unity has never exposed a public preview API.</summary>
    internal static class AudioPreview
    {
        private static readonly MethodInfo PlayMethod;

        static AudioPreview()
        {
            var audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
            PlayMethod = audioUtilType?.GetMethod(
                "PlayPreviewClip",
                BindingFlags.Static | BindingFlags.Public,
                null,
                new[] { typeof(AudioClip), typeof(int), typeof(bool) },
                null);
        }

        public static void Play(AudioClip clip)
        {
            if (clip == null || PlayMethod == null) return;
            PlayMethod.Invoke(null, new object[] { clip, 0, false });
        }
    }
}
