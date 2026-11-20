using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// DD uses a separate overlay so win-line cleanup cannot interrupt Inferno landings.
internal sealed class DDInfernoLandingCell : MonoBehaviour
{
    [SerializeField] private Image symbolImage;
    [SerializeField] private ImageAnimation landingAnimation;
    [SerializeField] private TMP_Text numberText;

    private Coroutine finishAnimationRoutine;
    private bool restoreSymbolImage;

    internal void Show(int number, List<Sprite> frames, float framesPerSecond)
    {
        Hide();
        gameObject.SetActive(true);
        if (numberText != null)
            ShowNumber(number);

        if (landingAnimation == null || frames == null || frames.Count == 0)
            return;

        landingAnimation.textureArray = frames;
        landingAnimation.doLoopAnimation = false;
        landingAnimation.onLoopComplete = _ => finishAnimationRoutine = StartCoroutine(FinishAnimation());
        landingAnimation.gameObject.SetActive(true);
        if (!landingAnimation.TryStartAtFrameRate(framesPerSecond))
        {
            StopLandingAnimation();
            return;
        }

        if (symbolImage != null && symbolImage.enabled)
        {
            restoreSymbolImage = true;
            symbolImage.enabled = false;
        }
    }

    private void ShowNumber(int number)
    {
        string digits = number.ToString(CultureInfo.InvariantCulture);
        if (numberText.spriteAsset == null)
        {
            numberText.text = digits;
            return;
        }

        // DD Inferno's sprite character table puts digits 0–9 at indices 0–9.
        var markup = new StringBuilder(digits.Length * 16);
        foreach (char digit in digits)
            markup.Append("<sprite index=").Append(digit).Append('>');
        numberText.text = markup.ToString();
    }

    internal void Hide()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator FinishAnimation()
    {
        // Keep the last frame visible for one render before restoring the static symbol.
        yield return null;
        finishAnimationRoutine = null;
        StopLandingAnimation();
    }

    private void StopLandingAnimation()
    {
        if (landingAnimation != null)
        {
            landingAnimation.onLoopComplete = null;
            landingAnimation.StopAnimation();
            landingAnimation.gameObject.SetActive(false);
        }

        if (restoreSymbolImage && symbolImage != null)
            symbolImage.enabled = true;
        restoreSymbolImage = false;
    }

    private void OnDisable()
    {
        if (finishAnimationRoutine != null)
        {
            StopCoroutine(finishAnimationRoutine);
            finishAnimationRoutine = null;
        }

        StopLandingAnimation();
        if (numberText != null)
            numberText.text = string.Empty;
    }
}
