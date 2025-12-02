using UnityEngine;
using Ninja.Systems;
using Ninja.Gameplay.Player;

namespace Ninja.Gameplay.Levels
{
    public class WinZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<MovementController>() != null)
            {
                GameManager.Instance?.NotifyPlayerEscape();
            }
        }
    }
}

