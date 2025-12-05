using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ninja.UI.Loading
{
    public class LoadingPanel : MenuBase
    {
        [SerializeField] private Slider progressBar;
        [SerializeField] private Hints hints;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private float hintInterval = 5f;

        [SerializeField] private Backgrounds backgrounds;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private float bgInterval = 5f;
        [SerializeField] private float fadeDuration = 1f;

        private float lastHintTime;
        private float lastBgTime;
        private Coroutine fadeCoroutine;

        public void OnProgress(float progress)
        {
            progressBar.value = progress;

            if (Time.time - lastHintTime >= hintInterval)
            {
                hintText.text = hints?.GetRandomHint() ?? "";
                lastHintTime = Time.time;
            }

            if (Time.time - lastBgTime >= bgInterval)
            {
                var bg = backgrounds?.GetRandomBackground();
                if (bg != null) SetBackground(bg);
                lastBgTime = Time.time;
            }
        }

        private void SetBackground(Sprite sprite)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeBackground(sprite));
        }

        private IEnumerator FadeBackground(Sprite newBg)
        {
            var newImage = Instantiate(backgroundImage, backgroundImage.transform.parent);
            newImage.sprite = newBg;
            newImage.color = new Color(1, 1, 1, 0);

            // Fade in new
            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                newImage.color = new Color(1, 1, 1, t / fadeDuration);
                yield return null;
            }

            // Fade out old
            t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                backgroundImage.color = new Color(1, 1, 1, 1 - t / fadeDuration);
                yield return null;
            }

            backgroundImage.sprite = newBg;
            backgroundImage.color = Color.white;
            Destroy(newImage.gameObject);
        }
    }
}
