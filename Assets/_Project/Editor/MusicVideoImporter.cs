using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using KingdomOfGod.Audio;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace KingdomOfGod.EditorTools
{
    /// <summary>
    /// Extracts the audio track from screen-recorded music videos (via ffmpeg, the only thing in
    /// this pipeline that can pull audio out of a video container — nothing in Unity does this)
    /// and assigns it to the matching MusicThemeData/LeitmotifData/AmbientSoundscapeData by exact
    /// name. Filenames like "musique Menu Principal.mp4" are expected to carry the target's
    /// displayName ("Menu Principal"), the same convention as VoiceNarrationImporter's verse
    /// references. Requires ffmpeg on PATH, or a path entered in the window.
    /// </summary>
    public class MusicVideoImporter : EditorWindow
    {
        private const string StagingFolder = "Assets/_Project/Audio/Music/_Import";
        private const string FinalFolder = "Assets/_Project/Audio/Music";
        private const string MusicFolder = "Assets/_Project/ScriptableObjects/Audio/Music";
        private const string LeitmotifsFolder = "Assets/_Project/ScriptableObjects/Audio/Leitmotifs";
        private const string AmbientFolder = "Assets/_Project/ScriptableObjects/Audio/Ambient";

        private static readonly string[] VideoExtensions = { ".mp4", ".mov", ".mkv", ".webm", ".avi" };

        private class Target
        {
            public ScriptableObject asset;
            public string displayName;
        }

        private class Row
        {
            public ScriptableObject target;
            public string displayName;
            public AudioClip clip;
            public bool skip;
        }

        private string sourceFolder = "";
        private string ffmpegPath = "ffmpeg";
        private readonly List<Row> rows = new List<Row>();
        private Vector2 scroll;

        [MenuItem("Kingdom of God/Setup/Import Music Videos")]
        public static void Open() => GetWindow<MusicVideoImporter>("Import Music");

        private void OnGUI()
        {
            GUILayout.Label("Import des musiques depuis des vidéos", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. Choisis le dossier des vidéos nommées \"musique <Thème>.mp4\" (ex. \"musique " +
                "Menu Principal.mp4\") et clique Extraire — l'audio est extrait via ffmpeg puis " +
                "associé au thème musical / leitmotiv / ambiance dont le nom correspond " +
                "exactement (accents ignorés).\n" +
                "2. VÉRIFIE la liste — écoute (▶) avant de valider ; les vidéos non reconnues " +
                "apparaissent quand même, à assigner à la main.\n" +
                "3. Clique Assigner pour écrire chaque clip dans le bon asset.",
                MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            sourceFolder = EditorGUILayout.TextField("Dossier source", sourceFolder);
            if (GUILayout.Button("Parcourir...", GUILayout.Width(90)))
            {
                var picked = EditorUtility.OpenFolderPanel("Dossier des vidéos", sourceFolder, "");
                if (!string.IsNullOrEmpty(picked)) sourceFolder = picked;
            }
            EditorGUILayout.EndHorizontal();

            ffmpegPath = EditorGUILayout.TextField(
                new GUIContent("Chemin ffmpeg", "\"ffmpeg\" si installé et dans le PATH, sinon le chemin complet vers l'exécutable."),
                ffmpegPath);

            if (GUILayout.Button("1. Extraire l'audio et associer"))
            {
                ExtractAndPair();
            }

            if (rows.Count == 0) return;

            int matchedCount = rows.Count(r => r.target != null && r.clip != null);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{matchedCount}/{rows.Count(r => r.target != null)} musiques trouvées — vérifie avant d'assigner :");
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(420));
            foreach (var row in rows) DrawRow(row);
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            if (GUILayout.Button("2. Assigner", GUILayout.Height(32)))
            {
                AssignClips();
            }
        }

        private void DrawRow(Row row)
        {
            EditorGUILayout.BeginHorizontal("box");

            bool keep = EditorGUILayout.ToggleLeft(GUIContent.none, !row.skip, GUILayout.Width(20));
            row.skip = !keep;

            EditorGUILayout.LabelField(row.displayName, GUILayout.Width(280));
            row.clip = (AudioClip)EditorGUILayout.ObjectField(row.clip, typeof(AudioClip), false, GUILayout.Width(220));

            GUI.enabled = row.clip != null;
            if (GUILayout.Button("▶", GUILayout.Width(28))) AudioPreview.Play(row.clip);
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Also folds "/" to a space — displayNames like "Ville / Jérusalem" can't appear
        /// verbatim in a filename (no OS allows "/" in a name), so a video named
        /// "musique Ville Jerusalem.mp4" must still compare equal to it.
        /// </summary>
        private static string NormalizeText(string s)
        {
            string decomposed = s.ToLowerInvariant().Replace("/", " ").Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            }
            return Regex.Replace(sb.ToString().Normalize(NormalizationForm.FormC), @"\s+", " ").Trim();
        }

        private static string ExtractNameFromFilename(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            name = Regex.Replace(name, @"^musique[\s_\-:]*", "", RegexOptions.IgnoreCase);
            return NormalizeText(name);
        }

        private bool CheckFfmpeg()
        {
            try
            {
                var psi = new ProcessStartInfo(ffmpegPath, "-version")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using var process = Process.Start(psi);
                process.WaitForExit(5000);
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private bool RunFfmpegExtract(string videoPath, string outputWavPath)
        {
            try
            {
                var psi = new ProcessStartInfo(ffmpegPath,
                    $"-y -i \"{videoPath}\" -vn -acodec pcm_s16le -ar 44100 -ac 2 \"{outputWavPath}\"")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using var process = Process.Start(psi);
                process.WaitForExit(120000);
                return process.ExitCode == 0 && File.Exists(outputWavPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Kingdom of God: erreur ffmpeg — {e.Message}");
                return false;
            }
        }

        private void AddTargets<T>(string folder, List<Target> targets) where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder });
            foreach (var guid in guids)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset == null) continue;

                var so = new SerializedObject(asset);
                string displayName = so.FindProperty("displayName")?.stringValue;
                if (string.IsNullOrEmpty(displayName)) continue;

                targets.Add(new Target { asset = asset, displayName = displayName });
            }
        }

        private List<Target> LoadTargets()
        {
            var targets = new List<Target>();
            AddTargets<MusicThemeData>(MusicFolder, targets);
            AddTargets<LeitmotifData>(LeitmotifsFolder, targets);
            AddTargets<AmbientSoundscapeData>(AmbientFolder, targets);
            return targets;
        }

        private void ExtractAndPair()
        {
            rows.Clear();

            if (string.IsNullOrEmpty(sourceFolder) || !Directory.Exists(sourceFolder))
            {
                Debug.LogError("Kingdom of God: dossier source introuvable.");
                return;
            }

            if (!CheckFfmpeg())
            {
                Debug.LogError("Kingdom of God: ffmpeg introuvable. Installe-le " +
                    "(https://ffmpeg.org/download.html) ou renseigne le chemin complet vers " +
                    "l'exécutable dans le champ \"Chemin ffmpeg\".");
                return;
            }

            if (!AssetDatabase.IsValidFolder(StagingFolder))
            {
                Directory.CreateDirectory(StagingFolder);
                AssetDatabase.Refresh();
            }

            var targets = LoadTargets();

            var videoFiles = Directory.GetFiles(sourceFolder)
                .Where(f => Path.GetFileNameWithoutExtension(f).StartsWith("musique", StringComparison.OrdinalIgnoreCase))
                .Where(f => VideoExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(f => f)
                .ToList();

            // Imported once per distinct normalized name, then looked up per target below — so a
            // name shared by two different targets (e.g. "Bataille" is both a MusicThemeData and
            // an AmbientSoundscapeData) suggests the same clip for both rows instead of silently
            // picking one and leaving the other unmatched.
            var clipsByName = new Dictionary<string, AudioClip>();

            for (int i = 0; i < videoFiles.Count; i++)
            {
                string videoPath = videoFiles[i];
                EditorUtility.DisplayProgressBar("Extraction audio", Path.GetFileName(videoPath), (float)i / Mathf.Max(1, videoFiles.Count));

                string wavName = $"{Path.GetFileNameWithoutExtension(videoPath)}.wav";
                string destPath = AssetDatabase.GenerateUniqueAssetPath($"{StagingFolder}/{wavName}");
                string absoluteDestPath = Path.GetFullPath(destPath);

                if (!RunFfmpegExtract(videoPath, absoluteDestPath))
                {
                    Debug.LogWarning($"Kingdom of God: extraction audio échouée pour {videoPath}.");
                    continue;
                }

                AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceSynchronousImport);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(destPath);
                if (clip == null) continue;

                clipsByName[ExtractNameFromFilename(Path.GetFileName(videoPath))] = clip;
            }

            EditorUtility.ClearProgressBar();

            var usedNames = new HashSet<string>();
            foreach (var target in targets)
            {
                string key = NormalizeText(target.displayName);
                AudioClip clip = clipsByName.TryGetValue(key, out var found) ? found : null;
                if (clip != null) usedNames.Add(key);
                rows.Add(new Row { target = target.asset, displayName = target.displayName, clip = clip });
            }

            foreach (var kvp in clipsByName)
            {
                if (!usedNames.Contains(kvp.Key))
                {
                    rows.Add(new Row { target = null, displayName = $"(non reconnu : {kvp.Key})", clip = kvp.Value });
                }
            }

            int matched = rows.Count(r => r.target != null && r.clip != null);
            Debug.Log($"Kingdom of God: {matched}/{targets.Count} musiques associées à partir de {videoFiles.Count} vidéo(s).");
        }

        private void AssignClips()
        {
            if (!AssetDatabase.IsValidFolder(FinalFolder))
            {
                Directory.CreateDirectory(FinalFolder);
                AssetDatabase.Refresh();
            }

            // A clip can be the suggestion for more than one row (e.g. "Bataille" exists as both
            // a MusicThemeData and an AmbientSoundscapeData) — move/rename each distinct asset
            // only once, named after whichever row claims it first.
            var moved = new HashSet<AudioClip>();
            int assigned = 0;
            foreach (var row in rows)
            {
                if (row.skip || row.target == null || row.clip == null) continue;

                if (!moved.Contains(row.clip))
                {
                    string currentPath = AssetDatabase.GetAssetPath(row.clip);
                    string ext = Path.GetExtension(currentPath);
                    string safeName = Regex.Replace(NormalizeText(row.displayName), @"[^a-z0-9]+", "");
                    string finalPath = AssetDatabase.GenerateUniqueAssetPath($"{FinalFolder}/Music_{safeName}{ext}");

                    string moveError = AssetDatabase.MoveAsset(currentPath, finalPath);
                    if (!string.IsNullOrEmpty(moveError))
                    {
                        Debug.LogWarning($"Kingdom of God: déplacement/renommage de {currentPath} échoué ({moveError}).");
                        continue;
                    }
                    moved.Add(row.clip);
                }

                var so = new SerializedObject(row.target);
                so.FindProperty("clip").objectReferenceValue = row.clip;
                so.ApplyModifiedProperties();
                assigned++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Kingdom of God: {assigned} musiques assignées.");
        }
    }
}
