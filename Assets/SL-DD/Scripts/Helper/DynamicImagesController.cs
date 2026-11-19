using System.Collections;
using UnityEngine;

// Double Diamond alternates its informational banners while the parent is visible.
public class DynamicImagesController : MonoBehaviour
{
    [SerializeField] private GameObject freeSpinsBanner;
    [SerializeField] private GameObject jackpotTriggerBanner;
    [SerializeField, Min(0.1f)] private float switchInterval = 3f;

    private Coroutine switchingRoutine;

    private void OnEnable()
    {
        switchingRoutine = StartCoroutine(SwitchBanners());
    }

    private void OnDisable()
    {
        if (switchingRoutine != null)
        {
            StopCoroutine(switchingRoutine);
            switchingRoutine = null;
        }
    }

    private IEnumerator SwitchBanners()
    {
        bool showFreeSpins = true;
        while (true)
        {
            if (freeSpinsBanner != null)
                freeSpinsBanner.SetActive(showFreeSpins);
            if (jackpotTriggerBanner != null)
                jackpotTriggerBanner.SetActive(!showFreeSpins);

            yield return new WaitForSeconds(Mathf.Max(0.1f, switchInterval));
            showFreeSpins = !showFreeSpins;
        }
    }
}
