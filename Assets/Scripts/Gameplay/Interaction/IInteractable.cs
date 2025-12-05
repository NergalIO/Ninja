using UnityEngine;

namespace Ninja.Gameplay.Interaction
{
    /// <summary>
    /// Интерфейс для всех интерактивных объектов
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Можно ли взаимодействовать с объектом
        /// </summary>
        bool CanInteract { get; }
        
        /// <summary>
        /// Подсказка для взаимодействия (например, "Нажмите E чтобы открыть")
        /// </summary>
        string InteractionHint { get; }
        
        /// <summary>
        /// Transform объекта для определения позиции
        /// </summary>
        Transform Transform { get; }
        
        /// <summary>
        /// Вызывается когда игрок начинает взаимодействие
        /// </summary>
        /// <param name="interactor">Объект, который взаимодействует (игрок)</param>
        void OnInteract(GameObject interactor);
        
        /// <summary>
        /// Вызывается когда игрок входит в зону взаимодействия
        /// </summary>
        /// <param name="interactor">Объект, который вошёл в зону</param>
        void OnFocus(GameObject interactor);
        
        /// <summary>
        /// Вызывается когда игрок выходит из зоны взаимодействия
        /// </summary>
        /// <param name="interactor">Объект, который вышел из зоны</param>
        void OnUnfocus(GameObject interactor);
    }
}
