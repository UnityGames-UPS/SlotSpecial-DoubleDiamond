using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class OCController : MonoBehaviour
{
    [System.Serializable]
    private struct SharedLayoutTransform
    {
        public RectTransform target;
        public Vector2 landscapePosition;
        public Vector3 landscapeScale;
        public Vector2 portraitPosition;
        public Vector3 portraitScale;
        public bool preserveVerticalPosition;

        public void Apply(bool isMobilePortrait)
        {
            if (target == null)
                return;

            var position = isMobilePortrait ? portraitPosition : landscapePosition;
            if (preserveVerticalPosition)
                position.y = target.anchoredPosition.y;

            target.anchoredPosition = position;
            target.localScale = isMobilePortrait ? portraitScale : landscapeScale;
        }
    }

    [Header("References")]
    [SerializeField] private OrientationChange orientationChange;
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private Transform slotObject;
    [SerializeField] private List<RectTransform> resizedObjects = new List<RectTransform>();
    [SerializeField] private List<RectTransform> squareResizedObjects = new List<RectTransform>();

    [Header("Panel Toggle Settings")]
    [SerializeField] private GameObject landscapePanelObject;
    [SerializeField] private GameObject portraitPanelObject;

    [Header("Background Toggle Settings")]
    [SerializeField] private GameObject landscapeBackground;
    [SerializeField] private GameObject portraitBackground;
    [SerializeField] private GameObject freeSpinLandscapeBackground;
    [SerializeField] private GameObject freeSpinPortraitBackground;
    [SerializeField] private GameObject wheelLandscapeBackground;
    [SerializeField] private GameObject wheelPortraitBackground;

    [Header("Canvas Scaler Resolutions")]
    [SerializeField] private Vector2 landscapeReferenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] private Vector2 portraitReferenceResolution = new Vector2(1080f, 1920f);

    [Header("Resized Object Dimensions")]
    [SerializeField] private Vector2 landscapeResizedObjectSize = new Vector2(1920f, 1080f);
    [SerializeField] private Vector2 portraitResizedObjectSize = new Vector2(1080f, 1920f);

    [Header("Square Resized Object Dimensions")]
    [SerializeField] private Vector2 landscapeSquareResizedObjectSize = new Vector2(1920f, 1080f);
    [SerializeField] private Vector2 portraitSquareResizedObjectSize = new Vector2(1920f, 1920f);

    [Header("Slot Object Settings")]
    [SerializeField] private Vector3 landscapeSlotScale = Vector3.one;
    [SerializeField] private Vector3 portraitSlotScale = new Vector3(0.73f, 0.73f, 0.73f);
    [SerializeField] private Vector3 landscapeSlotPosition = Vector3.zero;
    [SerializeField] private Vector3 portraitSlotPosition = new Vector3(0f, -150f, 0f);
    [SerializeField] private SharedLayoutTransform[] sharedSlotLayout = System.Array.Empty<SharedLayoutTransform>();
    [SerializeField] private GameObject landscapeBanner;

    [Header("Logo Object Settings")]
    [SerializeField] private RectTransform logoObject;
    [SerializeField] private Vector3 landscapeLogoScale = Vector3.one;
    [SerializeField] private Vector3 portraitLogoScale = new Vector3(1.27f, 1.27f, 1.27f);
    [SerializeField] private Vector2 landscapeLogoPosition = new Vector2(0f, 355f);
    [SerializeField] private Vector2 portraitLogoPosition = new Vector2(0f, 500f);

    [Header("Info Page & Guide Settings")]
    [SerializeField] private RectTransform infoPageScrollObject;
    [SerializeField] private RectTransform guideScrollObject;

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.2f;

    private List<Tween> activeTweens = new List<Tween>();
    private bool lineWinPopupVisible;
    private bool doubleDiamondFreeSpinsActive;
    private bool doubleDiamondFreeSpinDisplayVisible;

    private void Awake()
    {
        if (orientationChange == null)
        {
            orientationChange = GetComponent<OrientationChange>();
            if (orientationChange == null)
            {
                orientationChange = Object.FindFirstObjectByType<OrientationChange>();
            }
        }
        if (canvasScaler == null && orientationChange != null)
        {
            canvasScaler = orientationChange.GetComponent<CanvasScaler>();
        }
    }

    private void OnEnable()
    {
        OrientationChange.OnOrientationChanged += HandleOrientationChange;
        if (orientationChange != null)
        {
            orientationChange.OnOrientationChangedInstance += HandleOrientationChange;
        }
    }

    private void OnDisable()
    {
        OrientationChange.OnOrientationChanged -= HandleOrientationChange;
        if (orientationChange != null)
        {
            orientationChange.OnOrientationChangedInstance -= HandleOrientationChange;
        }
    }

    private void HandleOrientationChange(OrientationChange.OrientationMode mode, int width, int height)
    {
        KillActiveTweens();

        bool isMobilePortrait = (mode == OrientationChange.OrientationMode.MobilePortrait);

        // 1. Toggle Landscape vs Portrait Panel Objects
        if (landscapePanelObject != null)
        {
            landscapePanelObject.SetActive(!isMobilePortrait);
        }
        if (portraitPanelObject != null)
        {
            portraitPanelObject.SetActive(isMobilePortrait);
        }

        // 2. Toggle Landscape vs Portrait Background Objects
        UpdateBackgroundVisibility(isMobilePortrait);

        // Toggle Wheel Landscape vs Portrait Background Objects
        if (wheelLandscapeBackground != null)
        {
            wheelLandscapeBackground.SetActive(!isMobilePortrait);
        }
        if (wheelPortraitBackground != null)
        {
            wheelPortraitBackground.SetActive(isMobilePortrait);
        }

        // 3. Update Canvas Scaler Reference Resolution
        if (canvasScaler != null)
        {
            Vector2 targetRefRes = isMobilePortrait ? portraitReferenceResolution : landscapeReferenceResolution;
            canvasScaler.referenceResolution = targetRefRes;
        }

        // 4. Resize Target RectTransforms
        Vector2 targetSize = isMobilePortrait ? portraitResizedObjectSize : landscapeResizedObjectSize;
        if (resizedObjects != null)
        {
            foreach (var rect in resizedObjects)
            {
                if (rect != null)
                {
                    if (transitionDuration > 0)
                    {
                        Tween t = rect.DOSizeDelta(targetSize, transitionDuration).SetEase(Ease.OutCubic);
                        activeTweens.Add(t);
                    }
                    else
                    {
                        rect.sizeDelta = targetSize;
                    }
                }
            }
        }

        // 4b. Resize Target RectTransforms (1920x1080 Landscape, 1920x1920 Portrait)
        Vector2 targetSquareSize = isMobilePortrait ? portraitSquareResizedObjectSize : landscapeSquareResizedObjectSize;
        if (squareResizedObjects != null)
        {
            foreach (var rect in squareResizedObjects)
            {
                if (rect != null)
                {
                    if (transitionDuration > 0)
                    {
                        Tween t = rect.DOSizeDelta(targetSquareSize, transitionDuration).SetEase(Ease.OutCubic);
                        activeTweens.Add(t);
                    }
                    else
                    {
                        rect.sizeDelta = targetSquareSize;
                    }
                }
            }
        }

        // 5. Update Slot Object Scale and Position
        foreach (var layout in sharedSlotLayout)
        {
            layout.Apply(isMobilePortrait);
        }
        UpdateLandscapeBannerVisibility(isMobilePortrait);

        if (slotObject != null)
        {
            Vector3 targetScale = isMobilePortrait ? portraitSlotScale : landscapeSlotScale;
            Vector3 targetPosition = isMobilePortrait ? portraitSlotPosition : landscapeSlotPosition;

            if (transitionDuration > 0)
            {
                Tween scaleTween = slotObject.DOScale(targetScale, transitionDuration).SetEase(Ease.OutCubic);
                Tween posTween = slotObject.DOLocalMove(targetPosition, transitionDuration).SetEase(Ease.OutCubic);
                activeTweens.Add(scaleTween);
                activeTweens.Add(posTween);
            }
            else
            {
                slotObject.localScale = targetScale;
                slotObject.localPosition = targetPosition;
            }
        }

        // 6. Update Logo Object Scale and Position
        if (logoObject != null)
        {
            Vector3 targetScale = isMobilePortrait ? portraitLogoScale : landscapeLogoScale;
            Vector2 targetPosition = isMobilePortrait ? portraitLogoPosition : landscapeLogoPosition;

            if (transitionDuration > 0)
            {
                Tween scaleTween = logoObject.DOScale(targetScale, transitionDuration).SetEase(Ease.OutCubic);
                Tween posTween = logoObject.DOAnchorPos(targetPosition, transitionDuration).SetEase(Ease.OutCubic);
                activeTweens.Add(scaleTween);
                activeTweens.Add(posTween);
            }
            else
            {
                logoObject.localScale = targetScale;
                logoObject.anchoredPosition = targetPosition;
            }
        }

        // 7. Update Info Page Scroll Object Height (1080 for Landscape, 1920 for Mobile Portrait)
        if (infoPageScrollObject != null)
        {
            float targetHeight = isMobilePortrait ? 1920f : 1080f;
            Vector2 targetScrollSize = new Vector2(infoPageScrollObject.sizeDelta.x, targetHeight);
            if (transitionDuration > 0)
            {
                Tween scrollTween = infoPageScrollObject.DOSizeDelta(targetScrollSize, transitionDuration).SetEase(Ease.OutCubic);
                activeTweens.Add(scrollTween);
            }
            else
            {
                infoPageScrollObject.sizeDelta = targetScrollSize;
            }
        }

        // 8. Update Guide Scroll Object Height (1080 for Landscape, 1920 for Mobile Portrait)
        if (guideScrollObject != null)
        {
            float targetHeight = isMobilePortrait ? 1920f : 1080f;
            Vector2 targetScrollSize = new Vector2(guideScrollObject.sizeDelta.x, targetHeight);
            if (transitionDuration > 0)
            {
                Tween scrollTween = guideScrollObject.DOSizeDelta(targetScrollSize, transitionDuration).SetEase(Ease.OutCubic);
                activeTweens.Add(scrollTween);
            }
            else
            {
                guideScrollObject.sizeDelta = targetScrollSize;
            }
        }
    }

    // Double Diamond replaces the banner with the line win popup during replay.
    internal void SetLineWinPopupVisible(bool visible)
    {
        lineWinPopupVisible = visible;
        bool isMobilePortrait = orientationChange != null &&
                                orientationChange.CurrentMode == OrientationChange.OrientationMode.MobilePortrait;
        UpdateLandscapeBannerVisibility(isMobilePortrait);
    }

    internal void SetDoubleDiamondFreeSpinPresentation(bool active, bool displayVisible = false)
    {
        doubleDiamondFreeSpinsActive = active;
        doubleDiamondFreeSpinDisplayVisible = active && displayVisible;
        bool isMobilePortrait = orientationChange != null &&
                                orientationChange.CurrentMode == OrientationChange.OrientationMode.MobilePortrait;
        UpdateBackgroundVisibility(isMobilePortrait);
        UpdateLandscapeBannerVisibility(isMobilePortrait);
    }

    private void UpdateBackgroundVisibility(bool isMobilePortrait)
    {
        bool showFreeSpinLandscape = doubleDiamondFreeSpinsActive && !isMobilePortrait && freeSpinLandscapeBackground != null;
        bool showFreeSpinPortrait = doubleDiamondFreeSpinsActive && isMobilePortrait && freeSpinPortraitBackground != null;

        if (landscapeBackground != null) landscapeBackground.SetActive(!isMobilePortrait && !showFreeSpinLandscape);
        if (portraitBackground != null) portraitBackground.SetActive(isMobilePortrait && !showFreeSpinPortrait);
        if (freeSpinLandscapeBackground != null) freeSpinLandscapeBackground.SetActive(showFreeSpinLandscape);
        if (freeSpinPortraitBackground != null) freeSpinPortraitBackground.SetActive(showFreeSpinPortrait);
    }

    private void UpdateLandscapeBannerVisibility(bool isMobilePortrait)
    {
        if (landscapeBanner != null)
        {
            landscapeBanner.SetActive(!isMobilePortrait && !lineWinPopupVisible && !doubleDiamondFreeSpinDisplayVisible);
        }
    }

    private void KillActiveTweens()
    {
        foreach (var t in activeTweens)
        {
            if (t != null && t.IsActive())
            {
                t.Kill();
            }
        }
        activeTweens.Clear();
    }
}
