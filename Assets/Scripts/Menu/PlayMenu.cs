using UnityEngine;
using UnityEngine.UI;
using Ninja.Levels;
using Ninja.Utils;
using System.Collections;
using System.Collections.Generic;

namespace Ninja.UI
{
    public class PlayMenu : MonoBehaviour
    {
        [Header("Main")]
        [SerializeField] private LevelController levelController;

        [Header("UI References")]
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private RectTransform container;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GameObject emptyStateMessage;
        [SerializeField] private Button closeButton;

        [Header("Layout")]
        [SerializeField] private float cardSpacing = 16f;

        private readonly List<LevelCardWidget> LevelWidgets = new();

        private void Awake()
        {
            if (closeButton == null)
            {
                closeButton = GetComponentInChildren<Button>();
            }
            closeButton.onClick.AddListener(CloseWindow);
        }

        private void OnEnable() 
        {
            RefreshView();
        }

        public void CloseWindow()
        {
            gameObject.SetActive(false);
        }

        private void RefreshView()
        {
            if (levelController == null || cardPrefab == null || container == null) {
                ShowEmptyState();
                return;
            }

            ClearView();
            var levels = levelController.Levels;

            if (levels.Count == 0) {
                ShowEmptyState();
                return;
            }

            HideEmptyState();
            foreach (var level in levels)
            {
                CreateCard(level);
            }
            StartCoroutine(DelayedLayoutUpdate());
        }

        private IEnumerator DelayedLayoutUpdate() {
            yield return null;
            UpdateLayout();
            yield return null;
            ScrollToTop();
        }

        public void CreateCard(Level level)
        {
            GameObject widget = Instantiate(cardPrefab, container);
            widget.transform.SetAsFirstSibling();

            LevelCardWidget levelCardWidget = widget.GetComponent<LevelCardWidget>();
            if (levelCardWidget == null) {
                Destroy(widget);
                return;
            }
            levelCardWidget.SetController(levelController);

            var rectTransform = widget.GetComponent<RectTransform>();
            if (rectTransform != null) {
                rectTransform.localScale = Vector3.one;
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.pivot = new Vector2(0.5f, 1f);
                if (rectTransform.sizeDelta == Vector2.zero) {
                    rectTransform.sizeDelta = new Vector2(0, 180);
                }
            }

            widget.SetActive(true);
            levelCardWidget.SetLevel(level);
            LevelWidgets.Add(levelCardWidget);
        }

        private void ClearView()
        {
            foreach (var level in LevelWidgets) 
            {
                if (level != null) Destroy(level.gameObject);
            }
            LevelWidgets.Clear();
        }

        private void UpdateLayout() {
            if (container == null) return;

            container.gameObject.SetActive(true);
            container.anchorMin = new Vector2(0, 1);
            container.anchorMax = new Vector2(1, 1);
            container.pivot = new Vector2(0.5f, 1f);
            container.localScale = Vector3.one;

            var layoutGroup = container.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null) {
                layoutGroup = container.gameObject.AddComponent<VerticalLayoutGroup>();
                layoutGroup.childControlHeight = false;
                layoutGroup.childControlWidth = true;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            }
            layoutGroup.spacing = cardSpacing;

            var contentSizeFitter = container.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null) {
                contentSizeFitter = container.gameObject.AddComponent<ContentSizeFitter>();
            }
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
        }

        public void ScrollToTop() {
            if (scrollRect != null) {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public void ScrollToBottom() {
            if (scrollRect != null) {
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private void ShowEmptyState() {
            emptyStateMessage?.SetActive(true);
        }

        private void HideEmptyState() {
            emptyStateMessage?.SetActive(false);
        }
    }
}