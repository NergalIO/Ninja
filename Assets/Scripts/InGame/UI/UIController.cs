using UnityEngine;
using Ninja.Managers;


namespace Ninja.InGame.UI
{
    public class UIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject escMenu;
        [SerializeField] private float timeBetweenActivate = 0f;
        [SerializeField] private float lastActivateTime;

        private void Update()
        {
            if (UIInputController.Instance.EscMenu 
                    && Time.unscaledTime - lastActivateTime > timeBetweenActivate)
            {
                escMenu.SetActive(!escMenu.activeSelf);
                GameManager.Instance.TogglePause();
            }
                
        }
    }
}