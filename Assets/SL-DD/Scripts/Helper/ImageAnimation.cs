using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageAnimation : MonoBehaviour
{
    public enum ImageState
    {
        NONE,
        PLAYING,
        PAUSED
    }

    public static ImageAnimation Instance;

    public List<Sprite> textureArray;
    public Image rendererDelegate;
    public bool useSharedMaterial = true;
    public bool doLoopAnimation = true;
    
    public System.Action<int> onLoopComplete;
    private int currentLoopCount = 0;
    internal bool HasCompletedLoop => currentLoopCount > 0;
    
    [SerializeField] private bool StartOnAwake;
    [SerializeField] private bool StartonEnable;

    [HideInInspector]
    public ImageState currentAnimationState;

    private int indexOfTexture;
    private float idealFrameRate = 0.0416666679f; // ~24 fps
    private float delayBetweenAnimation;
    private bool? loopOverride;

    public float AnimationSpeed = 5f;
    public float delayBetweenLoop;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        EnsureRenderer();
        if (StartOnAwake)
        {
            StartAnimation();
        }
    }

    private void EnsureRenderer()
    {
        if (rendererDelegate == null)
        {
            rendererDelegate = GetComponent<Image>();
        }
    }

    void Start()
    {
        EnsureRenderer();
    }

    private void OnEnable()
    {
        EnsureRenderer();
        if (StartonEnable)
        {
            StartAnimation();
        }
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    private void AnimationProcess()
    {
        if (currentAnimationState != ImageState.PLAYING || textureArray == null || textureArray.Count == 0) return;

        SetTextureOfIndex();
        indexOfTexture++;

        if (indexOfTexture >= textureArray.Count)
        {
            indexOfTexture = 0;
            currentLoopCount++;
            onLoopComplete?.Invoke(currentLoopCount);
            // A loop callback can stop playback and hide this overlay.
            if (currentAnimationState != ImageState.PLAYING) return;

            if (loopOverride ?? doLoopAnimation)
            {
                Invoke(nameof(AnimationProcess), delayBetweenAnimation + delayBetweenLoop);
            }
            else
            {
                currentAnimationState = ImageState.NONE;
            }
        }
        else
        {
            Invoke(nameof(AnimationProcess), delayBetweenAnimation);
        }
    }

    public void StartAnimation()
    {
        // Legacy playback keeps its existing speed calculation.
        float frameInterval = idealFrameRate * (textureArray?.Count ?? 0) / AnimationSpeed;
        StartAnimationWithInterval(frameInterval);
    }

    // Double Diamond uses a fixed frame rate across symbol sequences.
    internal bool TryStartAtFrameRate(float framesPerSecond, bool? loopOverride = null)
    {
        return isActiveAndEnabled &&
            StartAnimationWithInterval(1f / Mathf.Max(1f, framesPerSecond), loopOverride);
    }

    private bool StartAnimationWithInterval(float frameInterval, bool? loopOverride = null)
    {
        if (textureArray == null || textureArray.Count == 0) return false;

        EnsureRenderer();
        if (rendererDelegate == null) return false;

        CancelInvoke(nameof(AnimationProcess));
        indexOfTexture = 0;
        currentLoopCount = 0;
        currentAnimationState = ImageState.PLAYING;
        this.loopOverride = loopOverride;

        RevertToInitialState();

        delayBetweenAnimation = frameInterval > 0 ? frameInterval : 0.05f;
        Invoke(nameof(AnimationProcess), delayBetweenAnimation);
        return true;
    }

    public void PlayAnimation()
    {
        StartAnimation();
    }

    public void Play()
    {
        StartAnimation();
    }

    public void PauseAnimation()
    {
        if (currentAnimationState == ImageState.PLAYING)
        {
            CancelInvoke(nameof(AnimationProcess));
            currentAnimationState = ImageState.PAUSED;
        }
    }

    public void ResumeAnimation()
    {
        if (currentAnimationState == ImageState.PAUSED && !IsInvoking(nameof(AnimationProcess)))
        {
            Invoke(nameof(AnimationProcess), delayBetweenAnimation);
            currentAnimationState = ImageState.PLAYING;
        }
    }

    public void StopAnimation()
    {
        if (currentAnimationState != ImageState.NONE)
        {
            EnsureRenderer();
            if (rendererDelegate != null && textureArray != null && textureArray.Count > 0)
            {
                rendererDelegate.sprite = textureArray[0];
            }
            CancelInvoke(nameof(AnimationProcess));
            currentAnimationState = ImageState.NONE;
            currentLoopCount = 0;
        }
    }

    public void RevertToInitialState()
    {
        indexOfTexture = 0;
        SetTextureOfIndex();
    }

    private void SetTextureOfIndex()
    {
        if (textureArray == null || textureArray.Count == 0 || indexOfTexture < 0 || indexOfTexture >= textureArray.Count) return;

        EnsureRenderer();
        if (rendererDelegate != null)
        {
            rendererDelegate.sprite = textureArray[indexOfTexture];
        }
    }
}
