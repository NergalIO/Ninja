using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ninja.Systems.Data
{
    [Serializable]
    public class LevelData
    {
        [SerializeField] private string guid;
        [SerializeField] private string levelId;
        [SerializeField] private int timesPlayed;
        [SerializeField] private int timesCaught;
        [SerializeField] private int timesDetected;
        [SerializeField] private int timesHeard;
        [SerializeField] private int timesCompleted;
        [SerializeField] private List<Vector3> catchPositions = new List<Vector3>();
        [SerializeField] private List<Vector3> detectionPositions = new List<Vector3>();
        [SerializeField] private List<Vector3> heardPositions = new List<Vector3>();
        [SerializeField] private List<Vector3> completionPositions = new List<Vector3>();

        public string Guid => guid;
        public string LevelId => levelId;
        public int TimesPlayed => timesPlayed;
        public int TimesCaught => timesCaught;
        public int TimesDetected => timesDetected;
        public int TimesHeard => timesHeard;
        public int TimesCompleted => timesCompleted;
        public IReadOnlyList<Vector3> CatchPositions => catchPositions;
        public IReadOnlyList<Vector3> DetectionPositions => detectionPositions;
        public IReadOnlyList<Vector3> HeardPositions => heardPositions;
        public IReadOnlyList<Vector3> CompletionPositions => completionPositions;

        public LevelData(string levelId, string guid = null)
        {
            this.levelId = levelId;
            this.guid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid().ToString() : guid;
            timesPlayed = 0;
            timesCaught = 0;
            timesDetected = 0;
            timesHeard = 0;
            timesCompleted = 0;
        }

        public void IncrementTimesPlayed()
        {
            timesPlayed++;
        }

        public void AddCatch(Vector3 position)
        {
            timesCaught++;
            catchPositions.Add(position);
        }

        public void AddDetection(Vector3 position)
        {
            timesDetected++;
            detectionPositions.Add(position);
        }

        public void AddHeard(Vector3 position)
        {
            timesHeard++;
            heardPositions.Add(position);
        }

        public void AddCompletion(Vector3 position)
        {
            timesCompleted++;
            completionPositions.Add(position);
        }
    }
}

