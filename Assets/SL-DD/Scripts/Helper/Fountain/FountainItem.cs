using System;
using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public sealed class FountainItem : MonoBehaviour
{
    private RectTransform rect;
    private CanvasGroup fade;
    private ImageAnimation flipAnimation;
    private Sequence flight;

    internal static FountainItem CreateInstance(RectTransform prefab, Transform parent)
    {
        RectTransform instance = Instantiate(prefab, parent, false);
        if (!instance.TryGetComponent(out FountainItem item))
            item = instance.gameObject.AddComponent<FountainItem>();

        return item;
    }

    private void Awake()
    {
        rect = (RectTransform)transform;
        fade = GetComponent<CanvasGroup>();
        fade.interactable = false;
        fade.blocksRaycasts = false;
        flipAnimation = GetComponentInChildren<ImageAnimation>(true);
    }

    internal void Launch(Vector3 localOrigin, float height, float drift, float riseTime, float fallTime,
        float dropBelowOrigin, float startRotation, float fadeInTime, Action<FountainItem> onLanded)
    {
        float fadeInDuration = Mathf.Clamp(fadeInTime, 0f, riseTime);
        BeginMotion(localOrigin, fadeInDuration > 0f ? 0f : 1f);
        rect.localRotation = Quaternion.Euler(0f, 0f, startRotation);
        Vector2 start = rect.anchoredPosition;

        flight = DOTween.Sequence()
            .Append(rect.DOAnchorPosY(start.y + height, riseTime).SetEase(Ease.OutQuad))
            .Append(rect.DOAnchorPosY(start.y - dropBelowOrigin, fallTime).SetEase(Ease.InQuad))
            .Insert(0f, rect.DOAnchorPosX(start.x + drift, riseTime + fallTime).SetEase(Ease.Linear));

        if (fadeInDuration > 0f)
            flight.Insert(0f, fade.DOFade(1f, fadeInDuration).SetEase(Ease.Linear));

        flight.Insert(riseTime, fade.DOFade(0f, fallTime).SetEase(Ease.Linear));
        RecycleOnComplete(onLanded);
    }

    internal void LaunchRipple(Vector3 start, Vector3 end, float duration, float fadeOutTime,
        Action<FountainItem> onComplete)
    {
        BeginMotion(start, 1f);
        flight = DOTween.Sequence()
            .Append(rect.DOLocalMove(end, duration).SetEase(Ease.Linear));

        float fadeDuration = Mathf.Clamp(fadeOutTime, 0f, duration);
        if (fadeDuration > 0f)
            flight.Insert(duration - fadeDuration, fade.DOFade(0f, fadeDuration).SetEase(Ease.Linear));

        RecycleOnComplete(onComplete);
    }

    private void BeginMotion(Vector3 localStart, float opacity)
    {
        flight?.Kill();
        rect.localPosition = localStart;
        fade.alpha = opacity;

        if (flipAnimation != null && flipAnimation.isActiveAndEnabled &&
            flipAnimation.currentAnimationState != ImageAnimation.ImageState.PLAYING)
        {
            flipAnimation.StartAnimation();
        }
    }

    private void RecycleOnComplete(Action<FountainItem> onComplete)
    {
        flight.OnComplete(() =>
        {
            flight = null;
            onComplete(this);
        });
    }

    private void OnDisable()
    {
        flight?.Kill();
        flight = null;
    }
}
