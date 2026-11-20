#if UNITY_EDITOR
using UnityEngine;

// DD test scene controls for the total-win popup.
public sealed class TotalWinPreview : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;

    public bool IsOpen { get; private set; }

    public void OpenPreview()
    {
        if (!Application.isPlaying || IsOpen || uiManager == null)
            return;

        if (!uiManager.TryOpenDDTotalWinPreview())
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

    private void OnDisable()
    {
        ClosePreview();
    }
}
#endif
