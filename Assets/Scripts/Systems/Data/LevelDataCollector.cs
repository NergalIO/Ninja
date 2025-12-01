using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Ninja.Gameplay.Levels;

namespace Ninja.Systems.Data
{
    [Serializable]
    public class LevelDataContainer
    {
        public Dictionary<string, LevelData> runs = new Dictionary<string, LevelData>();
    }

    public class LevelDataCollector
    {
        private const string SAVE_FILE_NAME = "GameRuns.json";
        
        private Dictionary<string, LevelData> allRuns = new Dictionary<string, LevelData>(); // Все пробеги по GUID
        private string currentLevelId;
        private Transform playerTransform;
        private LevelData currentRunData; // Временные данные для текущего пробега

        public void SetCurrentLevel(string levelId)
        {
            // Завершаем предыдущий пробег, если он был
            if (currentRunData != null && !string.IsNullOrEmpty(currentLevelId))
            {
                SaveCurrentRun();
            }

            currentLevelId = levelId;
            
            // Создаем новый временный объект данных для текущего пробега с новым GUID
            currentRunData = new LevelData(levelId);
            currentRunData.IncrementTimesPlayed();
        }

        public void SetPlayerTransform(Transform player)
        {
            playerTransform = player;
        }

        public void RecordPlayerCaught()
        {
            if (currentRunData == null || string.IsNullOrEmpty(currentLevelId))
                return;

            Vector3 position = playerTransform != null ? playerTransform.position : Vector3.zero;
            currentRunData.AddCatch(position);
        }

        public void RecordPlayerDetected()
        {
            if (currentRunData == null || string.IsNullOrEmpty(currentLevelId))
                return;

            Vector3 position = playerTransform != null ? playerTransform.position : Vector3.zero;
            currentRunData.AddDetection(position);
        }

        public void RecordPlayerHeard(Vector3 noisePosition, string sceneName = null)
        {
            if (currentRunData == null || string.IsNullOrEmpty(currentLevelId))
                return;

            currentRunData.AddHeard(noisePosition);
        }

        public void RecordPlayerEscape(string sceneName = null)
        {
            if (currentRunData == null || string.IsNullOrEmpty(currentLevelId))
                return;

            Vector3 position = playerTransform != null ? playerTransform.position : Vector3.zero;
            currentRunData.AddCompletion(position);
        }

        public LevelData GetLevelData(string levelId)
        {
            // Возвращаем данные для конкретного уровня (суммируем все пробеги этого уровня)
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
            // Возвращаем данные текущего пробега
            return currentRunData;
        }

        private void SaveCurrentRun()
        {
            if (currentRunData == null || string.IsNullOrEmpty(currentRunData.Guid))
                return;

            // Сохраняем текущий пробег в общий словарь
            allRuns[currentRunData.Guid] = currentRunData;
        }

        private void MergeData(LevelData target, LevelData source)
        {
            // Объединяем счетчики и позиции
            // Добавляем TimesPlayed из source в target
            for (int i = 0; i < source.TimesPlayed; i++)
            {
                target.IncrementTimesPlayed();
            }
            
            // Добавляем позиции из source в target (счетчики обновятся автоматически)
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
            // Возвращаем все пробеги, сгруппированные по levelId
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
        public void SaveData(string sceneName = null)
        {
            try
            {
                // Сохраняем текущий пробег перед сохранением файла
                if (currentRunData != null && !string.IsNullOrEmpty(currentLevelId))
                {
                    SaveCurrentRun();
                }

                // Загружаем существующие данные из файла
                LoadDataInternal();

                // Объединяем текущие пробеги с загруженными
                foreach (var run in allRuns.Values)
                {
                    // Пробеги уже в allRuns, просто убеждаемся что они сохранены
                }

                // Проверяем, есть ли данные для сохранения
                if (allRuns.Count == 0)
                {
                    Debug.LogWarning("No run data to save. AllRuns is empty.");
                    return;
                }

                // Создаем контейнер с пробегами
                LevelDataContainer container = new LevelDataContainer();
                container.runs = new Dictionary<string, LevelData>(allRuns);

                // JsonUtility не поддерживает Dictionary напрямую, используем обходной путь
                string json = SerializeRuns(container.runs);
                
                // Проверяем, что JSON не пустой
                if (string.IsNullOrEmpty(json) || json == "{}")
                {
                    Debug.LogWarning("Generated JSON is empty. No data to save.");
                    return;
                }

                string filePath = GetSaveFilePath(sceneName);

                // Создаем директорию, если её нет
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, json);
                Debug.Log($"Game runs data saved to: {filePath} (Runs: {allRuns.Count})");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game runs data: {e.Message}\n{e.StackTrace}");
            }
        }

        private string SerializeRuns(Dictionary<string, LevelData> runs)
        {
            // JsonUtility не поддерживает Dictionary, создаем список пар
            var runList = new List<KeyValuePair<string, LevelData>>();
            foreach (var kvp in runs)
            {
                runList.Add(kvp);
            }
            
            // Используем простой формат JSON вручную
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
                {
                    Debug.Log($"No save file found at: {filePath}");
                    return;
                }

                string json = File.ReadAllText(filePath);
                
                // Парсим JSON вручную, так как JsonUtility не поддерживает Dictionary
                Dictionary<string, LevelData> loadedRuns = DeserializeRuns(json);

                if (loadedRuns == null || loadedRuns.Count == 0)
                {
                    Debug.LogWarning("Failed to parse game runs data from JSON or no runs found");
                    return;
                }

                // Объединяем загруженные пробеги с текущими (не перезаписываем текущий пробег)
                foreach (var kvp in loadedRuns)
                {
                    if (!allRuns.ContainsKey(kvp.Key))
                    {
                        allRuns[kvp.Key] = kvp.Value;
                    }
                }

                Debug.Log($"Game runs data loaded from: {filePath}. Loaded {loadedRuns.Count} runs.");
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
                // Простой парсинг JSON формата {"GUID-1": {...}, "GUID-2": {...}}
                // Удаляем пробелы и переносы строк для упрощения
                json = json.Replace("\n", "").Replace("\r", "").Replace(" ", "");
                
                // Находим все GUID и их данные
                int startIndex = json.IndexOf('{') + 1;
                int endIndex = json.LastIndexOf('}');
                
                if (startIndex <= 0 || endIndex <= startIndex)
                    return runs;

                string content = json.Substring(startIndex, endIndex - startIndex);
                
                // Разбиваем по запятым между парами ключ-значение
                int pos = 0;
                while (pos < content.Length)
                {
                    // Находим следующий GUID
                    int guidStart = content.IndexOf('"', pos);
                    if (guidStart == -1) break;
                    int guidEnd = content.IndexOf('"', guidStart + 1);
                    if (guidEnd == -1) break;
                    
                    string guid = content.Substring(guidStart + 1, guidEnd - guidStart - 1);
                    
                    // Находим начало данных (после :)
                    int dataStart = content.IndexOf('{', guidEnd);
                    if (dataStart == -1) break;
                    
                    // Находим конец данных (соответствующая закрывающая скобка)
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
                    
                    // Переходим к следующей паре
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
                {
                    Debug.LogWarning($"File not found: {filePath}");
                    return;
                }

                string json = File.ReadAllText(filePath);
                Dictionary<string, LevelData> loadedRuns = DeserializeRuns(json);

                if (loadedRuns == null || loadedRuns.Count == 0)
                {
                    Debug.LogWarning("Failed to parse game runs data from JSON");
                    return;
                }

                allRuns.Clear();
                foreach (var kvp in loadedRuns)
                {
                    allRuns[kvp.Key] = kvp.Value;
                }

                Debug.Log($"Game runs data loaded from: {filePath}. Loaded {allRuns.Count} runs.");
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

            // Ищем файл GameRuns.json
            string filePath = GetSaveFilePath();
            if (File.Exists(filePath))
            {
                return new string[] { filePath };
            }
            return new string[0];
        }

        private string GetSaveFilePath(string sceneName = null)
        {
            // Всегда используем один файл для всех пробегов
            return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        }

        public string GetSaveFilePathForEditor()
        {
            return GetSaveFilePath();
        }
        #endregion
    }
}

