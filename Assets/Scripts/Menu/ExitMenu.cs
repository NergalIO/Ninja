using UnityEngine;
using UnityEngine.UI;


namespace Ninja.UI.Menu
{
    public class ExitMenu : MonoBehaviour
    {
        [Header("Controller")]
        [SerializeField] private UIController controller;

        [Header("Buttons")]
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        private void Awake()
        {
            Debug.Log("ExitMenu: Awake");
            yesButton.onClick.AddListener(OnYesButtonClicked);
            noButton.onClick.AddListener(OnNoButtonClicked);
        }

        public void OnYesButtonClicked()
        {
            Debug.Log("ExitMenu: OnYesButtonClicked");
            Application.Quit();
        }

        public void OnNoButtonClicked()
        {
            Debug.Log("ExitMenu: OnNoButtonClicked");
            controller.CloseAllWindows();
        }
    }
}
