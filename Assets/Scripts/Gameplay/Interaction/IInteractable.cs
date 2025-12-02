using UnityEngine;

namespace Ninja.Gameplay.Interaction
{
    /// <summary>
    /// Интерфейс для объектов, с которыми может взаимодействовать игрок
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Вызывается при взаимодействии с объектом
        /// </summary>
        /// <param name="interactor">Трансформ игрока, который взаимодействует</param>
        void Interact(Transform interactor);

        /// <summary>
        /// Проверяет, можно ли взаимодействовать с объектом в данный момент
        /// </summary>
        bool CanInteract(Transform interactor);

        /// <summary>
        /// Получить текст подсказки для взаимодействия
        /// </summary>
        string GetInteractionPrompt();

        /// <summary>
        /// Получить дистанцию взаимодействия
        /// </summary>
        float GetInteractionDistance();
    }
}

