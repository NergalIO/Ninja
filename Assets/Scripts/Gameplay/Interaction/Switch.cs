using UnityEngine;
using UnityEngine.Events;

namespace Ninja.Gameplay.Interaction
{
    /// <summary>
    /// Интерактивный переключатель (рычаг, кнопка и т.д.)
    /// </summary>
    public class Switch : InteractableObject
    {
        [Header("Switch Settings")]
        [SerializeField] private bool isOn = false;
        [SerializeField] private bool isToggle = true; // Если false - кнопка (автоматически выключается)
        [SerializeField] private float autoOffDelay = 0.5f; // Для кнопки
        
        [Header("Switch Hints")]
        [SerializeField] private string onHint = "Нажмите E чтобы выключить";
        [SerializeField] private string offHint = "Нажмите E чтобы включить";
        
        [Header("Switch Visual")]
        [SerializeField] private Sprite onSprite;
        [SerializeField] private Sprite offSprite;
        [SerializeField] private SpriteRenderer switchRenderer;
        
        [Header("Switch Events")]
        [SerializeField] private UnityEvent onSwitchOn;
        [SerializeField] private UnityEvent onSwitchOff;
        [SerializeField] private UnityEvent<bool> onStateChanged;
        
        public bool IsOn => isOn;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (switchRenderer == null)
                switchRenderer = GetComponent<SpriteRenderer>();
                
            UpdateSwitchState(false);
        }
        
        public override void OnInteract(GameObject interactor)
        {
            Toggle();
            base.OnInteract(interactor);
            
            // Если это кнопка (не toggle), автоматически выключаем
            if (!isToggle && isOn)
            {
                Invoke(nameof(AutoOff), autoOffDelay);
            }
        }
        
        public override void OnFocus(GameObject interactor)
        {
            UpdateHint();
            base.OnFocus(interactor);
        }
        
        /// <summary>
        /// Переключить состояние
        /// </summary>
        public void Toggle()
        {
            SetState(!isOn);
        }
        
        /// <summary>
        /// Установить состояние
        /// </summary>
        public void SetState(bool on)
        {
            if (isOn == on)
                return;
                
            isOn = on;
            UpdateSwitchState(true);
        }
        
        /// <summary>
        /// Включить
        /// </summary>
        public void TurnOn()
        {
            SetState(true);
        }
        
        /// <summary>
        /// Выключить
        /// </summary>
        public void TurnOff()
        {
            SetState(false);
        }
        
        private void AutoOff()
        {
            SetState(false);
        }
        
        private void UpdateSwitchState(bool fireEvents)
        {
            // Обновляем спрайт
            if (switchRenderer != null)
            {
                if (isOn && onSprite != null)
                    switchRenderer.sprite = onSprite;
                else if (!isOn && offSprite != null)
                    switchRenderer.sprite = offSprite;
            }
            
            UpdateHint();
            
            // Вызываем события
            if (fireEvents)
            {
                onStateChanged?.Invoke(isOn);
                
                if (isOn)
                    onSwitchOn?.Invoke();
                else
                    onSwitchOff?.Invoke();
            }
        }
        
        private void UpdateHint()
        {
            SetInteractionHint(isOn ? onHint : offHint);
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (switchRenderer == null)
                switchRenderer = GetComponent<SpriteRenderer>();
        }
#endif
    }
}
