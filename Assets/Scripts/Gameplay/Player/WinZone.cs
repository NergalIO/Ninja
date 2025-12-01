using UnityEngine;
using Ninja.Systems;

namespace Ninja.Gameplay.Player
{
    public class WinZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Проверяем, что в триггер входит именно игрок (по тегу или компоненту)
            if (other.CompareTag("Player") || other.GetComponent<MovementController>() != null)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.NotifyPlayerEscape();
                }
            }
        }
    }
}

