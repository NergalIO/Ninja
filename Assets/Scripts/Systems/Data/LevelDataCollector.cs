using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Ninja.Gameplay.Levels;
using Ninja.Gameplay.Player;

namespace Ninja.Systems.Data
{
    public class LevelDataCollector
    {
        private const string SAVE_FILE_NAME = "GameRuns.json";
        
        private Dictionary<string, LevelData> allRuns = new Dictionary<string, LevelData>();
        private string currentLevelId;
        private Transform playerTransform;
        private LevelData currentRunData;

        private bool HasActiveRun => currentRunData != null && !string.IsNullOrEmpty(currentLevelId);

        public void SetCurrentLevel(string levelId)
        {
            if (HasActiveRun)
            {
                SaveCurrentRun();
            }

            currentLevelId = levelId;
            currentRunData = new LevelData(levelId);
            currentRunData.IncrementTimesPlayed();
        }

        public void SetPlayerTransform(Transform player)
        {
            playerTransform = player;
        }

        public void RecordPlayerCaught()
        {
            if (!HasActiveRun)
                return;

            Vector3 position = GetPlayerPosition();
            currentRunData.AddCatch(position);
        }

        public void RecordPlayerDetected()
        {
            if (!HasActiveRun)
                return;

            Vector3 position = GetPlayerPosition();
            currentRunData.AddDetection(position);
        }

        public void RecordPlayerHeard(Vector3 noisePosition)
        {
            if (!HasActiveRun)
                return;

            currentRunData.AddHeard(noisePosition);
        }

        public void RecordPlayerEscape()
        {
            if (!HasActiveRun)
                return;

            Vector3 position = GetPlayerPosition();
            currentRunData.AddCompletion(position);
        }

        private Vector3 GetPlayerPosition()
        {
            if (playerTransform == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    playerTransform = playerObject.transform;
                }
                else
                {
                    var movementController = UnityEngine.Object.FindFirstObjectByType<MovementController>();
                    if (movementController != null)
                    {
                        playerTransform = movementController.transform;
                    }
                    else
                    {
                        return Vector3.zero;
                    }
                }
            }

            return playerTransform.position;
        }

        public LevelData GetLevelData(string levelId)
        {
            LevelData combined = new LevelData(levelId);
            foreach (var run in allRuns.Values)
            {
                if (run.LevelId == levelId)
                {
                    MergeData(combined, run);
                }
            }
            return combined;
        }

        public LevelData GetCurrentLevelData()
        {
            return currentRunData;
        }

        private void SaveCurrentRun()
        {
            if (currentRunData == null || string.IsNullOrEmpty(currentRunData.Guid))
                return;

            allRuns[currentRunData.Guid] = currentRunData;
        }

        private void MergeData(LevelData target, LevelData source)
        {
            for (int i = 0; i < source.TimesPlayed; i++)
            {
                target.IncrementTimesPlayed();
            }
            
            foreach (var pos in source.CatchPositions)
            {
                target.AddCatch(pos);
            }
            foreach (var pos in source.DetectionPositions)
            {
                target.AddDetection(pos);
            }
            foreach (var pos in source.HeardPositions)
            {
                target.AddHeard(pos);
            }
            foreach (var pos in source.CompletionPositions)
            {
                target.AddCompletion(pos);
            }
        }

        public IReadOnlyDictionary<string, LevelData> GetAllLevelData()
        {
            Dictionary<string, LevelData> result = new Dictionary<string, LevelData>();
            foreach (var run in allRuns.Values)
            {
                if (!result.ContainsKey(run.LevelId))
                {
                    result[run.LevelId] = new LevelData(run.LevelId);
                }
                MergeData(result[run.LevelId], run);
            }
            return result;
        }

        public IReadOnlyDictionary<string, LevelData> GetAllRuns()
        {
            return allRuns;
        }

        public void ClearData()
        {
            allRuns.Clear();
            currentLevelId = null;
            currentRunData = null;
        }

        #region Save/Load
        public void SaveData()
        {
            try
            {
                if (HasActiveRun)
                {
                    SaveCurrentRun();
                }

                LoadDataInternal();

                if (allRuns.Count == 0)
                    return;

                LevelDataContainer container = new LevelDataContainer();
                container.runs = new Dictionary<string, LevelData>(allRuns);

                string json = SerializeRuns(container.runs);
                
                if (string.IsNullOrEmpty(json) || json == "{}")
                    return;

                string filePath = GetSaveFilePath();
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game runs data: {e.Message}\n{e.StackTrace}");
            }
        }

        private string SerializeRuns(Dictionary<string, LevelData> runs)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("{\n");
            bool first = true;
            foreach (var kvp in runs)
            {
                if (!first) sb.Append(",\n");
                first = false;
                string runJson = JsonUtility.ToJson(kvp.Value);
                sb.Append($"  \"{kvp.Key}\": {runJson}");
            }
            sb.Append("\n}");
            return sb.ToString();
        }

        public void LoadData()
        {
            LoadDataInternal();
        }

        private void LoadDataInternal()
        {
            try
            {
                string filePath = GetSaveFilePath();

                if (!File.Exists(filePath))
                    return;

                string json = File.ReadAllText(filePath);
                Dictionary<string, LevelData> loadedRuns = DeserializeRuns(json);

                if (loadedRuns == null || loadedRuns.Count == 0)
                    return;

                foreach (var kvp in loadedRuns)
                {
                    if (!allRuns.ContainsKey(kvp.Key))
                    {
                        allRuns[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game runs data: {e.Message}");
            }
        }

        private Dictionary<string, LevelData> DeserializeRuns(string json)
        {
            Dictionary<string, LevelData> runs = new Dictionary<string, LevelData>();
            
            try
            {
                json = json.Replace("\n", "").Replace("\r", "").Replace(" ", "");
                
                int startIndex = json.IndexOf('{') + 1;
                int endIndex = json.LastIndexOf('}');
                
                if (startIndex <= 0 || endIndex <= startIndex)
                    return runs;

                string content = json.Substring(startIndex, endIndex - startIndex);
                
                int pos = 0;
                while (pos < content.Length)
                {
                    int guidStart = content.IndexOf('"', pos);
                    if (guidStart == -1) break;
                    int guidEnd = content.IndexOf('"', guidStart + 1);
                    if (guidEnd == -1) break;
                    
                    string guid = content.Substring(guidStart + 1, guidEnd - guidStart - 1);
                    
                    int dataStart = content.IndexOf('{', guidEnd);
                    if (dataStart == -1) break;
                    
                    int braceCount = 0;
                    int dataEnd = dataStart;
                    for (int i = dataStart; i < content.Length; i++)
                    {
                        if (content[i] == '{') braceCount++;
                        if (content[i] == '}') braceCount--;
                        if (braceCount == 0)
                        {
                            dataEnd = i + 1;
                            break;
                        }
                    }
                    
                    string runJson = content.Substring(dataStart, dataEnd - dataStart);
                    LevelData runData = JsonUtility.FromJson<LevelData>(runJson);
                    
                    if (runData != null && !string.IsNullOrEmpty(runData.Guid))
                    {
                        runs[guid] = runData;
                    }
                    
                    pos = dataEnd;
                    if (pos < content.Length && content[pos] == ',')
                        pos++;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize runs: {e.Message}");
            }
            
            return runs;
        }

        public void LoadDataFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return;

                string json = File.ReadAllText(filePath);
                Dictionary<string, LevelData> loadedRuns = DeserializeRuns(json);

                if (loadedRuns == null || loadedRuns.Count == 0)
                    return;

                allRuns.Clear();
                foreach (var kvp in loadedRuns)
                {
                    allRuns[kvp.Key] = kvp.Value;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game runs data: {e.Message}");
            }
        }

        public string[] GetAllSaveFiles()
        {
            string directory = Application.persistentDataPath;
            if (!Directory.Exists(directory))
                return new string[0];

            string filePath = GetSaveFilePath();
            if (File.Exists(filePath))
            {
                return new string[] { filePath };
            }
            return new string[0];
        }

        private string GetSaveFilePath()
        {
            return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        }

        public string GetSaveFilePathForEditor()
        {
            return GetSaveFilePath();
        }
        #endregion
    }
}

