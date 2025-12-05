namespace Ninja.Gameplay.Enemy
{
    public enum EnemyState
    {
        Patrol,
        Alert,      // Насторожился (50% обнаружения)
        Chase,
        Search,
        Investigate,
        Scan,
        Return
    }
}
