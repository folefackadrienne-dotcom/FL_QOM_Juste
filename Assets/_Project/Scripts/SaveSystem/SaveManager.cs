using System;
using System.IO;
using UnityEngine;

namespace KingdomOfGod.SaveSystem
{
    /// <summary>
    /// Local JSON save/load (works fully offline, per GDD "Mode hors-ligne complet").
    /// Cloud sync (PlayFab or equivalent, for PC/Mobile cross-play) is a stub to be
    /// implemented once a backend is chosen; local save is always the source of truth
    /// until a sync completes.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        private const string SaveFileName = "kingdomofgod_save.json";

        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public event Action<SaveData> Saved;
        public event Action<SaveData> Loaded;

        public void SaveLocal(SaveData data)
        {
            data.savedAtUtc = DateTime.UtcNow;
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, prettyPrint: true));
            Saved?.Invoke(data);
        }

        public bool HasLocalSave() => File.Exists(SavePath);

        public SaveData LoadLocal()
        {
            if (!HasLocalSave()) return null;

            var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            Loaded?.Invoke(data);
            return data;
        }

        /// <summary>TODO: push the local save to the cloud backend (PlayFab or equivalent) once integrated.</summary>
        public void SyncToCloud(SaveData data)
        {
            Debug.Log("Cloud sync not yet implemented — save kept local only.");
        }

        /// <summary>TODO: pull the latest cloud save and reconcile with the local one once integrated.</summary>
        public SaveData SyncFromCloud()
        {
            Debug.Log("Cloud sync not yet implemented — falling back to local save.");
            return LoadLocal();
        }
    }
}
