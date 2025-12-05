using UnityEngine;
using Ninja.Core.Events;
using Ninja.Gameplay.Player;

namespace Ninja.Gameplay.Levels
{
    public class WinZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<MovementController>() != null)
            {
                Events.Trigger(GameEvents.PlayerEscaped);
            }
        }
    }
}
