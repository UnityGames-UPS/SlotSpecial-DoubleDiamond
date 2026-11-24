#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

// DD test scene controls for the total-win popup and jackpot payout animations.
public sealed class TotalWinPreview : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private UIManager.TotalWinTitle title;
    [SerializeField, Min(0f)] private float amount = 25f;

    [Header("Jackpot Animation Preview")]
    [SerializeField] private ImageAnimation jackpotPanelAnimation;
    [SerializeField] private ImageAnimation[] jackpotRowAnimations = new ImageAnimation[8];
    [SerializeField, Tooltip("Select a jackpot row from 1 at the top to 8 at the bottom.")]
    private int jackpotRow = 1;

    private const float JackpotPreviewFramesPerSecond = 24f;
    private Coroutine jackpotPanelPlayback;
    private Coroutine jackpotRowPlayback;
    private ImageAnimation activeJackpotRow;

    public bool IsOpen { get; private set; }

    private void OnValidate()
    {
        jackpotRow = Mathf.Clamp(jackpotRow, 1, 8);
    }

    public void OpenPreview()
    {
        if (!Application.isPlaying || IsOpen || uiManager == null)
            return;

        if (!uiManager.TryOpenDDTotalWinPreview(title, amount))
        {
            Debug.LogWarning("Open the total win preview when the game is idle and the popup is closed.", this);
            return;
        }

        IsOpen = true;
    }

    public void ClosePreview()
    {
        if (!IsOpen)
            return;

        IsOpen = false;
        if (uiManager != null)
            uiManager.CloseDDTotalWinPreview();
    }

    public void PlayJackpotPanelAnimation()
    {
        if (Application.isPlaying && isActiveAndEnabled && jackpotPanelAnimation != null)
            ReplayJackpotAnimation(jackpotPanelAnimation, ref jackpotPanelPlayback);
    }

    public void PlaySelectedJackpotRowAnimation()
    {
        int index = jackpotRow - 1;
        if (!Application.isPlaying || !isActiveAndEnabled || jackpotRowAnimations == null ||
            index < 0 || index >= jackpotRowAnimations.Length || jackpotRowAnimations[index] == null)
            return;

        StopJackpotPreview(activeJackpotRow, ref jackpotRowPlayback);
        activeJackpotRow = jackpotRowAnimations[index];
        ReplayJackpotAnimation(activeJackpotRow, ref jackpotRowPlayback);
    }

    private void ReplayJackpotAnimation(ImageAnimation animation, ref Coroutine playback)
    {
        StopJackpotPreview(animation, ref playback);
        animation.gameObject.SetActive(true);
        if (animation.TryStartAtFrameRate(JackpotPreviewFramesPerSecond))
            playback = StartCoroutine(HideAfterJackpotAnimation(animation));
        else
            animation.gameObject.SetActive(false);
    }

    private static IEnumerator HideAfterJackpotAnimation(ImageAnimation animation)
    {
        yield return new WaitUntil(() => animation == null ||
            animation.currentAnimationState != ImageAnimation.ImageState.PLAYING);

        // Keep the final frame visible for one frame interval before hiding the overlay.
        yield return new WaitForSeconds(1f / JackpotPreviewFramesPerSecond);
        if (animation != null)
            animation.gameObject.SetActive(false);
    }

    private void StopJackpotPreview(ImageAnimation animation, ref Coroutine playback)
    {
        if (playback == null)
            return;

        StopCoroutine(playback);
        playback = null;
        if (animation != null)
            animation.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        ClosePreview();
        StopJackpotPreview(jackpotPanelAnimation, ref jackpotPanelPlayback);
        StopJackpotPreview(activeJackpotRow, ref jackpotRowPlayback);
        activeJackpotRow = null;
    }
}
#endif
