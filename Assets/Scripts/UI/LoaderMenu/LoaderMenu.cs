using Ninja.Hints;
using Ninja.Utils;
using Ninja.Managers;

using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Ninja.UI
{
    public class LoaderMenu : PersistentSingleton<LoaderMenu>
    {  
        [Header("WHEN LOADING")]
        [Header("Slider preferences")]
        [SerializeField] private Slider progressSlider;

        [Header("Hint preferences")]
        [SerializeField] private HintCollection hintCollection;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private float timeToChangeHint = 2f;
        [SerializeField] private float lastTimeHintChanged;

        [Header("Background images preferences")]
        [SerializeField] private Image currentBackground;
        [SerializeField] private float timeToChangeImage = 2f;
        [SerializeField] private float lastTimeImageChanged;
        

        [Header("AFTER LOADED")]
        [SerializeField] private TMP_Text text;
        [SerializeField] private bool spaceIsPressed = false;

        private bool isLoading = false;
        
        protected override void OnSingletonInitialized()
        {
            if (progressSlider == null)
            {
                progressSlider = GetComponentInChildren<Slider>();
            }

            currentBackground.sprite = RandomSprite.RandomSpriteFromResources;
            hintText.text = hintCollection.Hint;
            gameObject.SetActive(false);
        }

        public void OnEnable()
        {
            isLoading = true;
        }

        public void Update()
        {
            if (!isLoading)
            {
                return;
            }

            AsyncSceneLoader sceneLoader = AsyncSceneLoader.Instance;
            if (sceneLoader.State.Equals(LoaderStates.Idle))
            {
                isLoading = false;
                gameObject.SetActive(false);
                return;
            }

            progressSlider.value = sceneLoader.Progress;
            UpdateImage();
            UpdateHint();
        }

        public void UpdateImage()
        {
            if (Time.fixedTime - lastTimeImageChanged < timeToChangeImage)
                return;
            currentBackground.sprite = RandomSprite.RandomSpriteFromResources;
            lastTimeImageChanged = Time.fixedTime;
        }

        public void UpdateHint()
        {
            if (Time.time - lastTimeHintChanged < timeToChangeHint)
                return;
            hintText.text = hintCollection.Hint;
            lastTimeHintChanged = Time.time;
        }
    }
}