using Ninja.InGame.InputSystem;
using UnityEngine;
namespace Ninja.Gameplay.Player
{
    public class NoiseController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MovementController controller;
        [SerializeField] private CircleCollider2D noiseArea;

        [Header("Preference")]
        [SerializeField] private float defaultNoise;

        public void Awake()
        {
            if (controller == null)
            {
                controller = GetComponentInChildren<MovementController>();
            }

            if (noiseArea == null)
            {
                noiseArea = new GameObject("Noise Area").AddComponent<CircleCollider2D>();
                noiseArea.isTrigger = true;
            }
        }

        public void FixedUpdate()
        {
            noiseArea.radius = defaultNoise * controller.CurrentSpeed.magnitude;
        }
    }
}
