using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using KingdomOfGod.Audio;
using UnityEditor;
using UnityEngine;

namespace KingdomOfGod.EditorTools
{
    /// <summary>
    /// Bulk-imports sound effect recordings named "sound effects - &lt;description&gt;" (e.g.
    /// "sound effects - male cry.mp3") and suggests a clip for each of the 44 SfxCueData by
    /// keyword overlap between the file's description and a hand-authored keyword list per SFX
    /// (the same vocabulary used in the Epidemic Sound / ElevenLabs search-term guides earlier in
    /// this project). Unlike VoiceNarrationImporter's 1:1 verse matching, this is deliberately
    /// many-to-many: the same imported clip can be suggested — and assigned — to every SfxCueData
    /// whose keywords it matches, since one "war cry" recording can legitimately serve as both
    /// "Bataille - Cri de Guerre" and, say, "Antagonistes - Entrée en Scène du Boss". Matching is
    /// a heuristic, not a guarantee — always review/preview/override before assigning.
    /// </summary>
    public class SfxImporter : EditorWindow
    {
        private const string StagingFolder = "Assets/_Project/Audio/Sfx/_Import";
        private const string FinalFolder = "Assets/_Project/Audio/Sfx";
        private const string SfxFolder = "Assets/_Project/ScriptableObjects/Audio/Sfx";

        private class Row
        {
            public SfxCueData sfx;
            public AudioClip clip;
            public bool skip;
        }

        private class ImportedClip
        {
            public AudioClip clip;
            public HashSet<string> keywords;
        }

        private string sourceFolder = "";
        private readonly List<Row> rows = new List<Row>();
        private readonly List<ImportedClip> importedClips = new List<ImportedClip>();
        private Vector2 scroll;

        [MenuItem("Kingdom of God/Setup/Import Sound Effects")]
        public static void Open() => GetWindow<SfxImporter>("Import SFX");

        // Keyword vocabulary per SfxCueData.displayName — same phrasing as the Epidemic
        // Sound / ElevenLabs search-term guides, since that's the vocabulary a file is likely
        // to have been named/found with.
        private static readonly Dictionary<string, string[]> Keywords = new Dictionary<string, string[]>
        {
            ["Interface - Clic sur Parchemin"] = new[] { "parchment touch", "paper touch", "click", "tap" },
            ["Interface - Erreur / Action Impossible"] = new[] { "error", "deny", "denied", "buzz", "ui fail" },
            ["Interface - Fermeture de Menu"] = new[] { "scroll close", "paper roll close", "menu close" },
            ["Interface - Ouverture de Menu"] = new[] { "scroll open", "paper unroll", "unroll", "menu open" },
            ["Interface - Validation Positive"] = new[] { "confirm chime", "positive chime", "bell confirm" },
            ["Construction - Placement d'un Bâtiment"] = new[] { "construction", "stone build", "wood hammer", "build tools" },
            ["Construction - Bâtiment Important Terminé"] = new[] { "soft fanfare", "achievement chime", "building complete" },
            ["Bâtiments - Temple Amélioré"] = new[] { "upgrade fanfare", "solemn rising chime", "temple upgrade" },
            ["Bataille - Cri de Guerre"] = new[] { "war cry", "male cry", "battle shout", "battle cry", "war shout" },
            ["Bataille - Impact de Métal"] = new[] { "sword clash", "metal impact", "armor hit", "sword hit", "clang" },
            ["Bataille - Mort d'une Unité"] = new[] { "death grunt", "soldier fall", "unit death" },
            ["Antagonistes - Entrée en Scène du Boss"] = new[] { "monster roar", "boss entrance", "roar", "ominous entrance" },
            ["Antagonistes - Boss Vaincu"] = new[] { "boss defeat", "dramatic fall", "boss death" },
            ["Miracle - Début de Prière"] = new[] { "mystic breath", "invocation", "magic whoosh", "prayer start" },
            ["Miracle - Interruption"] = new[] { "dissonant sting", "interruption" },
            ["Miracle - Annulation"] = new[] { "spell fizzle", "magic fail", "fail descend" },
            ["Miracle - Déclenchement"] = new[] { "magic burst", "divine burst", "magic reveal", "choir shofar" },
            ["Foi & Alliance - Foi en Hausse"] = new[] { "positive chime warm", "bright rise note" },
            ["Foi & Alliance - Alliance en Hausse"] = new[] { "positive chime warm", "bright rise note" },
            ["Foi & Alliance - Foi en Baisse"] = new[] { "dissonant note soft", "negative chime" },
            ["Foi & Alliance - Alliance en Baisse"] = new[] { "dissonant note soft", "negative chime" },
            ["Alliance - Entrée en Crise"] = new[] { "dark ominous sting", "low horn warning", "crisis" },
            ["Alliance - Faveur Élevée"] = new[] { "bright solemn chime", "divine approval" },
            ["Alliance - Repentance / Restauration"] = new[] { "redemption swell", "light returning" },
            ["Progression - Leader Débloqué"] = new[] { "solemn fanfare brief", "leader unlock" },
            ["Progression - Leader Actif"] = new[] { "short flourish", "brass flourish" },
            ["Progression - Technologie Économique Débloquée"] = new[] { "oud chime", "economic tech chime" },
            ["Progression - Technologie Militaire Débloquée"] = new[] { "martial chime", "military tech chime" },
            ["Progression - Technologie Spirituelle Débloquée"] = new[] { "harp chime bright", "spiritual tech chime" },
            ["Économie - Population en Hausse"] = new[] { "warm soft positive note", "population rise" },
            ["Économie - Population en Baisse"] = new[] { "dark soft note", "population decline" },
            ["Économie - Murmures du Peuple"] = new[] { "crowd murmur worried", "worried murmur" },
            ["Économie - Rébellion Imminente"] = new[] { "low tense alarm", "urgent alarm", "rebellion" },
            ["Missions - Mission Commencée"] = new[] { "short engaging sting", "mission start" },
            ["Missions - Mission Accomplie"] = new[] { "success fanfare", "mission complete" },
            ["Versets - Verset Débloqué"] = new[] { "soft opening chime", "verse unlock" },
            ["Versets - Verset Mémorisé"] = new[] { "bright shimmering achievement", "achievement shimmer" },
            ["Sauvegarde - Partie Sauvegardée"] = new[] { "soft confirm stamp", "wax seal stamp", "save confirm" },
            ["Sauvegarde - Partie Chargée"] = new[] { "wide parchment scroll open", "load" },
            ["Monétisation - Achat Réussi"] = new[] { "purchase confirm chime", "soft purchase" },
            ["Monétisation - Édition Complète Débloquée"] = new[] { "warm generous fanfare", "celebratory fanfare" },
            ["Collectibles - Artefact Trouvé"] = new[] { "small curious metallic sparkle", "pickup sound", "sparkle" },
            ["Collectibles - Artefact Précieux Trouvé"] = new[] { "golden solemn resonant chime", "golden chime" },
            ["Collectibles - Collection d'Âge Complète"] = new[] { "full triumphant fanfare", "grand fanfare" },
        };

        private static readonly HashSet<string> Stopwords = new HashSet<string>
        {
            "a", "an", "the", "of", "and", "or", "for", "to", "in", "on", "with", "sound", "effects", "effect"
        };

        private void OnGUI()
        {
            GUILayout.Label("Import des effets sonores", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. Choisis le dossier où sont tes fichiers \"sound effects - <description>.mp3/.wav\" " +
                "(ex. \"sound effects - male cry.mp3\") et clique Importer — chaque SFX du jeu reçoit " +
                "une suggestion basée sur les mots-clés de sa description. Le même fichier peut être " +
                "suggéré (et assigné) à plusieurs SFX si ça correspond à plusieurs endroits.\n" +
                "2. VÉRIFIE la liste — les lignes sans clip n'ont trouvé aucune correspondance ; " +
                "corrige/écoute (▶) avant de valider.\n" +
                "3. Clique Assigner pour écrire chaque clip dans le bon SfxCueData.",
                MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            sourceFolder = EditorGUILayout.TextField("Dossier source", sourceFolder);
            if (GUILayout.Button("Parcourir...", GUILayout.Width(90)))
            {
                var picked = EditorUtility.OpenFolderPanel("Dossier des effets sonores", sourceFolder, "");
                if (!string.IsNullOrEmpty(picked)) sourceFolder = picked;
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("1. Importer et suggérer"))
            {
                ImportAndSuggest();
            }

            if (rows.Count == 0) return;

            int matchedCount = rows.Count(r => r.clip != null);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{matchedCount}/{rows.Count} SFX avec une suggestion — vérifie avant d'assigner :");
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(440));
            for (int i = 0; i < rows.Count; i++)
            {
                DrawRow(rows[i]);
            }
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

            EditorGUILayout.LabelField(
                new GUIContent(row.sfx.displayName, row.sfx.description),
                GUILayout.Width(340));
            row.clip = (AudioClip)EditorGUILayout.ObjectField(row.clip, typeof(AudioClip), false, GUILayout.Width(220));

            GUI.enabled = row.clip != null;
            if (GUILayout.Button("▶", GUILayout.Width(28))) AudioPreview.Play(row.clip);
            GUI.enabled = true;

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
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private static HashSet<string> Tokenize(string s)
        {
            return new HashSet<string>(Regex.Split(NormalizeText(s), @"[^a-z0-9]+")
                .Where(t => t.Length > 0 && !Stopwords.Contains(t)));
        }

        private static string ExtractDescriptionFromFilename(string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            return Regex.Replace(name, @"^sound\s*effects?[\s_\-:]*", "", RegexOptions.IgnoreCase);
        }

        private static string CleanFileName(string description)
        {
            var words = Regex.Split(NormalizeText(description), @"[^a-z0-9]+").Where(w => w.Length > 0);
            return string.Join("", words.Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1)));
        }

        private void ImportAndSuggest()
        {
            rows.Clear();
            importedClips.Clear();

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
                .Where(f => Path.GetFileNameWithoutExtension(f).StartsWith("sound effect", StringComparison.OrdinalIgnoreCase))
                .Where(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f)
                .ToList();

            foreach (var sourcePath in sourceFiles)
            {
                string destPath = AssetDatabase.GenerateUniqueAssetPath($"{StagingFolder}/{Path.GetFileName(sourcePath)}");
                File.Copy(sourcePath, destPath, overwrite: false);
                AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceSynchronousImport);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(destPath);
                if (clip == null) continue;

                string description = ExtractDescriptionFromFilename(Path.GetFileName(sourcePath));
                importedClips.Add(new ImportedClip { clip = clip, keywords = Tokenize(description) });
            }

            var sfxGuids = AssetDatabase.FindAssets("t:SfxCueData", new[] { SfxFolder });
            var sfxAssets = sfxGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => (int)AssetDatabase.LoadAssetAtPath<SfxCueData>(path).category)
                .ThenBy(path => path)
                .Select(AssetDatabase.LoadAssetAtPath<SfxCueData>)
                .ToList();

            var used = new HashSet<AudioClip>();
            foreach (var sfx in sfxAssets)
            {
                if (!Keywords.TryGetValue(sfx.displayName, out var phrases))
                {
                    rows.Add(new Row { sfx = sfx, clip = null });
                    continue;
                }

                var sfxTokens = new HashSet<string>(phrases.SelectMany(Tokenize));
                AudioClip best = null;
                int bestScore = 0;
                foreach (var imported in importedClips)
                {
                    int score = sfxTokens.Intersect(imported.keywords).Count();
                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = imported.clip;
                    }
                }

                if (best != null) used.Add(best);
                rows.Add(new Row { sfx = sfx, clip = best });
            }

            int unused = importedClips.Count(c => !used.Contains(c.clip));
            Debug.Log($"Kingdom of God: {rows.Count(r => r.clip != null)}/{rows.Count} SFX ont une suggestion " +
                $"({importedClips.Count} fichiers importés, {unused} non utilisés — assignables à la main via les champs clip).");
        }

        private void AssignClips()
        {
            if (!AssetDatabase.IsValidFolder(FinalFolder))
            {
                Directory.CreateDirectory(FinalFolder);
                AssetDatabase.Refresh();
            }

            var moved = new HashSet<AudioClip>();
            int assigned = 0;
            foreach (var row in rows)
            {
                if (row.skip || row.sfx == null || row.clip == null) continue;

                if (!moved.Contains(row.clip))
                {
                    string currentPath = AssetDatabase.GetAssetPath(row.clip);
                    string ext = Path.GetExtension(currentPath);
                    string description = ExtractDescriptionFromFilename(Path.GetFileName(currentPath));
                    string finalPath = AssetDatabase.GenerateUniqueAssetPath($"{FinalFolder}/SFX_{CleanFileName(description)}{ext}");

                    string moveError = AssetDatabase.MoveAsset(currentPath, finalPath);
                    if (!string.IsNullOrEmpty(moveError))
                    {
                        Debug.LogWarning($"Kingdom of God: déplacement/renommage de {currentPath} échoué ({moveError}).");
                    }
                    moved.Add(row.clip);
                }

                var so = new SerializedObject(row.sfx);
                so.FindProperty("clip").objectReferenceValue = row.clip;
                so.ApplyModifiedProperties();
                assigned++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Kingdom of God: {assigned} SFX assignés à partir de {moved.Count} fichier(s) audio distinct(s) (réutilisation incluse).");
        }
    }
}
