using System.Collections;

using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Ninja.UI.Loading
{
    public class LoadingPanel : MenuBase
    {
        [Header("References")]
        [SerializeField] private Slider progressBar;

        [Header("Hint Properties")]
        [SerializeField] private Hints LoadingHints;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private float hintUpdateInterval = 5f;
        [SerializeField] private float hintLastUpdateTime = 0f;

        [Header("Background Properties")]
        [SerializeField] private Backgrounds LoadingBackgrounds;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private float backgroundUpdateInterval = 5f;
        [SerializeField] private float backgroundLastUpdateTime = 0f;
        [SerializeField] private float fadeDuration = 1f;

        private Coroutine backgroundFadeCoroutine;

        public void OnProgress(float progress)
        {
            SetProgress(progress);

            if (Time.time - hintLastUpdateTime >= hintUpdateInterval)
            {
                string newHint = LoadingHints.GetRandomHint();
                SetHint(newHint);
                hintLastUpdateTime = Time.time;
            }

            if (Time.time - backgroundLastUpdateTime >= backgroundUpdateInterval)
            {
                Sprite newBackground = LoadingBackgrounds.GetRandomBackground();
                SetBackground(newBackground);
                backgroundLastUpdateTime = Time.time;
            }
        }

        public void SetProgress(float progress)
        {
            progressBar.value = progress;
        }

        public void SetHint(string hint)
        {
            hintText.text = hint;
        }

        public void SetBackground(Sprite background)
        {
            if (backgroundFadeCoroutine != null)
            {
                StopCoroutine(backgroundFadeCoroutine);
            }
            backgroundFadeCoroutine = StartCoroutine(FadeBackground(background));
        }

        private IEnumerator FadeBackground(Sprite newBackground)
        {
            Image newBackgroundImage = Instantiate(backgroundImage, backgroundImage.transform.parent);
            newBackgroundImage.sprite = newBackground;
            newBackgroundImage.color = new Color(1f, 1f, 1f, 0f);

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                newBackgroundImage.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }
            newBackgroundImage.color = new Color(1f, 1f, 1f, 1f);

            elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
                backgroundImage.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }
            backgroundImage.color = new Color(1f, 1f, 1f, 0f);

            backgroundImage.sprite = newBackground;
            backgroundImage.color = new Color(1f, 1f, 1f, 1f);

            Destroy(newBackgroundImage.gameObject);
        }

        public void Clear()
        {
            progressBar.value = 0f;
            hintText.text = string.Empty;
            backgroundImage.sprite = null;
        }
    }
}