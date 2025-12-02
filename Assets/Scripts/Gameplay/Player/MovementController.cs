using UnityEngine;

using Ninja.Input;
using Ninja.Systems;


namespace Ninja.Gameplay.Player
{
    public class MovementController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInputController controller;
        [SerializeField] private Rigidbody2D rigidbody2d;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private MouseInputController mouseInputController;

        [Header("Settings")]
        [SerializeField] private float defaultSpeed = 5f;
        [SerializeField] private float crouchSpeed = 3f;
        [SerializeField] private float sprintSpeed = 7f;
        [SerializeField] private float rotationSpeed = 360f; // Скорость поворота в градусах в секунду
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        public bool IsCrouching => controller.IsCrouching;
        public bool IsSprinting => controller.IsSprinting;
        public Vector2 CurrentSpeed => rigidbody2d.linearVelocity;

        private void Awake()
        {
            if (controller == null)
            {
                controller = GetComponentInChildren<PlayerInputController>();
            }

            if (rigidbody2d == null)
            {
                rigidbody2d = GetComponent<Rigidbody2D>();
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mouseInputController == null)
            {
                mouseInputController = GetComponentInChildren<MouseInputController>();
            }
        }

        private void FixedUpdate()
        {
            float currentSpeed = defaultSpeed;
            if (controller.IsCrouching)
                currentSpeed = crouchSpeed;
            else if (controller.IsSprinting)
                currentSpeed = sprintSpeed;
            rigidbody2d.linearVelocity = controller.MoveDirection * currentSpeed * Time.fixedDeltaTime;
        }

        private void Update()
        {
            // Проверяем и находим камеру, если она не была найдена
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            RotateTowardsMouse();
        }

        private void RotateTowardsMouse()
        {
            if (controller == null || mainCamera == null)
                return;

            // Получаем позицию мыши через PlayerInputController
            Vector2 mouseScreenPosition = controller.MousePosition;
            
            // Проверяем, что позиция мыши валидна (больше нуля)
            if (mouseScreenPosition.x < 0.1f && mouseScreenPosition.y < 0.1f)
                return;

            // Преобразуем позицию мыши из экранных координат в мировые
            // Для ортографической камеры используем правильное преобразование
            Vector3 mouseWorldPosition;
            if (mainCamera.orthographic)
            {
                // Для ортографической камеры используем Camera.ScreenToWorldPoint с правильным Z
                // Z должен быть расстоянием от камеры до плоскости игрока
                float zDistance = mainCamera.transform.position.z - transform.position.z;
                mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, zDistance));
            }
            else
            {
                // Для перспективной камеры
                mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, mainCamera.nearClipPlane));
            }
            
            // Устанавливаем Z координату равной позиции игрока для 2D
            mouseWorldPosition.z = transform.position.z;

            // Вычисляем направление от игрока к мыши
            Vector2 directionToMouse = new Vector2(
                mouseWorldPosition.x - transform.position.x,
                mouseWorldPosition.y - transform.position.y
            );
            
            // Проверяем минимальное расстояние
            float distance = directionToMouse.magnitude;
            if (distance < 0.01f)
                return;

            directionToMouse.Normalize();

            // Вычисляем целевой угол поворота по оси Z
            float targetAngle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
            float currentAngle = transform.eulerAngles.z;
            
            // Вычисляем разницу углов с учетом кратчайшего пути
            float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
            
            if (showDebugInfo)
            {
                Debug.Log($"Mouse Screen: {mouseScreenPosition}, World: {mouseWorldPosition}, Direction: {directionToMouse}, Current Angle: {currentAngle}, Target Angle: {targetAngle}, Difference: {angleDifference}");
            }
            
            // Если разница очень мала, не поворачиваем
            if (Mathf.Abs(angleDifference) < 0.1f)
                return;
            
            // Плавно поворачиваем к целевому углу (используем Time.deltaTime для плавности)
            float rotationStep = Mathf.Sign(angleDifference) * Mathf.Min(Mathf.Abs(angleDifference), rotationSpeed * Time.deltaTime);
            float newAngle = currentAngle + rotationStep;
            
            // Применяем поворот только по оси Z
            transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
        }

    }
}

