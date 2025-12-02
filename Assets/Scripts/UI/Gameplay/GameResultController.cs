using UnityEngine;
using Ninja.Systems;



namespace Ninja.UI.Gameplay
{
    public class GameResultController : MonoBehaviour
    {
        [Header("Preferences")]
        [SerializeField] private GameObject background;
        [SerializeField] private LoseWonMenu loseWonWindow;

        public void Start()
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.OnPlayerCatched += PlayerCatched;
            GameManager.Instance.OnPlayerEscapeTrigger += PlayerWin;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.OnPlayerCatched -= PlayerCatched;
            GameManager.Instance.OnPlayerEscapeTrigger -= PlayerWin;
        }

        public void PlayerCatched()
        {
            OpenLoseWonWindow(false);
        }

        public void PlayerWin()
        {
            OpenLoseWonWindow(true);
        }

        public void OpenLoseWonWindow(bool isWon)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }

            loseWonWindow.isWon = isWon;
            loseWonWindow.UpdateStatistics();
            loseWonWindow.Open();
        }
    }
}