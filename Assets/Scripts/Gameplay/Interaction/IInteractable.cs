using UnityEngine;

namespace Ninja.Gameplay.Interaction
{
    public interface IInteractable
    {
        void Interact(Transform interactor);
        bool CanInteract(Transform interactor);
        string GetInteractionPrompt();
        float GetInteractionDistance();
    }
}
