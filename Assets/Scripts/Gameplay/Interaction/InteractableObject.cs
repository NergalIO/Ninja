using UnityEngine;

namespace Ninja.Gameplay.Interaction
{
    /// <summary>
    /// Базовый класс для объектов взаимодействия
    /// </summary>
    public abstract class InteractableObject : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] protected float interactionDistance = 2f;
        [SerializeField] protected string interactionPrompt = "Нажмите E для взаимодействия";
        [SerializeField] protected bool canInteractMultipleTimes = true;
        [SerializeField] protected bool isInteractable = true;

        protected bool hasBeenInteracted = false;

        public virtual void Interact(Transform interactor)
        {
            if (!CanInteract(interactor))
                return;

            OnInteract(interactor);

            if (!canInteractMultipleTimes)
            {
                hasBeenInteracted = true;
            }
        }

        public virtual bool CanInteract(Transform interactor)
        {
            if (!isInteractable)
                return false;

            if (!canInteractMultipleTimes && hasBeenInteracted)
                return false;

            float distance = Vector2.Distance(transform.position, interactor.position);
            return distance <= interactionDistance;
        }

        public virtual string GetInteractionPrompt()
        {
            return interactionPrompt;
        }

        public virtual float GetInteractionDistance()
        {
            return interactionDistance;
        }

        /// <summary>
        /// Переопределите этот метод для реализации конкретной логики взаимодействия
        /// </summary>
        protected abstract void OnInteract(Transform interactor);

        /// <summary>
        /// Визуализация дистанции взаимодействия в редакторе
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}

