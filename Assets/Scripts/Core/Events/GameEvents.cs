namespace Ninja.Core.Events
{
    /// <summary>
    /// Константы имён игровых событий
    /// </summary>
    public static class GameEvents
    {
        // Game State
        public const string GamePaused = "GamePaused";
        public const string GameResumed = "GameResumed";
        
        // Level
        public const string LevelStarted = "LevelStarted";
        public const string LevelCompleted = "LevelCompleted";
        
        // Player
        public const string PlayerEscaped = "PlayerEscaped";
        public const string PlayerCaught = "PlayerCaught";
        public const string PlayerDetected = "PlayerDetected";
        public const string PlayerHeard = "PlayerHeard";
        
        // Enemy
        public const string ChaseStarted = "ChaseStarted";
        public const string ChaseEnded = "ChaseEnded";
        
        // Interaction
        public const string InteractionFocused = "InteractionFocused";
        public const string InteractionUnfocused = "InteractionUnfocused";
        public const string InteractionPerformed = "InteractionPerformed";
    }
}
