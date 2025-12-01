using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Ninja.Systems.Data;
using Ninja.Systems;

namespace Ninja.Editor
{
    public class LevelDataViewer : EditorWindow
    {
        private Vector2 scrollPosition;
        private Dictionary<string, LevelData> loadedRuns;
        private Dictionary<string, LevelData> aggregatedLevelData; // Данные по уровням (суммированные)
        private string filePath;
        private bool autoRefresh = true;
        private float lastRefreshTime;
        private bool showRuns = false; // Показывать отдельные пробеги или агрегированные данные

        [MenuItem("Ninja/Level Data Viewer")]
        public static void ShowWindow()
        {
            GetWindow<LevelDataViewer>("Level Data Viewer");
        }

        private void OnEnable()
        {
            UpdateFilePath();
            LoadData();
        }

        private void UpdateFilePath()
        {
            if (GameManager.Instance != null)
            {
                var files = GameManager.Instance.DataCollector.GetAllSaveFiles();
                if (files.Length > 0)
                {
                    filePath = files[0];
                }
                else
                {
                    filePath = Application.persistentDataPath + "/GameRuns.json";
                }
            }
            else
            {
                filePath = Application.persistentDataPath + "/GameRuns.json";
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            
            // Заголовок
            EditorGUILayout.Space(10);
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("Level Statistics", titleStyle);
            EditorGUILayout.Space(10);

            // Информация о файле
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("File Path:", GUILayout.Width(80));
            EditorGUILayout.SelectableLabel(filePath, EditorStyles.textField, GUILayout.Height(20));
            EditorGUILayout.EndHorizontal();

            // Список всех файлов
            if (GameManager.Instance != null)
            {
                var allFiles = GameManager.Instance.DataCollector.GetAllSaveFiles();
                if (allFiles.Length > 0)
                {
                    EditorGUILayout.LabelField($"Total Save Files: {allFiles.Length}", EditorStyles.miniLabel);
                    EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(Mathf.Min(60, allFiles.Length * 20)));
                    foreach (var file in allFiles)
                    {
                        EditorGUILayout.BeginHorizontal();
                        string fileName = System.IO.Path.GetFileName(file);
                        if (GUILayout.Button(fileName, EditorStyles.linkLabel, GUILayout.Width(300)))
                        {
                            filePath = file;
                            LoadData();
                        }
                        EditorGUILayout.LabelField(System.IO.File.GetCreationTime(file).ToString("yyyy-MM-dd HH:mm:ss"), EditorStyles.miniLabel);
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndScrollView();
                }
            }

            // Кнопки управления
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Data", GUILayout.Height(30)))
            {
                LoadData();
            }
            if (GUILayout.Button("Save Data", GUILayout.Height(30)))
            {
                SaveData();
            }
            if (GUILayout.Button("Refresh", GUILayout.Height(30)))
            {
                LoadData();
            }
            if (GUILayout.Button("Clear All Data", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Clear All Data", 
                    "Are you sure you want to clear all level data? This cannot be undone!", 
                    "Yes", "No"))
                {
                    ClearAllData();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Автообновление
            autoRefresh = EditorGUILayout.Toggle("Auto Refresh", autoRefresh);
            if (autoRefresh && Time.realtimeSinceStartup - lastRefreshTime > 2f)
            {
                LoadData();
                lastRefreshTime = Time.realtimeSinceStartup;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // Переключатель режима отображения
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("View Mode:", GUILayout.Width(100));
            showRuns = EditorGUILayout.Toggle("Show Individual Runs", showRuns);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Отображение данных
            if (GameManager.Instance != null)
            {
                // Используем данные из GameManager
                if (showRuns)
                {
                    var allRuns = GameManager.Instance.DataCollector.GetAllRuns();
                    if (allRuns == null || allRuns.Count == 0)
                    {
                        EditorGUILayout.HelpBox("No run data found. Play the game to generate statistics.", MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"Total Runs: {allRuns.Count}", EditorStyles.boldLabel);
                        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                        foreach (var kvp in allRuns.OrderBy(r => r.Value.LevelId).ThenBy(r => r.Key))
                        {
                            DrawRunData(kvp.Key, kvp.Value);
                        }
                        EditorGUILayout.EndScrollView();
                    }
                }
                else
                {
                    var allLevelData = GameManager.Instance.DataCollector.GetAllLevelData();
                    if (allLevelData == null || allLevelData.Count == 0)
                    {
                        EditorGUILayout.HelpBox("No level data found. Play the game to generate statistics.", MessageType.Info);
                    }
                    else
                    {
                        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                        foreach (var levelData in allLevelData.Values.OrderBy(l => l.LevelId))
                        {
                            DrawLevelData(levelData);
                        }
                        EditorGUILayout.EndScrollView();
                    }
                }
            }
            else
            {
                // Если GameManager недоступен, пытаемся загрузить из файла
                if (loadedRuns == null || loadedRuns.Count == 0)
                {
                    EditorGUILayout.HelpBox("GameManager not found. Please run the game to collect data.", MessageType.Info);
                }
                else
                {
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                    if (showRuns)
                    {
                        foreach (var kvp in loadedRuns.OrderBy(r => r.Value.LevelId).ThenBy(r => r.Key))
                        {
                            DrawRunData(kvp.Key, kvp.Value);
                        }
                    }
                    else
                    {
                        foreach (var levelData in aggregatedLevelData.Values.OrderBy(l => l.LevelId))
                        {
                            DrawLevelData(levelData);
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawRunData(string guid, LevelData runData)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Заголовок пробега
            GUIStyle runTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12
            };
            EditorGUILayout.LabelField($"Run: {guid.Substring(0, Mathf.Min(8, guid.Length))}... | Level: {runData.LevelId}", runTitleStyle);
            EditorGUILayout.Space(3);
            
            DrawLevelDataContent(runData);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }

        private void DrawLevelData(LevelData levelData)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Заголовок уровня
            GUIStyle levelTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14
            };
            EditorGUILayout.LabelField($"Level: {levelData.LevelId}", levelTitleStyle);
            EditorGUILayout.Space(5);
            
            DrawLevelDataContent(levelData);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawLevelDataContent(LevelData levelData)
        {
            EditorGUILayout.Space(5);

            // Статистика
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Times Played:", GUILayout.Width(120));
            EditorGUILayout.LabelField(levelData.TimesPlayed.ToString(), EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Times Caught:", GUILayout.Width(120));
            EditorGUILayout.LabelField(levelData.TimesCaught.ToString(), 
                levelData.TimesCaught > 0 ? EditorStyles.boldLabel : EditorStyles.label);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Times Detected:", GUILayout.Width(120));
            EditorGUILayout.LabelField(levelData.TimesDetected.ToString(), 
                levelData.TimesDetected > 0 ? EditorStyles.boldLabel : EditorStyles.label);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Times Heard:", GUILayout.Width(120));
            EditorGUILayout.LabelField(levelData.TimesHeard.ToString(), 
                levelData.TimesHeard > 0 ? EditorStyles.boldLabel : EditorStyles.label);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Times Completed:", GUILayout.Width(120));
            GUIStyle completedStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = Color.green }
            };
            EditorGUILayout.LabelField(levelData.TimesCompleted.ToString(), 
                levelData.TimesCompleted > 0 ? completedStyle : EditorStyles.label);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Позиции событий
            if (levelData.CatchPositions != null && levelData.CatchPositions.Count > 0)
            {
                EditorGUILayout.LabelField($"Catch Positions ({levelData.CatchPositions.Count}):", EditorStyles.miniLabel);
                foreach (var pos in levelData.CatchPositions)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  • ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})", EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (levelData.DetectionPositions != null && levelData.DetectionPositions.Count > 0)
            {
                EditorGUILayout.LabelField($"Detection Positions ({levelData.DetectionPositions.Count}):", EditorStyles.miniLabel);
                foreach (var pos in levelData.DetectionPositions)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  • ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})", EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (levelData.HeardPositions != null && levelData.HeardPositions.Count > 0)
            {
                EditorGUILayout.LabelField($"Heard Positions ({levelData.HeardPositions.Count}):", EditorStyles.miniLabel);
                foreach (var pos in levelData.HeardPositions)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  • ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})", EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (levelData.CompletionPositions != null && levelData.CompletionPositions.Count > 0)
            {
                GUIStyle completionHeaderStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = Color.green }
                };
                EditorGUILayout.LabelField($"Completion Positions ({levelData.CompletionPositions.Count}):", completionHeaderStyle);
                foreach (var pos in levelData.CompletionPositions)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  • ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})", EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }
            }

        }

        private void LoadData()
        {
            UpdateFilePath();

            if (GameManager.Instance != null)
            {
                // Используем данные из GameManager
                GameManager.Instance.LoadLevelData();
                return;
            }

            // Если GameManager недоступен, загружаем из файла напрямую
            if (!File.Exists(filePath))
            {
                loadedRuns = new Dictionary<string, LevelData>();
                aggregatedLevelData = new Dictionary<string, LevelData>();
                return;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                
                // Парсим JSON вручную, так как JsonUtility не поддерживает Dictionary
                loadedRuns = DeserializeRuns(json);
                
                if (loadedRuns == null)
                {
                    loadedRuns = new Dictionary<string, LevelData>();
                }

                // Агрегируем данные по уровням
                aggregatedLevelData = new Dictionary<string, LevelData>();
                foreach (var kvp in loadedRuns)
                {
                    if (!aggregatedLevelData.ContainsKey(kvp.Value.LevelId))
                    {
                        aggregatedLevelData[kvp.Value.LevelId] = new LevelData(kvp.Value.LevelId);
                    }
                    MergeData(aggregatedLevelData[kvp.Value.LevelId], kvp.Value);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load level data: {e.Message}");
                loadedRuns = new Dictionary<string, LevelData>();
                aggregatedLevelData = new Dictionary<string, LevelData>();
            }
        }

        private Dictionary<string, LevelData> DeserializeRuns(string json)
        {
            Dictionary<string, LevelData> runs = new Dictionary<string, LevelData>();
            
            try
            {
                // Простой парсинг JSON формата {"GUID-1": {...}, "GUID-2": {...}}
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

        private void SaveData()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveLevelData();
                LoadData();
                Debug.Log("Level data saved successfully!");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "GameManager instance not found. Please start the game first.", "OK");
            }
        }

        private void ClearAllData()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DataCollector.ClearData();
                GameManager.Instance.SaveLevelData();
                LoadData();
                Debug.Log("All level data cleared!");
            }
            else
            {
                // Удаляем файл напрямую
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    LoadData();
                    Debug.Log("Level data file deleted!");
                }
            }
        }
    }
}

