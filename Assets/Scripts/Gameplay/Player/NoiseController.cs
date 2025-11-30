using UnityEngine;

using Ninja.Input;
using Unity.Mathematics;


namespace Ninja.Gameplay.Player
{
    public class NoiseController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MovementController controller;
        [SerializeField] private CircleCollider2D noiseArea;

        [Header("Preference")]
        [SerializeField] private float defaultNoise;

        [Header("Variables")]
        [SerializeField] private float lerpTime = 5f;

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
            float targetValue = defaultNoise * controller.CurrentSpeed.magnitude;
            noiseArea.radius = Mathf.Lerp(noiseArea.radius, targetValue, lerpTime);
        }

        public void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(noiseArea.transform.position, noiseArea.radius);
        }
    }
}
