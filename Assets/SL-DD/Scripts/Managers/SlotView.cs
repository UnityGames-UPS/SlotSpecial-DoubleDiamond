using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SlotView : MonoBehaviour
{
    private const int DDInfernoSymbolId = 13;
    private const int FirstVisibleReelImage = 2;

    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Header("Symbol Sprites - Assign by Name")]
    [SerializeField] private Sprite spriteWildStar;           // ID: 0
    [SerializeField] private Sprite spriteRedJwell;            // ID: 1
    [SerializeField] private Sprite spriteBlueJwell;         // ID: 2
    [SerializeField] private Sprite spriteGoldBar;              // ID: 3
    [SerializeField] private Sprite spriteGoldCoin;                 // ID: 4
    [SerializeField] private Sprite spriteSeven;                 // ID: 5
    [SerializeField] private Sprite spriteBar;                 // ID: 6
    [SerializeField] private Sprite spriteLemon;                 // ID: 7
    [SerializeField] private Sprite spriteCheryy;               // ID: 8
    // Double Diamond server IDs. The seven variants use temporary artwork.
    [SerializeField] private Sprite goldCoin_2;        // ID 9: Fiery Seven placeholder.
    [SerializeField] private Sprite starCoin;          // ID 10: Triple Seven placeholder.
    [SerializeField] private Sprite spriteSingleSeven; // ID 11: Single Seven placeholder.
    [SerializeField] private Sprite spriteBonusDiam;   // ID 12: Bonus Diamond.
    [SerializeField] private Sprite JackpotInferno;    // ID 13: Jackpot Inferno.

    // Internal array built from named sprites
    private Sprite[] symbolSprites;

    [System.Serializable]
    private class SymbolWinAnimation
    {
        public int symbolId;
        public List<Sprite> frames;
    }

    [Header("Double Diamond Win Animations")]
    [SerializeField] private SymbolWinAnimation[] doubleDiamondWinAnimations = System.Array.Empty<SymbolWinAnimation>();
    [SerializeField, Min(1f)] private float doubleDiamondWinFramesPerSecond = 24f;
    private List<Sprite>[] doubleDiamondAnimationSpriteArrays;

    [Header("Bonus Reel Outline")]
    [SerializeField] private ImageAnimation[] bonusReelOutlines = System.Array.Empty<ImageAnimation>();
    [SerializeField] private List<Sprite> bonusReelOutlineFrames = new List<Sprite>();
    [SerializeField, Min(1f)] private float bonusReelOutlineFramesPerSecond = 24f;
    [SerializeField, Min(0f)] private float bonusReelOutlineFadeDuration = 0.25f;
    private bool bonusReelOutlinesSuppressed;
    private Coroutine bonusReelOutlineFadeRoutine;

    [Header("DD Inferno Landing")]
    [SerializeField] private List<Sprite> ddInfernoLandingFrames = new List<Sprite>();
    [SerializeField, Min(1f)] private float ddInfernoLandingFramesPerSecond = 24f;
    private readonly HashSet<DDInfernoLandingCell> activeDDInfernoCells = new HashSet<DDInfernoLandingCell>();

    [Header("Legacy CNY Win Animations")]
    [SerializeField] private List<Sprite> animSpritesUSpin; // Legacy U-Spin, ID 11.
    private List<Sprite>[] animationSpriteArrays;

    [Header("Reel Containers")]
    [SerializeField] private Transform[] reelTransforms;

    [Header("Reel Images - 8 images per reel")]
    [SerializeField] private List<ReelImages> reelImagesList;
    [SerializeField] private float reelSymbolScale = 0.85f;

    [Header("Spin Settings")]
    [SerializeField] private float symbolHeight = 100f;
    [SerializeField] private float spinSpeed = 2000f;
    [SerializeField] private float reelStartStagger = 0.08f;
    [SerializeField] private float reelStopStagger = 0.12f;

    [Header("Animation Settings - Casino Style")]
    [SerializeField] private float anticipationUpDistance = 20f;
    [SerializeField] private float anticipationUpDuration = 0.12f;

    [Header("Win Animation Settings")]
    [SerializeField] private float winPopDuration = 0.4f;
    [SerializeField] private int winPopRepeat = 3;


    [Header("Stop Animation Settings")]
    [SerializeField] private float stopOvershootDistance = 50f;
    [SerializeField] private float stopOvershootDuration = 0.20f;
    [SerializeField] private float stopSettleDuration = 0.30f;

    [Header("Quick Spin Settings")]
    [SerializeField] private float quickStopStagger = 0.06f;
    [SerializeField] private float quickStopOvershoot = 20f;
    [SerializeField] private float quickStopDuration = 0.2f;
    [SerializeField] private int minSpinCyclesBeforeStop = 3;


    [Header("Win Animation Settings")]
    [SerializeField] private float winAnimationDuration = 3.0f; // Total duration each win symbol animation plays
    [SerializeField] private float winSymbolLoopDuration = 1.5f;
    [SerializeField] private int winSymbolLoopCount = 3;
    [Tooltip("Delay between enabling winBox overlay and starting the ImageAnimation - for sync timing")]
    [SerializeField] private float winLineBoxToAnimationDelay = 0.05f;

    [Header("Phase 1 Total Win Presentation")]
    [SerializeField] private TMPro.TMP_Text phase1TotalWinText;

    [Header("Win Box Overlays — Col 0..4  (each has 3 rows: 0=top .. 2=bottom)")]
    [SerializeField] private ColumnOverlays[] winBoxColumns = new ColumnOverlays[5];

    [Header("Win Animation Objects — Col 0..4  (each has 3 rows, contains ImageAnimation component)")]
    [Tooltip("GameObject references for win animations. Each should have an ImageAnimation component attached.")]
    [SerializeField] private ColumnOverlays[] winAnimationColumns = new ColumnOverlays[5];


    [Header("Symbol Info Card")]
    [SerializeField] private SymbolInfoCard symbolInfoCard;


    private float middlePosition = 0f;
    private float cycleDistance;


    private List<Tween> spinTweens = new List<Tween>();
    private List<Tween> winTweens = new List<Tween>();
    private List<int> reelCycleCount = new List<int>();
    private Coroutine winAnimationCoroutine;


    internal List<List<int>> currentDisplayMatrix;

    private bool isSpinning;

    #region Initialization

    private void Awake()
    {
        BuildSymbolSpriteArray();
        InitializeReels();
    }

    private void Start()
    {
        if (symbolSprites == null || symbolSprites.Length == 0)
        {
            BuildSymbolSpriteArray();
        }
        DisableAllOverlays();
        SetupSymbolButtons();
    }

    private void DisableAllOverlays()
    {
        ClearDDInfernoLandings();
        DisableColumns(winBoxColumns);
        DisableColumns(winAnimationColumns);
        HidePhase1TotalWinText();
        if (symbolInfoCard) symbolInfoCard.HideCard();
    }

    internal void SetupSymbolButtons()
    {
        if (reelImagesList == null) return;

        const int visibleStartIndex = 2;
        int rowCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;
        for (int col = 0; col < reelImagesList.Count; col++)
        {
            var reel = reelImagesList[col];
            if (reel == null || reel.images == null) continue;
            for (int row = 0; row < rowCount; row++)
            {
                int imageIndex = visibleStartIndex + row;
                if (imageIndex < reel.images.Count && reel.images[imageIndex] != null)
                {
                    Image img = reel.images[imageIndex];
                    SymbolButtonHandler btnHandler = img.GetComponent<SymbolButtonHandler>();
                    if (btnHandler == null)
                    {
                        btnHandler = img.gameObject.AddComponent<SymbolButtonHandler>();
                    }
                    btnHandler.Init(col, row, this);
                }
            }
        }
    }

    internal void HideSymbolInfoCard()
    {
        if (symbolInfoCard != null) symbolInfoCard.HideCard();
    }

    internal void OnBetChanged()
    {
        if (symbolInfoCard != null && symbolInfoCard.gameObject.activeSelf)
        {
            symbolInfoCard.RefreshCard(gameManager);
        }
    }

    internal void OnSymbolClicked(int col, int row, RectTransform symbolRect)
    {
        if (isSpinning)
        {
            if (symbolInfoCard != null) symbolInfoCard.HideCard();
            return;
        }

        if (currentDisplayMatrix == null || col >= currentDisplayMatrix.Count || row >= currentDisplayMatrix[col].Count)
        {
            return;
        }

        int symbolId = currentDisplayMatrix[col][row];
        if (symbolInfoCard != null)
        {
            symbolInfoCard.ShowCard(symbolId, col, row, symbolRect, gameManager);
        }
    }

    private static void DisableColumns(ColumnOverlays[] cols)
    {
        if (cols == null) return;
        foreach (var col in cols)
            if (col?.rows != null)
                foreach (var go in col.rows)
                    if (go) go.SetActive(false);
    }

    private static GameObject WinBox(ColumnOverlays[] cols, int col, int row)
        => (col >= 0 && col < cols?.Length && cols[col]?.rows != null && row >= 0 && row < cols[col].rows.Length)
            ? cols[col].rows[row] : null;

    private void BuildSymbolSpriteArray()
    {
        // Index sprites by the Double Diamond symbol IDs returned by the server.
        symbolSprites = new Sprite[14];
        symbolSprites[0] = spriteWildStar;
        symbolSprites[1] = spriteRedJwell;
        symbolSprites[2] = spriteBlueJwell;
        symbolSprites[3] = spriteGoldBar;
        symbolSprites[4] = spriteGoldCoin;
        symbolSprites[5] = spriteSeven;
        symbolSprites[6] = spriteBar;
        symbolSprites[7] = spriteLemon;
        symbolSprites[8] = spriteCheryy;
        symbolSprites[9] = goldCoin_2;
        symbolSprites[10] = starCoin;
        symbolSprites[11] = spriteSingleSeven;
        symbolSprites[12] = spriteBonusDiam;
        symbolSprites[DDInfernoSymbolId] = JackpotInferno;

        // Validate
        for (int i = 0; i < symbolSprites.Length; i++)
        {
            if (symbolSprites[i] == null)
            {
                Debug.LogError($"[SlotView] Symbol sprite at index {i} is not assigned in inspector!");
            }
        }

        animationSpriteArrays = new List<Sprite>[symbolSprites.Length];
        // Legacy CNY U-Spin frames at ID 11. Double Diamond frame bindings are separate work.
        animationSpriteArrays[11] = animSpritesUSpin;

        // Double Diamond leaves IDs without imported frames static.
        doubleDiamondAnimationSpriteArrays = new List<Sprite>[symbolSprites.Length];
        if (doubleDiamondWinAnimations == null) return;

        foreach (var animation in doubleDiamondWinAnimations)
        {
            if (animation?.frames == null || animation.symbolId < 0 || animation.symbolId >= doubleDiamondAnimationSpriteArrays.Length)
                continue;

            doubleDiamondAnimationSpriteArrays[animation.symbolId] = animation.frames.FindAll(frame => frame != null);
        }
    }

    private void InitializeReels()
    {
        cycleDistance = symbolHeight;
        middlePosition = 0f;

        int rowCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;

        currentDisplayMatrix = new List<List<int>>();
        for (int col = 0; col < 5; col++)
        {
            var defaultCol = new List<int>();
            for (int r = 0; r < rowCount; r++)
            {
                defaultCol.Add(0);
            }
            currentDisplayMatrix.Add(defaultCol);
            reelCycleCount.Add(0);
        }

        if (reelImagesList == null) return;

        Vector3 scale = Vector3.one * reelSymbolScale;
        foreach (var reel in reelImagesList)
        {
            if (reel?.images == null) continue;

            foreach (var image in reel.images)
            {
                if (image != null)
                {
                    image.transform.localScale = scale;
                }
            }
        }
    }

    internal void SetInitialMatrix(List<List<int>> matrix)
    {
        if (matrix == null || matrix.Count != 5) return;

        int rowCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;

        for (int col = 0; col < 5; col++)
        {
            if (matrix[col].Count != rowCount) return;
        }

        ClearDDInfernoLandings();
        currentDisplayMatrix = matrix;

        for (int col = 0; col < 5; col++)
        {
            SetReelSymbols(col, matrix[col], true);
        }
    }

    #endregion

    #region Symbol Display

    private void SetReelSymbols(int columnIndex, List<int> visibleSymbolIds, bool isInitial = false)
    {
        if (columnIndex >= reelImagesList.Count)
        {
            Debug.LogError($"SetReelSymbols: Invalid column index {columnIndex}, max is {reelImagesList.Count - 1}");
            return;
        }

        int rowCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;

        if (visibleSymbolIds == null || visibleSymbolIds.Count != rowCount)
        {
            Debug.LogError($"SetReelSymbols: Invalid visibleSymbolIds count {visibleSymbolIds?.Count}, expected {rowCount}");
            return;
        }

        var reel = reelImagesList[columnIndex];

        if (reel.images == null || reel.images.Count != 8)
        {
            Debug.LogError($"SetReelSymbols: Reel {columnIndex} has invalid image count {reel.images?.Count}, expected 8");
            return;
        }

        int visibleStartIndex = 2;
        for (int row = 0; row < rowCount; row++)
        {
            int imageIndex = visibleStartIndex + row;
            if (imageIndex < reel.images.Count)
            {
                int symbolId = visibleSymbolIds[row];
                reel.images[imageIndex].sprite = GetSymbolSprite(symbolId);
            }
        }

        for (int i = 0; i < visibleStartIndex; i++)
        {
            reel.images[i].sprite = GetSymbolSprite(Random.Range(0, 10));
        }

        for (int i = visibleStartIndex + rowCount; i < reel.images.Count; i++)
        {
            reel.images[i].sprite = GetSymbolSprite(Random.Range(0, 10));
        }

        if (isInitial && reelTransforms[columnIndex] != null)
        {
            reelTransforms[columnIndex].localPosition = new Vector3(
                reelTransforms[columnIndex].localPosition.x,
                middlePosition,
                0
            );
        }
    }

    private Sprite GetSymbolSprite(int symbolId)
    {
        if (symbolSprites == null || symbolSprites.Length == 0)
        {
            BuildSymbolSpriteArray();
        }

        if (symbolSprites == null || symbolSprites.Length == 0)
        {
            Debug.LogError("[SlotView] symbolSprites array is null or empty after BuildSymbolSpriteArray!");
            return null;
        }

        if (symbolId < 0 || symbolId >= symbolSprites.Length)
        {
            Debug.LogWarning($"[SlotView] Invalid symbolId {symbolId}, using default sprite 0. Total sprites: {symbolSprites.Length}");
            return symbolSprites[0];
        }

        if (symbolSprites[symbolId] == null)
        {
            Debug.LogError($"[SlotView] Symbol sprite for ID {symbolId} is null!");
            return symbolSprites[0];
        }

        return symbolSprites[symbolId];
    }

    #endregion

    #region Spin Animation

    internal void StartSpin()
    {
        if (isSpinning) return;

        if (symbolInfoCard != null) symbolInfoCard.HideCard();

        isSpinning = true;
        KillAllTweens();

        FadeBonusReelOutlines();
        bonusReelOutlinesSuppressed = false;
        DisableAllOverlays();

        for (int i = 0; i < reelCycleCount.Count; i++)
        {
            reelCycleCount[i] = 0;
        }

        for (int col = 0; col < 5; col++)
        {
            StartReelCycleWithDelay(col, col * reelStartStagger);
        }
    }

    private void StartReelCycleWithDelay(int columnIndex, float delay)
    {
        if (columnIndex >= reelTransforms.Length) return;

        Transform slotTransform = reelTransforms[columnIndex];

        Sequence startSequence = DOTween.Sequence();

        if (delay > 0)
        {
            startSequence.AppendInterval(delay);
        }

        startSequence.Append(
            slotTransform.DOLocalMoveY(middlePosition + anticipationUpDistance, anticipationUpDuration)
                .SetEase(Ease.OutQuad)
        );

        startSequence.Append(
            slotTransform.DOLocalMoveY(middlePosition, anticipationUpDuration * 0.5f)
                .SetEase(Ease.InQuad)
        );

        startSequence.OnComplete(() => {
            if (isSpinning)
            {
                StartReelCycle(columnIndex);
            }
        });

        startSequence.Play();

        if (spinTweens.Count <= columnIndex)
            spinTweens.Add(startSequence);
        else
            spinTweens[columnIndex] = startSequence;
    }

    private void StartReelCycle(int columnIndex)
    {
        if (columnIndex >= reelTransforms.Length) return;
        if (!isSpinning) return;

        Transform slotTransform = reelTransforms[columnIndex];

        slotTransform.localPosition = new Vector3(slotTransform.localPosition.x, middlePosition, 0);

        float currentSpeed = spinSpeed;

        float cycleDuration = symbolHeight / currentSpeed;

        Sequence cycleSequence = DOTween.Sequence();

        cycleSequence.Append(
            slotTransform.DOLocalMoveY(middlePosition - symbolHeight, cycleDuration)
                .SetEase(Ease.Linear)
        );

        cycleSequence.OnComplete(() => {
            if (isSpinning)
            {
                CycleReelSymbols(columnIndex);

                if (columnIndex < reelCycleCount.Count)
                {
                    reelCycleCount[columnIndex]++;
                }

                StartReelCycle(columnIndex);
            }
        });

        cycleSequence.Play();

        if (spinTweens.Count <= columnIndex)
            spinTweens.Add(cycleSequence);
        else
            spinTweens[columnIndex] = cycleSequence;
    }

    private void CycleReelSymbols(int columnIndex)
    {
        var reel = reelImagesList[columnIndex];
        if (reel.images == null || reel.images.Count != 8) return;

        for (int i = reel.images.Count - 1; i > 0; i--)
        {
            reel.images[i].sprite = reel.images[i - 1].sprite;
        }

        reel.images[0].sprite = GetSymbolSprite(Random.Range(0, 10));
    }

    #endregion

    #region Stop Spin

    internal void StopSpin(List<List<int>> resultMatrix, System.Action onComplete)
    {
        bool isDoubleDiamond = gameManager?.lastResult?.isDoubleDiamondResult == true;
        if (!isSpinning)
        {
            currentDisplayMatrix = resultMatrix;
            for (int col = 0; col < 5; col++)
            {
                SetReelSymbols(col, resultMatrix[col], false);
                if (isDoubleDiamond)
                    PlayStopAnimationsForColumn(col, resultMatrix[col], isDoubleDiamond);
            }
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(StopSpinSequence(resultMatrix, onComplete, isQuickStop: false, isDoubleDiamond: isDoubleDiamond));
    }

    private IEnumerator StopSpinSequence(List<List<int>> resultMatrix, System.Action onComplete,
        bool isQuickStop, bool isDoubleDiamond)
    {
        currentDisplayMatrix = resultMatrix;

        while (true)
        {
            bool allReelsReady = true;
            for (int col = 0; col < 5; col++)
            {
                if (reelCycleCount[col] < minSpinCyclesBeforeStop)
                {
                    allReelsReady = false;
                    break;
                }
            }

            if (allReelsReady) break;
            yield return null;
        }

        float stagger = isQuickStop ? quickStopStagger : reelStopStagger;
        int remainingReels = 5;
        System.Action onReelSettled = () => remainingReels--;

        for (int col = 0; col < 5; col++)
        {
            float delay = col * stagger;
            StartCoroutine(StopSingleReel(col, resultMatrix[col], delay, isQuickStop, isDoubleDiamond, onReelSettled));
        }

        // Staggered starts can reach their tween callbacks later than a duration estimate.
        yield return new WaitUntil(() => remainingReels == 0);

        isSpinning = false;

        onComplete?.Invoke();
    }

    private IEnumerator StopSingleReel(int columnIndex, List<int> targetSymbols, float delay,
        bool isQuickStop, bool isDoubleDiamond, System.Action onSettled)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        if (columnIndex < spinTweens.Count && spinTweens[columnIndex] != null)
        {
            spinTweens[columnIndex].Kill();
        }

        Transform slotTransform = reelTransforms[columnIndex];

        SetReelSymbols(columnIndex, targetSymbols, false);

        float currentY = slotTransform.localPosition.y;
        float targetY = middlePosition;
        float offset = (currentY - targetY) % cycleDistance;
        if (offset < 0) offset += cycleDistance;

        slotTransform.localPosition = new Vector3(
            slotTransform.localPosition.x,
            targetY + offset,
            0
        );

        // ── Play reel-stop sound immediately when symbols lock in ──────────
        AudioManager.Instance?.PlayReelStop();

        // Legacy CNY adds a Wild hit. DD uses one stop sound per reel.
        if (!isDoubleDiamond && currentDisplayMatrix != null && columnIndex < currentDisplayMatrix.Count)
        {
            bool hasWild = false;
            int wildId = gameManager?.gameConfig != null ? gameManager.gameConfig.wildSymbolId : 10;
            foreach (int sym in currentDisplayMatrix[columnIndex])
            {
                if (sym == wildId) hasWild = true;
            }
            if (hasWild) AudioManager.Instance?.PlayReelStop();
        }
        // ──────────────────────────────────────────────────────────────────

        Sequence stopSequence = DOTween.Sequence();
        if (isQuickStop)
        {
            stopSequence.Append(
                slotTransform.DOLocalMoveY(middlePosition - quickStopOvershoot, quickStopDuration * 0.3f)
                    .SetEase(Ease.OutQuad)
            );

            stopSequence.Append(
                slotTransform.DOLocalMoveY(middlePosition, quickStopDuration * 0.7f)
                    .SetEase(Ease.InOutQuad)
            );

        }
        else
        {
            stopSequence.Append(
                slotTransform.DOLocalMoveY(middlePosition - stopOvershootDistance, stopOvershootDuration)
                    .SetEase(Ease.OutQuad)
            );

            stopSequence.Append(
                slotTransform.DOLocalMoveY(middlePosition, stopSettleDuration)
                    .SetEase(Ease.InOutQuad)
            );

        }

        stopSequence.OnComplete(() =>
        {
            PlayStopAnimationsForColumn(columnIndex, targetSymbols, isDoubleDiamond);
            onSettled();
        });
        spinTweens[columnIndex] = stopSequence;
    }

    #endregion

    #region Quick Spin

    internal void QuickStop(List<List<int>> resultMatrix, System.Action onComplete = null)
    {
        bool isDoubleDiamond = gameManager?.lastResult?.isDoubleDiamondResult == true;
        if (!isSpinning)
        {
            currentDisplayMatrix = resultMatrix;
            for (int col = 0; col < 5; col++)
            {
                if (col < reelTransforms.Length)
                {
                    SetReelSymbols(col, resultMatrix[col], false);
                    reelTransforms[col].localPosition = new Vector3(
                        reelTransforms[col].localPosition.x,
                        middlePosition,
                        0
                    );
                    PlayStopAnimationsForColumn(col, resultMatrix[col], isDoubleDiamond);
                }
            }

            onComplete?.Invoke();
            return;
        }

        StartCoroutine(StopSpinSequence(resultMatrix, onComplete, isQuickStop: true, isDoubleDiamond: isDoubleDiamond));
    }

    #endregion

    #region Reel Stop Presentation

    private void PlayStopAnimationsForColumn(int col, List<int> landedSymbols, bool isDoubleDiamond)
    {
        if (landedSymbols == null) return;
        if (isDoubleDiamond)
        {
            ShowDDInfernoLandings(col, landedSymbols);
            TryPlayBonusReelOutline(col, landedSymbols);
            return;
        }

        for (int row = 0; row < landedSymbols.Count; row++)
        {
            int symId = landedSymbols[row];
            int wildId = gameManager?.gameConfig != null ? gameManager.gameConfig.wildSymbolId : 10;
            bool isWild = (symId == wildId);

            if (isWild)
            {
                AnimateSymbolSingleLoop(col, row, 1);
            }
        }
    }

    private void ShowDDInfernoLandings(int col, List<int> landedSymbols)
    {
        if (reelImagesList == null || col < 0 || col >= reelImagesList.Count)
            return;

        var images = reelImagesList[col]?.images;
        if (images == null)
            return;

        int rowCount = gameManager?.gameConfig?.rowCount ?? landedSymbols.Count;
        int visibleRows = Mathf.Min(rowCount, landedSymbols.Count, images.Count - FirstVisibleReelImage);
        if (visibleRows <= 0 || landedSymbols.IndexOf(DDInfernoSymbolId, 0, visibleRows) < 0)
            return;

        // DD derives the ordinal from reel order even if stop callbacks arrive together.
        int number = 0;
        for (int earlierCol = 0; earlierCol < col; earlierCol++)
        {
            var earlierReel = currentDisplayMatrix[earlierCol];
            for (int row = 0; row < Mathf.Min(rowCount, earlierReel.Count); row++)
            {
                if (earlierReel[row] == DDInfernoSymbolId)
                    number++;
            }
        }

        for (int row = 0; row < visibleRows; row++)
        {
            if (landedSymbols[row] != DDInfernoSymbolId)
                continue;

            number++;
            var image = images[FirstVisibleReelImage + row];
            var cell = image != null ? image.GetComponentInChildren<DDInfernoLandingCell>(true) : null;
            if (cell == null)
                continue;

            activeDDInfernoCells.Add(cell);
            cell.Show(number, ddInfernoLandingFrames, ddInfernoLandingFramesPerSecond);
        }
    }

    private void ClearDDInfernoLandings()
    {
        foreach (var cell in activeDDInfernoCells)
        {
            if (cell != null)
                cell.Hide();
        }

        activeDDInfernoCells.Clear();
    }

    private void TryPlayBonusReelOutline(int col, List<int> landedSymbols)
    {
        int bonusSymbolId = gameManager?.gameConfig?.bonusSymbolId ?? -1;
        if (bonusReelOutlinesSuppressed || bonusSymbolId < 0 || col >= bonusReelOutlines.Length ||
            bonusReelOutlineFrames.Count == 0 || landedSymbols == null) return;

        var outline = bonusReelOutlines[col];
        if (outline == null || outline.rendererDelegate == null) return;

        int visibleRows = Mathf.Min(landedSymbols.Count, gameManager.gameConfig.rowCount);
        if (landedSymbols.IndexOf(bonusSymbolId, 0, visibleRows) < 0) return;

        var image = outline.rendererDelegate;
        image.DOKill();
        Color color = image.color;
        color.a = 1f;
        image.color = color;
        outline.textureArray = bonusReelOutlineFrames;
        outline.doLoopAnimation = true;
        outline.gameObject.SetActive(true);
        if (!outline.TryStartAtFrameRate(bonusReelOutlineFramesPerSecond))
            outline.gameObject.SetActive(false);
    }

    internal void FadeBonusReelOutlinesAfterFirstLoop()
    {
        if (!HasBonusReelOutlineInFirstLoop())
        {
            FadeBonusReelOutlines();
            return;
        }

        if (bonusReelOutlineFadeRoutine == null)
            bonusReelOutlineFadeRoutine = StartCoroutine(WaitForBonusReelOutlineFirstLoops());
    }

    private IEnumerator WaitForBonusReelOutlineFirstLoops()
    {
        // The last reel may have just started its outline as the result presentation begins.
        yield return new WaitWhile(HasBonusReelOutlineInFirstLoop);
        bonusReelOutlineFadeRoutine = null;
        FadeBonusReelOutlines();
    }

    private bool HasBonusReelOutlineInFirstLoop()
    {
        foreach (var outline in bonusReelOutlines)
        {
            if (outline != null && outline.isActiveAndEnabled &&
                outline.currentAnimationState == ImageAnimation.ImageState.PLAYING && !outline.HasCompletedLoop)
                return true;
        }

        return false;
    }

    internal void FadeBonusReelOutlines()
    {
        if (bonusReelOutlineFadeRoutine != null)
        {
            StopCoroutine(bonusReelOutlineFadeRoutine);
            bonusReelOutlineFadeRoutine = null;
        }

        // Ignore a late reel-settle callback after the award popup has opened.
        bonusReelOutlinesSuppressed = true;
        foreach (var outline in bonusReelOutlines)
        {
            if (outline == null || !outline.gameObject.activeSelf) continue;

            var image = outline.rendererDelegate;
            if (image == null || bonusReelOutlineFadeDuration <= 0f)
            {
                image?.DOKill();
                outline.gameObject.SetActive(false);
                continue;
            }

            image.DOKill();
            image.DOFade(0f, bonusReelOutlineFadeDuration)
                .SetLink(outline.gameObject)
                .OnComplete(() => outline.gameObject.SetActive(false));
        }
    }

    // Double Diamond finds the Bonus Diamonds in the landed matrix, even when there are no winning lines.
    internal static List<int> FindBonusDiamondPositions(List<List<int>> reelMatrix, int bonusSymbolId)
    {
        var positions = new List<int>();
        if (reelMatrix == null || bonusSymbolId < 0) return positions;

        for (int col = 0; col < Mathf.Min(5, reelMatrix.Count); col++)
        {
            var reel = reelMatrix[col];
            if (reel == null) continue;

            for (int row = 0; row < reel.Count; row++)
            {
                if (reel[row] == bonusSymbolId)
                    positions.Add(row * 5 + col);
            }
        }

        return positions;
    }

    internal IEnumerator AnimateBonusTrigger(List<int> bonusPositions, float countUpDuration = 0f,
        System.Action<float> updateWinCountUp = null)
    {
        if (bonusPositions.Count == 0 && updateWinCountUp == null) yield break;

        // This can start from the first-pass callback of the win coroutine.
        KillWinTweens(false);
        yield return AnimateWinPositions(bonusPositions, isDoubleDiamond: true,
            countUpDuration: countUpDuration, updateWinCountUp: updateWinCountUp);
        KillWinTweens(false);
    }

    // Legacy CNY scatter playback uses the U-Spin ID.
    internal void AnimateAllScatters(int loopCount)
    {
        if (gameManager?.lastResult?.isDoubleDiamondResult == true) return;
        if (currentDisplayMatrix == null) return;

        // Clear any individual hit animations before starting the collective one
        KillWinTweens();

        int actualScatterId = gameManager?.gameConfig != null ? gameManager.gameConfig.scatterSymbolId : -1;
        if (actualScatterId < 0) return;

        for (int col = 0; col < 5; col++)
        {
            for (int row = 0; row < currentDisplayMatrix[col].Count; row++)
            {
                if (currentDisplayMatrix[col][row] == actualScatterId)
                {
                    AnimateSymbolSingleLoop(col, row, loopCount);
                }
            }
        }
    }

    internal void AnimateUSpinWin(System.Action onComplete = null)
    {
        if (currentDisplayMatrix == null)
        {
            onComplete?.Invoke();
            return;
        }

        KillWinTweens();
        AudioManager.Instance?.PlayWinLinePhase1Start();

        List<ImageAnimation> activeUSpinAnims = new List<ImageAnimation>();
        int completedCount = 0;
        int targetLoops = 2; // Exactly 2 loops of full animation

        for (int col = 0; col < 5; col++)
        {
            if (col >= currentDisplayMatrix.Count) continue;
            for (int row = 0; row < currentDisplayMatrix[col].Count; row++)
            {
                if (currentDisplayMatrix[col][row] == 11) // USpin Symbol ID
                {
                    EnableWinBox(col, row);

                    var animGO = WinBox(winAnimationColumns, col, row);
                    if (animGO != null)
                    {
                        ImageAnimation imageAnim = animGO.GetComponent<ImageAnimation>();
                        int imageIndex = 2 + row;
                        Image symbolImage = (col < reelImagesList.Count && reelImagesList[col].images != null && imageIndex < reelImagesList[col].images.Count)
                            ? reelImagesList[col].images[imageIndex]
                            : null;

                        if (imageAnim != null)
                        {
                            activeUSpinAnims.Add(imageAnim);

                            List<Sprite> animSprites = animationSpriteArrays[11];
                            imageAnim.textureArray = animSprites;
                            imageAnim.doLoopAnimation = true;

                            animGO.SetActive(true);
                            Image animRenderer = imageAnim.rendererDelegate != null ? imageAnim.rendererDelegate : animGO.GetComponent<Image>();
                            if (animRenderer != null)
                            {
                                animRenderer.DOKill();
                                Color c = animRenderer.color;
                                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
                            }
                            if (symbolImage != null)
                            {
                                symbolImage.DOKill();
                                symbolImage.DOFade(0f, 0.2f);
                            }

                            imageAnim.onLoopComplete = (loopCount) =>
                            {
                                if (loopCount >= targetLoops)
                                {
                                    imageAnim.onLoopComplete = null;
                                    imageAnim.StopAnimation();
                                    animGO.SetActive(false);

                                    if (symbolImage != null)
                                    {
                                        symbolImage.DOKill();
                                        symbolImage.DOFade(1f, 0.2f);
                                    }

                                    completedCount++;
                                    if (completedCount >= activeUSpinAnims.Count)
                                    {
                                        onComplete?.Invoke();
                                    }
                                }
                            };

                            imageAnim.StartAnimation();
                        }
                    }
                }
            }
        }

        if (activeUSpinAnims.Count == 0)
        {
            onComplete?.Invoke();
        }
    }

    internal void AnimateMoneyBagWin()
    {
        if (currentDisplayMatrix == null) return;

        KillWinTweens();
        AudioManager.Instance?.PlayWinLinePhase1Start();

        for (int col = 0; col < 5; col++)
        {
            if (col >= currentDisplayMatrix.Count) continue;
            for (int row = 0; row < currentDisplayMatrix[col].Count; row++)
            {
                if (currentDisplayMatrix[col][row] == 12) // MoneyBag Symbol ID
                {
                    EnableWinBox(col, row);
                }
            }
        }
    }

    private void AnimateSymbolSingleLoop(int column, int row, int loopCount = 1)
    {
        if (column >= reelImagesList.Count) return;

        var reel = reelImagesList[column];
        if (reel.images == null || reel.images.Count != 8) return;

        int imageIndex = 2 + row;
        if (imageIndex >= reel.images.Count) return;

        Image symbolImage = reel.images[imageIndex];
        if (symbolImage == null) return;

        var animGO = WinBox(winAnimationColumns, column, row);
        if (animGO == null) return;

        ImageAnimation imageAnim = animGO.GetComponent<ImageAnimation>();
        if (imageAnim == null) return;

        int symbolId = currentDisplayMatrix[column][row];
        if (symbolId < 0 || symbolId >= animationSpriteArrays.Length) return;

        List<Sprite> animSprites = animationSpriteArrays[symbolId];
        if (animSprites == null || animSprites.Count == 0) return;

        imageAnim.textureArray = animSprites;

        Color originalColor = new Color(symbolImage.color.r, symbolImage.color.g, symbolImage.color.b, 1f);

        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() => {
            animGO.SetActive(true);
            Image animRenderer = imageAnim.rendererDelegate;
            if (animRenderer != null)
            {
                animRenderer.DOKill();
                Color c = animRenderer.color;
                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
            }
            symbolImage.DOKill();
            symbolImage.DOFade(0f, 0.2f);

            imageAnim.StartAnimation();
        });

        seq.AppendInterval(winSymbolLoopDuration * loopCount);

        seq.AppendCallback(() => {
            Image animRenderer = imageAnim != null ? imageAnim.rendererDelegate : null;

            if (animRenderer != null)
            {
                animRenderer.DOKill();
                Color c = animRenderer.color;
                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
            }

            if (imageAnim != null) imageAnim.StopAnimation();
            if (animGO != null) animGO.SetActive(false);

            if (symbolImage != null)
            {
                symbolImage.DOKill();
                symbolImage.DOFade(originalColor.a, 0.2f);
            }
        });

        winTweens.Add(seq);
    }

    #endregion

    #region Win Line Animation

    internal void ShowWinLineAnimation(List<WinLine> winLines, System.Action onComplete,
        bool? replayLines = null, System.Action<WinLine, Sprite> onLineShown = null,
        float countUpDuration = 0f, System.Action<float> updateWinCountUp = null)
    {
        if (winLines == null || winLines.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        KillWinTweens();
        bool isDoubleDiamond = gameManager?.lastResult?.isDoubleDiamondResult == true;
        winAnimationCoroutine = StartCoroutine(PlayTwoPhaseWinLines(winLines, onComplete, replayLines,
            onLineShown, isDoubleDiamond, countUpDuration, updateWinCountUp));
    }

    private IEnumerator PlayTwoPhaseWinLines(List<WinLine> winLines, System.Action onComplete,
        bool? replayLines, System.Action<WinLine, Sprite> onLineShown, bool isDoubleDiamond,
        float countUpDuration, System.Action<float> updateWinCountUp)
    {
        int rowLimit = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;

        // ==========================================
        // PHASE 1: Show all winning icons at once
        // ==========================================
        HashSet<int> allWinPositions = new HashSet<int>();
        foreach (var winLine in winLines)
        {
            if (winLine.positions != null)
            {
                foreach (int flatIndex in winLine.positions)
                {
                    allWinPositions.Add(flatIndex);
                }
            }
        }

        //Debug.Log($"[PlayTwoPhaseWinLines] Phase 1: Showing all {allWinPositions.Count} winning icons at once for {winLines.Count} win lines");

        // Calculate Phase 1 Total Win Amount
        double totalWinAmount = 0;
        foreach (var winLine in winLines)
        {
            totalWinAmount += winLine.winAmount;
        }
        if (totalWinAmount <= 0 && gameManager != null && gameManager.lastResult != null)
        {
            totalWinAmount = gameManager.lastResult.winAmount;
        }

        // Show Phase 1 Total Win Text with final win value
        ShowPhase1TotalWin(totalWinAmount, isDoubleDiamond);

        // DD win audio is synchronized with the Win Display count-up.
        if (!isDoubleDiamond)
            AudioManager.Instance?.PlayWinLinePhase1Start();

        // Keep the animation in this coroutine so spin-start cleanup cancels it with the replay.
        yield return AnimateWinPositions(allWinPositions, isDoubleDiamond, randomizeWinBoxes: isDoubleDiamond,
            countUpDuration: countUpDuration, updateWinCountUp: updateWinCountUp);

        KillWinTweens(false);
        HideAllWinLineTexts();
        HidePhase1TotalWinText();

        // Invoke onComplete immediately after Phase 1 so game logic (Free Spins / Autoplay / Win complete) can proceed
        onComplete?.Invoke();

        // Double Diamond decides whether to replay before the completion callback advances the spin.
        // Legacy CNY uses the existing free-spin, autoplay, and feature checks.
        bool hasSpecialFeature = (gameManager != null && gameManager.lastResult != null && (
            (gameManager.lastResult.uSpinData != null && gameManager.lastResult.uSpinData.triggered) ||
            (gameManager.lastResult.moneyBagData != null && gameManager.lastResult.moneyBagData.triggered) ||
            (gameManager.lastResult.freeSpinData != null && gameManager.lastResult.freeSpinData.isTriggered)
        ));

        bool skipLegacyReplay = (gameManager != null && (gameManager.isInFreeSpins || gameManager.isAutoPlaying)) || hasSpecialFeature;
        bool skipPhase2 = replayLines.HasValue ? !replayLines.Value : skipLegacyReplay;
        if (skipPhase2)
        {
            yield break;
        }

        // ==========================================
        // PHASE 2: Individual Win Line presentation loop
        // ==========================================
        var winBoxSpritesByLine = new Dictionary<int, Sprite>();
        while (true)
        {
            foreach (var winLine in winLines)
            {
                if (winLine.positions == null || winLine.positions.Count == 0) continue;

                KillWinTweens(false);
                HideAllWinLineTexts();

                // Double Diamond shows the line payout even when there is just one winning line.
                if (onLineShown != null || winLines.Count > 1)
                {
                    int labelPosition = isDoubleDiamond
                        ? GetRightmostWinPosition(winLine.positions, rowLimit)
                        : winLine.positions[0];
                    if (labelPosition >= 0)
                        ShowWinLineTextOnIcon(labelPosition % 5, labelPosition / 5, winLine.winAmount, isDoubleDiamond);
                }

                // Keep the Double Diamond popup synchronized with the line being highlighted.
                onLineShown?.Invoke(winLine, GetSymbolSprite(winLine.symbolId));

                Sprite lineWinBoxSprite = null;
                if (isDoubleDiamond && !winBoxSpritesByLine.TryGetValue(winLine.lineId, out lineWinBoxSprite))
                {
                    lineWinBoxSprite = PickLineWinBoxSprite(winLine.positions, rowLimit);
                    winBoxSpritesByLine.Add(winLine.lineId, lineWinBoxSprite);
                }

                yield return AnimateWinPositions(winLine.positions, isDoubleDiamond, winBoxSprite: lineWinBoxSprite,
                    loopCountOverride: isDoubleDiamond ? 1 : (int?)null);
            }
        }
    }

    private IEnumerator AnimateWinPositions(IEnumerable<int> flatPositions, bool isDoubleDiamond,
        bool randomizeWinBoxes = false, Sprite winBoxSprite = null, float countUpDuration = 0f,
        System.Action<float> updateWinCountUp = null, int? loopCountOverride = null)
    {
        if (flatPositions == null) yield break;

        bool usesCountUp = updateWinCountUp != null;
        if (usesCountUp && countUpDuration <= 0f)
        {
            updateWinCountUp(1f);
            yield break;
        }

        int rowLimit = gameManager?.gameConfig?.rowCount ?? 3;
        int loopCountTarget = loopCountOverride ?? ((gameManager != null && (gameManager.isInFreeSpins || gameManager.isAutoPlaying))
            ? 1
            : Mathf.Max(1, winSymbolLoopCount));
        var animationLookup = isDoubleDiamond ? doubleDiamondAnimationSpriteArrays : animationSpriteArrays;
        var animations = new List<(ImageAnimation player, Image renderer, Image symbol, List<Sprite> frames)>();

        foreach (int flatIndex in flatPositions)
        {
            if (flatIndex < 0) continue;

            int row = flatIndex / 5;
            int col = flatIndex % 5;
            if (row >= rowLimit) continue;

            // WinBoxes belong to the matching cells even when the symbols have no frames.
            EnableWinBox(col, row, winBoxSprite, randomizeWinBoxes);

            if (animationLookup == null || currentDisplayMatrix == null || col >= currentDisplayMatrix.Count ||
                currentDisplayMatrix[col] == null || row >= currentDisplayMatrix[col].Count) continue;
            int symbolId = currentDisplayMatrix[col][row];
            if (symbolId < 0 || symbolId >= animationLookup.Length) continue;
            var frames = animationLookup[symbolId];
            if (frames == null || frames.Count == 0) continue;

            if (col >= reelImagesList.Count) continue;
            var reel = reelImagesList[col];
            int imageIndex = 2 + row;
            if (reel.images == null || imageIndex >= reel.images.Count) continue;
            Image symbol = reel.images[imageIndex];
            if (symbol == null) continue;

            var overlay = WinBox(winAnimationColumns, col, row);
            if (overlay == null || !overlay.TryGetComponent(out ImageAnimation player) || !player.enabled) continue;
            var renderer = player.rendererDelegate != null ? player.rendererDelegate : overlay.GetComponent<Image>();
            if (renderer == null) continue;

            animations.Add((player, renderer, symbol, frames));
        }

        if (winLineBoxToAnimationDelay > 0)
        {
            yield return new WaitForSeconds(winLineBoxToAnimationDelay);
        }

        int animatedCount = 0;
        foreach (var cell in animations)
        {
            var player = cell.player;
            var overlay = player.gameObject;

            // Assign frames after activation so every cell starts on the same pass.
            player.textureArray = null;
            overlay.SetActive(true);
            bool started = player.isActiveAndEnabled;
            if (!usesCountUp)
            {
                player.textureArray = cell.frames;
                player.doLoopAnimation = true;
                player.onLoopComplete = loops =>
                {
                    if (loops < loopCountTarget) return;

                    player.onLoopComplete = null;
                    player.StopAnimation();
                    overlay.SetActive(false);
                    if (cell.symbol != null)
                    {
                        cell.symbol.DOKill();
                        cell.symbol.DOFade(1f, 0.2f);
                    }
                };

                if (isDoubleDiamond)
                {
                    started = player.TryStartAtFrameRate(doubleDiamondWinFramesPerSecond);
                }
                else
                {
                    player.StartAnimation();
                    started = player.currentAnimationState == ImageAnimation.ImageState.PLAYING;
                }
            }

            if (!started)
            {
                player.onLoopComplete = null;
                overlay.SetActive(false);
                continue;
            }

            cell.renderer.DOKill();
            Color color = cell.renderer.color;
            cell.renderer.color = new Color(color.r, color.g, color.b, 1f);
            cell.symbol.DOKill();
            cell.symbol.DOFade(0f, 0.2f);
            animatedCount++;
        }

        // The count-up and every grouped symbol reach the end on the same clock.
        if (usesCountUp)
        {
            yield return AnimateWinCountUp(countUpDuration, updateWinCountUp, animations);
        }
        else if (animatedCount > 0)
        {
            yield return new WaitUntil(() => animations.TrueForAll(cell => cell.player == null ||
                !cell.player.isActiveAndEnabled || cell.player.currentAnimationState != ImageAnimation.ImageState.PLAYING));
        }
        else
        {
            yield return new WaitForSeconds(winSymbolLoopDuration);
        }
    }

    internal IEnumerator AnimateWinCountUp(float duration, System.Action<float> updateWinCountUp,
        List<(ImageAnimation player, Image renderer, Image symbol, List<Sprite> frames)> animations = null)
    {
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            float progress = elapsed / duration;
            updateWinCountUp(progress);
            if (animations != null)
            {
                foreach (var cell in animations)
                {
                    if (cell.player == null || !cell.player.isActiveAndEnabled || cell.renderer == null) continue;

                    int frame = Mathf.Min((int)(progress * cell.frames.Count), cell.frames.Count - 1);
                    cell.renderer.sprite = cell.frames[frame];
                }
            }

            yield return null;
        }

        updateWinCountUp(1f);
    }

    private static int GetRightmostWinPosition(IEnumerable<int> positions, int rowLimit)
    {
        int rightmostPosition = -1;
        int rightmostColumn = -1;
        foreach (int position in positions)
        {
            if (position < 0 || position / 5 >= rowLimit) continue;

            int column = position % 5;
            if (column <= rightmostColumn) continue;

            rightmostPosition = position;
            rightmostColumn = column;
        }

        return rightmostPosition;
    }

    private void ShowWinLineTextOnIcon(int col, int row, double winAmount, bool isDoubleDiamond)
    {
        if (col < 0 || col >= reelImagesList.Count) return;
        var reel = reelImagesList[col];
        if (reel.images == null) return;
        int imageIndex = 2 + row;
        if (imageIndex >= reel.images.Count) return;

        Image symbolImage = reel.images[imageIndex];
        if (symbolImage == null) return;

        Transform textTransform = symbolImage.transform.Find("WinLineText");
        if (textTransform != null)
        {
            var tmpText = textTransform.GetComponent<TMPro.TMP_Text>();
            if (tmpText != null)
            {
                SetWinAmountText(tmpText, winAmount, isDoubleDiamond);
            }
            if (isDoubleDiamond)
            {
                textTransform.localScale = Vector3.one;
                textTransform.gameObject.SetActive(true);
            }
            else
            {
                AnimateTextScaleAppear(textTransform);
            }
        }
    }

    private void HideAllWinLineTexts()
    {
        if (reelImagesList == null) return;
        foreach (var reel in reelImagesList)
        {
            if (reel.images != null)
            {
                foreach (var image in reel.images)
                {
                    if (image != null)
                    {
                        Transform textTransform = image.transform.Find("WinLineText");
                        if (textTransform != null)
                        {
                            textTransform.DOKill();
                            textTransform.localScale = Vector3.one;
                            textTransform.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    private void ShowPhase1TotalWin(double totalWinAmount, bool isDoubleDiamond)
    {
        if (phase1TotalWinText != null)
        {
            SetWinAmountText(phase1TotalWinText, totalWinAmount, isDoubleDiamond);
            AnimateTextScaleAppear(phase1TotalWinText.transform);
        }
    }

    private void SetWinAmountText(TMPro.TMP_Text label, double amount, bool isDoubleDiamond)
    {
        if (isDoubleDiamond && gameManager?.uiManager != null)
            gameManager.uiManager.SetWinSpriteDigits(label, amount);
        else
            label.text = amount.ToString("0.###");
    }

    private void HidePhase1TotalWinText()
    {
        if (phase1TotalWinText != null)
        {
            phase1TotalWinText.transform.DOKill();
            phase1TotalWinText.transform.localScale = Vector3.one;
            phase1TotalWinText.gameObject.SetActive(false);
        }
    }

    private void AnimateTextScaleAppear(Transform textTransform, float popScale = 1.2f, float durationUp = 0.15f, float durationDown = 0.10f)
    {
        if (textTransform == null) return;
        textTransform.DOKill();
        textTransform.localScale = Vector3.zero;
        textTransform.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(textTransform.DOScale(popScale, durationUp).SetEase(Ease.OutQuad));
        seq.Append(textTransform.DOScale(1.0f, durationDown).SetEase(Ease.InQuad));
        winTweens.Add(seq);
    }
    private Sprite PickLineWinBoxSprite(IEnumerable<int> positions, int rowLimit)
    {
        foreach (int position in positions)
        {
            if (position < 0 || position / 5 >= rowLimit) continue;

            var box = WinBox(winBoxColumns, position % 5, position / 5);
            var sprite = GetRandomWinBoxSprite(box);
            if (sprite != null) return sprite;
        }

        return null;
    }

    private static Sprite GetRandomWinBoxSprite(GameObject box)
    {
        return box != null && box.TryGetComponent(out WinBoxSelector selector) ? selector.GetRandomSprite() : null;
    }

    private void EnableWinBox(int col, int row, Sprite sprite = null, bool randomize = false)
    {
        var go = WinBox(winBoxColumns, col, row);
        if (go)
        {
            if (go.TryGetComponent(out Image image))
                image.overrideSprite = randomize ? GetRandomWinBoxSprite(go) : sprite;

            go.SetActive(true);
        }
        else
        {
            Debug.LogError($"[EnableWinBox] WinBox GameObject is NULL at col: {col}, row: {row}");
        }
    }

    private void DisableWinBox(int col, int row)
    {
        var go = WinBox(winBoxColumns, col, row);
        if (go) go.SetActive(false);
    }

    private void ResetSymbolScale(int col, int row)
    {
        if (col >= reelImagesList.Count) return;
        var reel = reelImagesList[col];
        if (reel.images == null) return;
        int imageIndex = 2 + row;
        if (imageIndex >= reel.images.Count) return;
        if (reel.images[imageIndex] != null)
        {
            reel.images[imageIndex].DOKill();
            reel.images[imageIndex].transform.localScale = Vector3.one * reelSymbolScale;
            // Restore alpha to full opacity
            Color c = reel.images[imageIndex].color;
            reel.images[imageIndex].color = new Color(c.r, c.g, c.b, 1f);
        }

        // Also ensure the corresponding animation object is disabled
        var animGO = WinBox(winAnimationColumns, col, row);
        if (animGO != null)
        {
            ImageAnimation imageAnim = animGO.GetComponent<ImageAnimation>();
            if (imageAnim != null)
            {
                if (imageAnim.rendererDelegate != null) imageAnim.rendererDelegate.DOKill();
                imageAnim.StopAnimation();
            }
            animGO.SetActive(false);
        }
    }


    private void AnimateWinSymbol(int column, int row)
    {

        if (column >= reelImagesList.Count)
        {
            Debug.LogError($"[AnimateWinSymbol] Invalid column {column}, max is {reelImagesList.Count - 1}");
            return;
        }

        var reel = reelImagesList[column];
        if (reel.images == null || reel.images.Count != 8)
        {
            Debug.LogError($"[AnimateWinSymbol] Reel {column} has invalid images list");
            return;
        }

        int imageIndex = 2 + row;
        if (imageIndex >= reel.images.Count)
        {
            Debug.LogError($"[AnimateWinSymbol] Image index {imageIndex} out of range for reel {column}");
            return;
        }

        Image symbolImage = reel.images[imageIndex];
        if (symbolImage == null)
        {
            Debug.LogError($"[AnimateWinSymbol] Symbol image is NULL at col: {column}, row: {row}, imageIndex: {imageIndex}");
            return;
        }



        // Get the animation GameObject for this position
        var animGO = WinBox(winAnimationColumns, column, row);
        if (animGO == null)
        {
            Debug.LogError($"[AnimateWinSymbol] Animation GameObject is NULL at col: {column}, row: {row}");
            return;
        }

        // Get the ImageAnimation component
        ImageAnimation imageAnim = animGO.GetComponent<ImageAnimation>();
        if (imageAnim == null)
        {
            Debug.LogError($"[AnimateWinSymbol] ImageAnimation component not found on animation object at col: {column}, row: {row}");
            return;
        }

        // Get the current symbol ID at this position
        if (column >= currentDisplayMatrix.Count || row >= currentDisplayMatrix[column].Count)
        {
            Debug.LogError($"[AnimateWinSymbol] Invalid matrix position col: {column}, row: {row}");
            return;
        }

        int symbolId = currentDisplayMatrix[column][row];

        // Validate symbolId
        if (symbolId < 0 || symbolId >= animationSpriteArrays.Length)
        {
            Debug.LogError($"[AnimateWinSymbol] Invalid symbolId {symbolId} at col: {column}, row: {row}");
            return;
        }

        // Get the animation sprite array for this symbol
        List<Sprite> animSprites = animationSpriteArrays[symbolId];
        if (animSprites == null || animSprites.Count == 0)
        {
            // Expected for most symbols now
            return;
        }

        // Set the sprite array on the ImageAnimation component
        imageAnim.textureArray = animSprites;

        Color originalColor = new Color(symbolImage.color.r, symbolImage.color.g, symbolImage.color.b, 1f);

        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() => {
            animGO.SetActive(true);
            Image animRenderer = imageAnim.rendererDelegate;
            if (animRenderer != null)
            {
                animRenderer.DOKill();
                Color c = animRenderer.color;
                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
            }
            symbolImage.DOKill();
            symbolImage.DOFade(0f, 0.2f);
        });

        if (winLineBoxToAnimationDelay > 0)
        {
            seq.AppendInterval(winLineBoxToAnimationDelay);
        }

        seq.AppendCallback(() => {
            imageAnim.StartAnimation();
        });

        int loopCount = (gameManager != null && (gameManager.isInFreeSpins || gameManager.isAutoPlaying)) ? 1 : winSymbolLoopCount;
        seq.AppendInterval(winSymbolLoopDuration * loopCount);

        seq.AppendCallback(() => {
            Image animRenderer = imageAnim != null ? imageAnim.rendererDelegate : null;

            if (animRenderer != null)
            {
                animRenderer.DOKill();
                Color c = animRenderer.color;
                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
            }

            if (imageAnim != null) imageAnim.StopAnimation();
            if (animGO != null) animGO.SetActive(false);

            if (symbolImage != null)
            {
                symbolImage.DOKill();
                symbolImage.DOFade(originalColor.a, 0.2f);
            }
        });

        winTweens.Add(seq);
    }

    private void KillWinTweens(bool stopCoroutine = true)
    {
        foreach (var tween in winTweens)
        {
            tween?.Kill();
        }
        winTweens.Clear();

        if (stopCoroutine && winAnimationCoroutine != null)
        {
            StopCoroutine(winAnimationCoroutine);
            winAnimationCoroutine = null;
        }

        // Stop all win animations and disable animation GameObjects
        if (winAnimationColumns != null)
        {
            foreach (var col in winAnimationColumns)
            {
                if (col?.rows != null)
                {
                    foreach (var animGO in col.rows)
                    {
                        if (animGO != null)
                        {
                            ImageAnimation imageAnim = animGO.GetComponent<ImageAnimation>();
                            if (imageAnim != null)
                            {
                                imageAnim.onLoopComplete = null;
                                if (imageAnim.rendererDelegate != null)
                                {
                                    imageAnim.rendererDelegate.DOKill();
                                    Color ac = imageAnim.rendererDelegate.color;
                                    imageAnim.rendererDelegate.color = new Color(ac.r, ac.g, ac.b, 1f);
                                }
                                imageAnim.StopAnimation();
                            }
                            if (animGO.activeSelf)
                            {
                                animGO.SetActive(false);
                            }
                        }
                    }
                }
            }
        }

        DisableColumns(winBoxColumns);
        HideAllWinLineTexts();
        HidePhase1TotalWinText();

        // Restore each symbol image to its configured scale and full opacity
        foreach (var reel in reelImagesList)
        {
            if (reel.images != null)
            {
                foreach (var image in reel.images)
                {
                    if (image != null)
                    {
                        image.DOKill();
                        image.transform.localScale = Vector3.one * reelSymbolScale;
                        Color c = image.color;
                        image.color = new Color(c.r, c.g, c.b, 1f);
                    }
                }
            }
        }
    }

    #endregion



    internal List<List<int>> GetCurrentDisplayMatrix()
    {
        return currentDisplayMatrix;
    }

    internal bool IsSpinning()
    {
        return isSpinning;
    }


    private void KillAllTweens()
    {
        foreach (var tween in spinTweens)
        {
            tween?.Kill();
        }
        spinTweens.Clear();

        KillWinTweens();
    }

    #region Cleanup

    private void OnDestroy()
    {
        KillAllTweens();
    }

    #endregion
}

[System.Serializable]
public class ReelImages
{
    public List<Image> images = new List<Image>(16);
}


[System.Serializable]
public class ColumnOverlays
{
    [Tooltip("Row 0 = top, Row 1, Row 2 = bottom")]
    public GameObject[] rows = new GameObject[3];
}
