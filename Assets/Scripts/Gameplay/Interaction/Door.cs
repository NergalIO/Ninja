using UnityEngine;

namespace Ninja.Gameplay.Interaction
{
    /// <summary>
    /// Пример интерактивного объекта - дверь
    /// </summary>
    public class Door : InteractableObject
    {
        [Header("Door Settings")]
        [SerializeField] private bool isOpen = false;
        [SerializeField] private bool isLocked = false;
        [SerializeField] private string lockedHint = "Дверь заперта";
        [SerializeField] private string openHint = "Нажмите E чтобы закрыть";
        [SerializeField] private string closedHint = "Нажмите E чтобы открыть";
        
        [Header("Door Visual")]
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private SpriteRenderer doorRenderer;
        
        [Header("Door Collider")]
        [SerializeField] private Collider2D doorBlocker;
        
        public bool IsOpen => isOpen;
        public bool IsLocked => isLocked;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (doorRenderer == null)
                doorRenderer = GetComponent<SpriteRenderer>();
                
            UpdateDoorState();
        }
        
        public override void OnInteract(GameObject interactor)
        {
            if (isLocked)
            {
                Debug.Log($"[Door] {gameObject.name} заперта!");
                return;
            }
            
            Toggle();
            base.OnInteract(interactor);
        }
        
        public override void OnFocus(GameObject interactor)
        {
            // Обновляем подсказку перед фокусом
            UpdateHint();
            base.OnFocus(interactor);
        }
        
        /// <summary>
        /// Переключить состояние двери
        /// </summary>
        public void Toggle()
        {
            if (isLocked)
                return;
                
            isOpen = !isOpen;
            UpdateDoorState();
        }
        
        /// <summary>
        /// Открыть дверь
        /// </summary>
        public void Open()
        {
            if (isLocked || isOpen)
                return;
                
            isOpen = true;
            UpdateDoorState();
        }
        
        /// <summary>
        /// Закрыть дверь
        /// </summary>
        public void Close()
        {
            if (!isOpen)
                return;
                
            isOpen = false;
            UpdateDoorState();
        }
        
        /// <summary>
        /// Заблокировать/разблокировать дверь
        /// </summary>
        public void SetLocked(bool locked)
        {
            isLocked = locked;
            UpdateHint();
        }
        
        private void UpdateDoorState()
        {
            // Обновляем спрайт
            if (doorRenderer != null)
            {
                if (isOpen && openSprite != null)
                    doorRenderer.sprite = openSprite;
                else if (!isOpen && closedSprite != null)
                    doorRenderer.sprite = closedSprite;
            }
            
            // Обновляем коллайдер блокировки
            if (doorBlocker != null)
                doorBlocker.enabled = !isOpen;
                
            UpdateHint();
        }
        
        private void UpdateHint()
        {
            if (isLocked)
                SetInteractionHint(lockedHint);
            else if (isOpen)
                SetInteractionHint(openHint);
            else
                SetInteractionHint(closedHint);
        }
    }
}
