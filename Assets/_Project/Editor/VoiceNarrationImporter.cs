using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using KingdomOfGod.Verses;
using UnityEditor;
using UnityEngine;

namespace KingdomOfGod.EditorTools
{
    /// <summary>
    /// Bulk-imports verse narration recordings whose filenames carry no identifying information
    /// (e.g. ElevenLabs' default "audio", "audio (1)", "audio (2)"... exports) and assigns them to
    /// VerseData.narrationClip*. Files are paired against the 34 VerseData assets in file-creation
    /// order, on the assumption they were recorded/downloaded verse-by-verse in that order — a
    /// best guess, not a guarantee, hence the two-step flow: import first (so each row has a real,
    /// previewable AudioClip), reorder/skip as needed, THEN assign. Assigned clips are renamed from
    /// "audio (7)" to "Narration_<Reference>_<Language>" so the project stays readable afterward.
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
        private Vector2 scroll;

        [MenuItem("Kingdom of God/Setup/Import Voice Narrations")]
        public static void Open() => GetWindow<VoiceNarrationImporter>("Import Narrations");

        private void OnGUI()
        {
            GUILayout.Label("Import des narrations de versets", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. Choisis le dossier où sont tes fichiers \"audio*.mp3/.wav\" et clique Importer — " +
                "ils sont copiés dans le projet et associés aux 34 versets par ordre de date de " +
                "création.\n" +
                "2. VÉRIFIE et corrige l'ordre avec ▲▼/▶ avant de continuer — c'est une estimation, " +
                "pas une garantie que ça correspond à ce que tu as réellement enregistré.\n" +
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

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{rows.Count} lignes — vérifie l'ordre avant d'assigner :");
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(420));
            for (int i = 0; i < rows.Count; i++)
            {
                DrawRow(rows[i], i);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            if (GUILayout.Button("2. Assigner et renommer", GUILayout.Height(32)))
            {
                AssignAndRename();
            }
        }

        private void DrawRow(Row row, int index)
        {
            EditorGUILayout.BeginHorizontal("box");

            bool keep = EditorGUILayout.ToggleLeft(GUIContent.none, !row.skip, GUILayout.Width(20));
            row.skip = !keep;

            EditorGUILayout.LabelField(row.verse != null ? row.verse.reference : "(pas de verset)", GUILayout.Width(160));
            row.clip = (AudioClip)EditorGUILayout.ObjectField(row.clip, typeof(AudioClip), false, GUILayout.Width(220));

            GUI.enabled = index > 0;
            if (GUILayout.Button("▲", GUILayout.Width(24))) Swap(index, index - 1);
            GUI.enabled = index < rows.Count - 1;
            if (GUILayout.Button("▼", GUILayout.Width(24))) Swap(index, index + 1);
            GUI.enabled = row.clip != null;
            if (GUILayout.Button("▶", GUILayout.Width(28))) AudioPreview.Play(row.clip);
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        private void Swap(int a, int b) => (rows[a], rows[b]) = (rows[b], rows[a]);

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
                .OrderBy(File.GetLastWriteTimeUtc)
                .ToList();

            var clips = new List<AudioClip>();
            foreach (var sourcePath in sourceFiles)
            {
                string destPath = AssetDatabase.GenerateUniqueAssetPath($"{StagingFolder}/{Path.GetFileName(sourcePath)}");
                File.Copy(sourcePath, destPath, overwrite: false);
                AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceSynchronousImport);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(destPath);
                if (clip != null) clips.Add(clip);
            }

            var verseGuids = AssetDatabase.FindAssets("t:VerseData", new[] { VersesFolder });
            var verses = verseGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => (int)AssetDatabase.LoadAssetAtPath<VerseData>(path).age)
                .ThenBy(path => path)
                .Select(AssetDatabase.LoadAssetAtPath<VerseData>)
                .ToList();

            int count = Mathf.Max(clips.Count, verses.Count);
            for (int i = 0; i < count; i++)
            {
                rows.Add(new Row
                {
                    verse = i < verses.Count ? verses[i] : null,
                    clip = i < clips.Count ? clips[i] : null
                });
            }

            if (clips.Count != verses.Count)
            {
                Debug.LogWarning($"Kingdom of God: {clips.Count} fichiers importés pour {verses.Count} versets — " +
                    "vérifie les lignes en trop/manquantes avant d'assigner.");
            }
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
