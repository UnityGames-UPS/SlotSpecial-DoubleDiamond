using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SymbolInfoCard : MonoBehaviour
{
    [Header("UI Component References")]
    [SerializeField] private Image cardBgImage;
    [SerializeField] private TMP_Text infoText;

    [Header("Pointer Sprites")]
    [Tooltip("Sprite used when card is on the RIGHT side of symbol (1st & 2nd reel - pointer points left)")]
    [SerializeField] private Sprite rightSideCardSprite;
    [Tooltip("Sprite used when card is on the LEFT side of symbol (3rd, 4th, 5th reel - pointer points right)")]
    [SerializeField] private Sprite leftSideCardSprite;

    [Tooltip("Alternative inspector alias for pointer pointing left (used for right side placement)")]
    [SerializeField] private Sprite leftPointSprite;
    [Tooltip("Alternative inspector alias for pointer pointing right (used for left side placement)")]
    [SerializeField] private Sprite rightPointSprite;

    [Header("Layout & Auto-Close Settings")]
    [Tooltip("Horizontal spacing from symbol center")]
    [SerializeField] private float xSpacing = 160f;
    [Tooltip("Vertical offset adjustment")]
    [SerializeField] private float yOffset = 0f;
    [Tooltip("Auto close duration in seconds")]
    [SerializeField] private float autoCloseDuration = 1.5f;

    private RectTransform rectTransform;
    private int activeCol = -1;
    private int activeRow = -1;
    private int activeSymbolId = -1;
    private GameManager cachedGameManager;
    private Coroutine autoCloseCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private Sprite GetRightSideSprite()
    {
        if (rightSideCardSprite != null) return rightSideCardSprite;
        if (leftPointSprite != null) return leftPointSprite;
        return null;
    }

    private Sprite GetLeftSideSprite()
    {
        if (leftSideCardSprite != null) return leftSideCardSprite;
        if (rightPointSprite != null) return rightPointSprite;
        return null;
    }

    public void ShowCard(int symbolId, int colIndex, int rowIndex, RectTransform symbolRect, GameManager gameManager)
    {
        // Toggle hide if clicking the exact same symbol position while visible
        if (gameObject.activeSelf && activeCol == colIndex && activeRow == rowIndex)
        {
            HideCard();
            return;
        }

        // Cancel any active auto-close timer
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }

        activeCol = colIndex;
        activeRow = rowIndex;
        activeSymbolId = symbolId;
        cachedGameManager = gameManager;

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        // 1. Position Card based on Reel / Slot Column Index
        // Columns 0 & 1 (1st and 2nd slot): RIGHT side of symbol
        // Columns 2, 3, 4 (3rd, 4th, 5th slot): LEFT side of symbol
        Vector3 symbolWorldPos = symbolRect != null ? symbolRect.position : transform.position;
        Vector3 localPos = transform.parent != null ? transform.parent.InverseTransformPoint(symbolWorldPos) : symbolWorldPos;

        float offsetDir = (colIndex < 2) ? Mathf.Abs(xSpacing) : -Mathf.Abs(xSpacing);
        rectTransform.localPosition = new Vector3(localPos.x + offsetDir, localPos.y + yOffset, localPos.z);

        // 2. Change Sprite Based on Side
        if (cardBgImage != null)
        {
            Sprite targetSprite = (colIndex < 2) ? GetRightSideSprite() : GetLeftSideSprite();
            if (targetSprite != null)
            {
                cardBgImage.sprite = targetSprite;
            }
        }

        // 3. Setup Info Text Content & TextMeshPro Alignment Settings
        SetupCardContent(symbolId, gameManager);

        gameObject.SetActive(true);

        // 4. Start 1.5s Auto Close Timer
        autoCloseCoroutine = StartCoroutine(AutoCloseTimer(autoCloseDuration));
    }

    private IEnumerator AutoCloseTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        HideCard();
    }

    public void RefreshCard(GameManager gameManager = null)
    {
        if (!gameObject.activeSelf || activeSymbolId < 0) return;
        if (gameManager != null) cachedGameManager = gameManager;
        SetupCardContent(activeSymbolId, cachedGameManager);

        // Reset auto close timer on refresh
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
        }
        autoCloseCoroutine = StartCoroutine(AutoCloseTimer(autoCloseDuration));
    }

    private void SetupCardContent(int symbolId, GameManager gameManager)
    {
        if (infoText == null) return;

        SymbolInfo symbolInfo = null;
        if (gameManager != null && gameManager.gameConfig != null && gameManager.gameConfig.symbols != null)
        {
            symbolInfo = gameManager.gameConfig.symbols.Find(s => s.id == symbolId);
        }

        string symbolNameLower = symbolInfo != null ? (symbolInfo.name ?? "").ToLower() : "";

        // DD uses the symbol identities provided by init. CNY retains its legacy card logic below.
        if (gameManager?.gameConfig?.bonusSymbolId == 12 && symbolInfo != null)
        {
            string description = null;
            if (symbolInfo.isWild)
                description = "Substitutes for all symbols except Bonus Diamond and Jackpot Inferno.";
            else if (symbolId == gameManager.gameConfig.bonusSymbolId)
                description = "3 Bonus symbols award 6 free spins and 2× total bet.\n4 award 10 spins and 5× total bet.\n5 award 20 spins and 10× total bet.";
            else if (symbolNameLower.Contains("jackpot"))
                description = "Landing 5 or more Jackpot Inferno symbols awards the corresponding tier. The jackpot pays the tier multiplier × your total bet.";

            if (description != null)
            {
                infoText.alignment = TextAlignmentOptions.Center;
                infoText.textWrappingMode = TextWrappingModes.Normal;
                infoText.text = description;
                return;
            }
        }

        // Legacy CNY special symbols.
        bool isWild = (symbolId == 10) || (symbolInfo != null && symbolInfo.isWild) || symbolNameLower.Contains("wild");
        bool isUSpin = (symbolId == 11) || symbolNameLower.Contains("uspin");
        bool isMoneyBag = (symbolId == 12) || symbolNameLower.Contains("moneybag") || symbolNameLower.Contains("money bag");

        if (isWild || isUSpin || isMoneyBag)
        {
            // SPECIAL SYMBOL: Text alignment CENTER
            infoText.alignment = TextAlignmentOptions.Center;
            infoText.enableWordWrapping = true;
if (isUSpin)
{
    infoText.text = "3 U-Spin symbols trigger the Wheel Bonus feature.";
}
else if (isMoneyBag)
{
    infoText.text = "3 Money Bag symbols trigger the Money Bag Collect feature.";
}
else if (isWild)
{
    infoText.text = "Substitutes for all symbols except U-Spin and Money Bag.";
}
        }
        else
        {
            // NORMAL SYMBOL: Text alignment FLUSH
            infoText.alignment = TextAlignmentOptions.Flush;
            infoText.enableWordWrapping = false;

            var lines = new List<string>();
            if (gameManager != null)
            {
                foreach (var (matchCount, payout) in gameManager.GetSymbolPayoutRows(symbolInfo))
                {
                    lines.Add($"<color=#FFC700>X{matchCount}</color>   {payout:0.###}");
                }
            }

            infoText.text = string.Join("\n", lines);
        }
    }

    public void HideCard()
    {
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }

        activeCol = -1;
        activeRow = -1;
        activeSymbolId = -1;

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }
}
