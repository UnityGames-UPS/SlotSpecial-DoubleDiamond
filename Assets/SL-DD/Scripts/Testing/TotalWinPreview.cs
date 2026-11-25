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
    private ImageAnimation jackpotPanelPreview;
    private ImageAnimation jackpotRowPreview;

    public bool IsOpen => uiManager != null && uiManager.IsDDTotalWinPreviewOpen;
    private bool CanPlayJackpotPreview => Application.isPlaying && isActiveAndEnabled &&
        (uiManager == null || uiManager.CanPreviewDDPresentation);

    private void OnValidate()
    {
        jackpotRow = Mathf.Clamp(jackpotRow, 1, 8);
    }

    public void OpenPreview()
    {
        if (!Application.isPlaying || IsOpen || uiManager == null)
            return;

        if (!uiManager.TryOpenDDTotalWinPreview(title, amount))
            Debug.LogWarning("Open the total win preview when the game is idle and the popup is closed.", this);
    }

    public void ClosePreview()
    {
        if (uiManager != null)
            uiManager.CloseDDTotalWinPreview();
    }

    public void PlayJackpotPanelAnimation()
    {
        if (CanPlayJackpotPreview && jackpotPanelAnimation != null)
            ReplayJackpotAnimation(jackpotPanelAnimation, ref jackpotPanelPreview, ref jackpotPanelPlayback);
    }

    public void PlaySelectedJackpotRowAnimation()
    {
        int index = jackpotRow - 1;
        if (!CanPlayJackpotPreview || jackpotRowAnimations == null ||
            index < 0 || index >= jackpotRowAnimations.Length || jackpotRowAnimations[index] == null)
            return;

        ReplayJackpotAnimation(jackpotRowAnimations[index], ref jackpotRowPreview, ref jackpotRowPlayback);
    }

    private void ReplayJackpotAnimation(ImageAnimation source, ref ImageAnimation preview, ref Coroutine playback)
    {
        StopJackpotPreview(ref preview, ref playback);
        // The Inspector preview owns a copy so its cleanup cannot hide a gameplay overlay.
        preview = Instantiate(source, source.transform.parent, false);
        preview.gameObject.SetActive(true);
        if (preview.rendererDelegate != null)
            preview.rendererDelegate.raycastTarget = false;

        if (preview.TryStartAtFrameRate(JackpotPreviewFramesPerSecond, loopOverride: false))
            playback = StartCoroutine(HideAfterJackpotAnimation(preview));
        else
            ReleaseJackpotPreview(preview);
    }

    private IEnumerator HideAfterJackpotAnimation(ImageAnimation animation)
    {
        yield return new WaitUntil(() => !CanPlayJackpotPreview || animation == null ||
            animation.currentAnimationState != ImageAnimation.ImageState.PLAYING);

        // Keep the final frame visible for one frame interval before hiding the overlay.
        if (CanPlayJackpotPreview)
            yield return new WaitForSeconds(1f / JackpotPreviewFramesPerSecond);
        ReleaseJackpotPreview(animation);
    }

    private void StopJackpotPreview(ref ImageAnimation preview, ref Coroutine playback)
    {
        if (playback != null)
            StopCoroutine(playback);
        playback = null;
        ReleaseJackpotPreview(preview);
        preview = null;
    }

    private static void ReleaseJackpotPreview(ImageAnimation preview)
    {
        if (preview == null)
            return;

        preview.gameObject.SetActive(false);
        Destroy(preview.gameObject);
    }

    private void OnDisable()
    {
        ClosePreview();
        StopJackpotPreview(ref jackpotPanelPreview, ref jackpotPanelPlayback);
        StopJackpotPreview(ref jackpotRowPreview, ref jackpotRowPlayback);
    }
}
#endif
