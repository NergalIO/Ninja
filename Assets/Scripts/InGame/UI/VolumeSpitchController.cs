using Ninja.InGame.Player;
using UnityEngine;
using UnityEngine.UI;


namespace Ninja.InGame.UI
{
    public class VolumeSpitchController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MovementController controller;
        [SerializeField] private Image soundImage;

        [Header("Preferences")]
        [SerializeField] private float fadeTime = 0.01f;

        public void FixedUpdate()
        {
            float newOpacity = Mathf.Clamp01(controller.CurrentSpeed.magnitude / 10);
            Color newColor  = new(soundImage.color.r, soundImage.color.g, soundImage.color.b, newOpacity);
            soundImage.color = newColor;
        }
    }
}