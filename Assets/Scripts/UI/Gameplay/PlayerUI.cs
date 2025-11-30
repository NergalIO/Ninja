using Ninja.Systems;
using UnityEngine;
using UnityEngine.UI;


namespace Ninja.UI.Gameplay
{
    public class PlayerUI : MenuBase
    {
        [Header("References")]
        [SerializeField] private Rigidbody2D playerRB2D;

        [Header("Components")]
        [SerializeField] private Image noiseSpitch;

        [Header("Settings")]
        [SerializeField] private float maxVelocityForAlpha = 10f;
        [SerializeField] private float alphaLerpSpeed = 5f;

        private float currentAlpha = 0f;

        public override void Update()
        {
            if (GameManager.Instance.IsPaused)
                Close();
            base.Update();
        }

        public void FixedUpdate()
        {
            UpdateNoiseSpitch();
        }

        public void UpdateNoiseSpitch()
        {
            float currentVelocityMagnitude = playerRB2D.linearVelocity.magnitude;
            float targetAlpha = Mathf.Clamp01(currentVelocityMagnitude / maxVelocityForAlpha);
            
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, alphaLerpSpeed * Time.deltaTime);
            
            Color currentColor = noiseSpitch.color;
            Color newColor = new(
                r: currentColor.r, 
                b: currentColor.b, 
                g: currentColor.g, 
                a: currentAlpha
            );
            noiseSpitch.color = newColor;
        }
    }
}