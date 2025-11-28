using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using TMPro;



namespace Ninja.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonController : 
         MonoBehaviour, 
        IPointerEnterHandler, 
        IPointerExitHandler, 
        IPointerDownHandler, 
        IPointerUpHandler
    {
        [Header("Reference")]
        [SerializeField] private TMP_Text text;

        [Header("Options")]
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color hoverColor = Color.gray;
        [SerializeField] private Color clickedColor = Color.whiteSmoke;

        private void Awake()
        {
            if (text == null)
            {
                text = GetComponentInChildren<TMP_Text>();
                if (text == null)
                {
                    GameObject newText = new GameObject("Text");
                    text = newText.AddComponent<TMP_Text>();
                    newText.transform.parent = transform;
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            text.color = clickedColor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            text.color = defaultColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            text.color = hoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            text.color = defaultColor;
        }
    }
}