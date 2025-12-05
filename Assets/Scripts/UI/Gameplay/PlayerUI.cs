using UnityEngine;
using UnityEngine.UI;
using Ninja.Systems;

namespace Ninja.UI.Gameplay
{
    public class PlayerUI : MenuBase
    {
        [SerializeField] private Rigidbody2D playerRB;
        [SerializeField] private Image noiseIndicator;
        [SerializeField] private float maxVelocity = 10f;
        [SerializeField] private float lerpSpeed = 5f;

        private float currentAlpha;

        public override void Update()
        {
            if (GameManager.Instance && GameManager.Instance.IsPaused)
                Close();
            UpdateNoiseIndicator();
            base.Update();
        }

        private void UpdateNoiseIndicator()
        {
            if (playerRB == null || noiseIndicator == null) return;

            float target = Mathf.Clamp01(playerRB.linearVelocity.magnitude / maxVelocity);
            currentAlpha = Mathf.Lerp(currentAlpha, target, lerpSpeed * Time.deltaTime);

            var c = noiseIndicator.color;
            noiseIndicator.color = new Color(c.r, c.g, c.b, currentAlpha);
        }
    }
}
