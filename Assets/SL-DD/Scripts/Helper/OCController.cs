using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class OCController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private OrientationChange orientationChange;
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private Transform slotObject;
    [SerializeField] private List<RectTransform> resizedObjects = new List<RectTransform>();

    [Header("Panel Toggle Settings")]
    [SerializeField] private GameObject landscapePanelObject;
    [SerializeField] private GameObject portraitPanelObject;

    [Header("Background Toggle Settings")]
    [SerializeField] private GameObject landscapeBackground;
    [SerializeField] private GameObject portraitBackground;
    [SerializeField] private GameObject freeSpinLandscapeBackground;
    [SerializeField] private GameObject freeSpinPortraitBackground;

    [Header("Canvas Scaler Resolutions")]
    [SerializeField] private Vector2 landscapeReferenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] private Vector2 portraitReferenceResolution = new Vector2(1080f, 1920f);

    [Header("Resized Object Dimensions")]
    [SerializeField] private Vector2 landscapeResizedObjectSize = new Vector2(1920f, 1080f);
    [SerializeField] private Vector2 portraitResizedObjectSize = new Vector2(1080f, 1920f);

    [Header("Slot Object Settings")]
    [SerializeField] private Vector3 landscapeSlotScale = Vector3.one;
    [SerializeField] private Vector3 portraitSlotScale = new Vector3(0.73f, 0.73f, 0.73f);
    [SerializeField] private Vector3 landscapeSlotPosition = Vector3.zero;
    [SerializeField] private Vector3 portraitSlotPosition = new Vector3(0f, -150f, 0f);

    [Header("Bonus Spin Popup Settings")]
    [SerializeField] private RectTransform bonusSpinPopup;
    [SerializeField] private Vector2 landscapePopupPosition;
    [SerializeField] private Vector3 landscapePopupScale = Vector3.one;
    [SerializeField] private Vector2 portraitPopupPosition;
    [SerializeField] private Vector3 portraitPopupScale = Vector3.one;

    [Header("Double Diamond Dynamic Images")]
    // The same banners serve both orientations.
    [FormerlySerializedAs("landscapeBanner")]
    [SerializeField] private GameObject dynamicImages;

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

        // The free-spin trigger popup uses its own anchors outside the slot hierarchy.
        if (bonusSpinPopup != null)
        {
            bonusSpinPopup.anchoredPosition = isMobilePortrait ? portraitPopupPosition : landscapePopupPosition;
            bonusSpinPopup.localScale = isMobilePortrait ? portraitPopupScale : landscapePopupScale;
        }
        UpdateDynamicImagesVisibility();

        // 5. Update Slot Object Scale and Position

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

        // 6. Update Info Page Scroll Object Height (1080 for Landscape, 1920 for Mobile Portrait)
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

        // 7. Update Guide Scroll Object Height (1080 for Landscape, 1920 for Mobile Portrait)
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
        UpdateDynamicImagesVisibility();
    }

    internal void SetDoubleDiamondFreeSpinPresentation(bool active, bool displayVisible = false)
    {
        doubleDiamondFreeSpinsActive = active;
        doubleDiamondFreeSpinDisplayVisible = active && displayVisible;
        bool isMobilePortrait = orientationChange != null &&
                                orientationChange.CurrentMode == OrientationChange.OrientationMode.MobilePortrait;
        UpdateBackgroundVisibility(isMobilePortrait);
        UpdateDynamicImagesVisibility();
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

    private void UpdateDynamicImagesVisibility()
    {
        if (dynamicImages != null)
        {
            dynamicImages.SetActive(!lineWinPopupVisible && !doubleDiamondFreeSpinDisplayVisible);
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
