using UnityEngine;
using Ninja.Systems;
using Ninja.Core.Events;

namespace Ninja.UI.Gameplay
{
    public class GameResultController : MonoBehaviour
    {
        [SerializeField] private LoseWonMenu loseWonWindow;

        private void Start()
        {
            Events.Subscribe(GameEvents.PlayerCaught, OnPlayerCaught);
            Events.Subscribe(GameEvents.PlayerEscaped, OnPlayerEscaped);
        }

        private void OnDestroy()
        {
            Events.Unsubscribe(GameEvents.PlayerCaught, OnPlayerCaught);
            Events.Unsubscribe(GameEvents.PlayerEscaped, OnPlayerEscaped);
        }

        private void OnPlayerCaught(EventArgs _) => ShowResult(false);
        private void OnPlayerEscaped(EventArgs _) => ShowResult(true);

        private void ShowResult(bool won)
        {
            GameManager.Instance?.Pause();
            loseWonWindow.isWon = won;
            loseWonWindow.UpdateStatistics();
            loseWonWindow.Open();
        }
    }
}
