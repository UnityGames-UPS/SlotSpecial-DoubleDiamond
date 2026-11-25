using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] internal SocketIOManager socketManager;
    [SerializeField] internal UIManager uiManager;
    [SerializeField] private PopupManager popupManager;
    [SerializeField] private SlotView slotView;
    [SerializeField] internal WheelSpinController wheelController;

    [Header("Spin Settings")]
    [SerializeField] private float normalSpinDuration = 3.5f;
    [SerializeField] private float turboSpinDuration = 2.0f;
    [SerializeField] private float quickSpinCycleDuration = 0.8f;
    [Tooltip("Seconds the reels keep spinning after pressing Stop. Set to 0 to skip this pause.")]
    [SerializeField, Min(0f)] private float manualStopDelay = 0.5f;

    [Header("Win Settings")]
    [SerializeField] private double bigWinMultiplierThreshold = 500.0;
    public double BigWinMultiplierThreshold => bigWinMultiplierThreshold;

    internal GameConfig gameConfig;
    internal PlayerData playerData;
    internal SpinResult lastResult;

    internal GameState currentState;
    internal SpinSpeed currentSpinSpeed;

    internal int currentBetIndex;
    internal double currentBetAmount;

    internal bool isAutoPlaying;
    internal int autoPlayTotalRounds;
    internal int autoPlayRemainingRounds;
    internal bool wasAutoPlayingBeforeFreeSpins;
    internal int savedAutoPlayRemainingRounds;
    internal int savedAutoPlayTotalRounds;

    internal bool isInFreeSpins;
    internal int freeSpinsRemaining;
    internal int freeSpinsUsed;
    private int doubleDiamondFreeSpinsAwarded;
    internal bool waitingForFreeSpinStart;

    internal bool isInitialized;
    internal bool initializationFailed;

    private Coroutine spinCoroutine;
    private Coroutine firstFreeSpinCoroutine;
    private Coroutine jackpotPresentationCoroutine;
    private SpinResult jackpotPresentationResult;
    private bool stopRequested;
    private bool waitingForSpecialWin;

    #region Initialization

    private void Start()
    {
        currentState = GameState.Initializing;
        currentSpinSpeed = SpinSpeed.Normal;
        waitingForFreeSpinStart = false;
        isInitialized = false;
        initializationFailed = false;
    }

    internal void OnInitDataReceived(GameConfig config, PlayerData player, List<List<int>> initialMatrix)
    {
        gameConfig = config;
        playerData = player;
        currentBetIndex = playerData.currentBetIndex;
        UpdateBetAmount();

        if (slotView != null)
        {
            if (initialMatrix != null)
            {
                slotView.SetInitialMatrix(initialMatrix);
            }

            slotView.SetupSymbolButtons();
        }

        if (wheelController != null && gameConfig.uSpinSegments != null)
        {
            wheelController.OverrideSegmentsWithData(gameConfig.uSpinSegments);
        }

        isInitialized = true;
        currentState = GameState.Idle;

        uiManager.OnGameInitialized();
    }

    #endregion

    #region Bet Management

    internal void IncreaseBet()
    {
        if (currentState != GameState.Idle || isAutoPlaying) return;
        if (gameConfig == null || gameConfig.availableBets == null || gameConfig.availableBets.Count == 0) return;

        int maxIndex = gameConfig.availableBets.Count - 1;
        int nextIndex = currentBetIndex + 1;
        if (nextIndex > maxIndex)
        {
            nextIndex = 0;
        }

        if (nextIndex == maxIndex)
        {
            AudioManager.Instance?.PlayMaxBetReached();
        }
        else
        {
            AudioManager.Instance?.PlayBetPlusMinus();
        }

        SetBetIndex(nextIndex);
    }

    internal void DecreaseBet()
    {
        if (currentState != GameState.Idle || isAutoPlaying) return;
        if (gameConfig == null || gameConfig.availableBets == null || gameConfig.availableBets.Count == 0) return;

        int maxIndex = gameConfig.availableBets.Count - 1;
        int nextIndex = currentBetIndex - 1;
        if (nextIndex < 0)
        {
            nextIndex = maxIndex;
        }

        if (nextIndex == maxIndex)
        {
            AudioManager.Instance?.PlayMaxBetReached();
        }
        else
        {
            AudioManager.Instance?.PlayBetPlusMinus();
        }

        SetBetIndex(nextIndex);
    }

    internal void SetBetIndex(int index)
    {
        currentBetIndex = index;
        UpdateBetAmount();
        uiManager.UpdateBetDisplay();
        if (slotView != null) slotView.OnBetChanged();
    }

    private void UpdateBetAmount()
    {
        currentBetAmount = gameConfig.availableBets[currentBetIndex];
    }

    #endregion

    #region Spin Control
    
    internal void RequestSpin(bool playButtonCue = false)
    {
        if (waitingForFreeSpinStart) return;

        if (currentState != GameState.Idle) return;
        if (!socketManager.isConnected) return;

        double totalPay = GetTotalPay();
        if (!isInFreeSpins && playerData.balance < totalPay)
        {
            if (popupManager != null)
            {
                popupManager.ShowInsufficientFundsError();
            }
            return;
        }

        if (playButtonCue)
            AudioManager.Instance?.PlaySpinStart();

        StartSpin();
    }

    internal void RequestStop()
    {
        if (currentState == GameState.Spinning)
        {
            if (isAutoPlaying)
            {
                StopAutoPlay();
            }
            else if (!isInFreeSpins)
            {
                stopRequested = true;
                uiManager.DisableSpinButtonDuringStop();
            }
        }
    }

    private void StartSpin()
    {
        if (lastResult != null)
        {
            ProcessSpinResult();
        }

        lastResult = null;
        currentState = GameState.Spinning;
        stopRequested = false;

        // Deduct total pay from balance on spin start (except in free spins)
        if (!isInFreeSpins)
        {
            playerData.balance -= GetTotalPay();
            if (playerData.balance < 0) playerData.balance = 0;
        }

        // Clear the Double Diamond line replay UI when a new spin starts.
        uiManager.CloseLineWinPopup();
        uiManager.OnSpinStarted();

        if (slotView != null)
        {
            slotView.StartSpin();
        }

        socketManager.SendSpinRequest(currentBetIndex, isInFreeSpins);

        if (spinCoroutine != null)
            StopCoroutine(spinCoroutine);
        spinCoroutine = StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        float spinDuration = GetSpinDuration();
        float elapsed = 0f;

        while (elapsed < spinDuration && !stopRequested)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // A manual stop can keep the reels spinning briefly for visual feedback.
        if (stopRequested && manualStopDelay > 0f)
        {
            yield return new WaitForSeconds(manualStopDelay);
        }

        while (lastResult == null)
        {
            yield return null;
        }

        currentState = GameState.Stopping;
        var stoppedResult = lastResult;
        System.Action onReelsStopped = () => OnReelsStoppedComplete(stoppedResult);

        if (slotView != null && stoppedResult.resultMatrix != null)
        {
            if (currentSpinSpeed == SpinSpeed.QuickSpin || stopRequested)
            {
                slotView.QuickStop(stoppedResult.resultMatrix, onReelsStopped);
            }
            else
            {
                slotView.StopSpin(stoppedResult.resultMatrix, onReelsStopped);
            }
        }
        else
        {
            AudioManager.Instance?.StopDDReelSpinning();
            onReelsStopped();
        }
    }

    private void OnReelsStoppedComplete(SpinResult stoppedResult)
    {
        // SlotView stops reels asynchronously. A late callback must still belong to the active spin.
        if (stoppedResult == null || lastResult != stoppedResult || currentState != GameState.Stopping ||
            socketManager == null || !socketManager.isConnected)
        {
            return;
        }

        // Double Diamond keeps the outlines for the trigger popup only when opening a free-spin round.
        if (lastResult?.isDoubleDiamondResult == true &&
            (lastResult.freeSpinData?.isTriggered != true || isInFreeSpins))
        {
            slotView?.FadeBonusReelOutlinesAfterFirstLoop();
        }

        if (lastResult != null)
        {
            if (lastResult.jackpotBonusData?.isTriggered == true)
            {
                if (jackpotPresentationResult != lastResult)
                {
                    jackpotPresentationResult = lastResult;
                    jackpotPresentationCoroutine = StartCoroutine(PlayDoubleDiamondJackpot(lastResult));
                }
                return;
            }

            // Keep the post-bet balance visible before any trigger-result UI is refreshed.
            if (lastResult.isDoubleDiamondResult && !lastResult.isFreeSpin &&
                lastResult.freeSpinData?.isTriggered == true)
            {
                uiManager.HoldDoubleDiamondFreeSpinBalance();
            }

            double reelStopBalance = lastResult.playerData?.balance ?? 0d;
            // Legacy CNY defers crediting U-Spin and Money Bag payouts until their features finish.
            if (!lastResult.isDoubleDiamondResult && lastResult.playerData != null)
                reelStopBalance -= lastResult.GetTotalFeatureDeferredWins();

            playerData = new PlayerData
            {
                balance = reelStopBalance,
                currentBetIndex = lastResult.playerData != null ? lastResult.playerData.currentBetIndex : currentBetIndex
            };
        }

        if (lastResult?.isDoubleDiamondResult == true && lastResult.winAmount > 0)
        {
            var result = lastResult;
            uiManager.DisableControlsDuringWinAnimation();
            var updateWinCountUp = uiManager.BeginWinDisplayCountUp(result);
            float duration = uiManager.WinDisplayCountUpDuration;

            if (result.winLines?.Count > 0)
            {
                bool replayLines = !result.isFreeSpin && !isInFreeSpins &&
                    !isAutoPlaying && result.freeSpinData?.isTriggered != true;
                slotView.ShowWinLineAnimation(
                    result.winLines,
                    onComplete: () => StartCoroutine(ProcessSpecialFeaturesAfterWin()),
                    replayLines: replayLines,
                    onLineShown: uiManager.ShowLineWinPopup,
                    countUpDuration: duration,
                    updateWinCountUp: updateWinCountUp);
            }
            else
            {
                StartCoroutine(PlayDoubleDiamondWinWithoutLines(result, duration, updateWinCountUp));
            }
            return;
        }

        if (lastResult != null && lastResult.winAmount > 0 && lastResult.winLines != null && lastResult.winLines.Count > 0)
        {
            // Legacy CNY threshold presentation.
            double totalPay = GetTotalPay();
            double multiplier = totalPay > 0 ? (lastResult.winAmount / totalPay) : 0;

            if (multiplier >= bigWinMultiplierThreshold)
            {
                uiManager.DisableControlsDuringWinAnimation();
                currentState = GameState.Idle;
                slotView.ShowWinLineAnimation(lastResult.winLines, OnWinAnimationComplete);
                StartCoroutine(TriggerWinPopupWithDelay(1.5f, lastResult));
            }
            else
            {
                // For normal wins, trigger UI update immediately and enable controls
                uiManager.OnSpinStopping(lastResult);
                uiManager.EnableControlsAfterWinAnimation();
                uiManager.OnSpinCompleted(lastResult);
                currentState = GameState.Idle;
                slotView.ShowWinLineAnimation(lastResult.winLines, OnWinAnimationComplete);
            }
        }
        else
        {
            uiManager.OnSpinStopping(lastResult);
            currentState = GameState.Idle;
            OnWinAnimationComplete();
        }
    }

    // Jackpot Inferno plays the table highlight alongside the grouped win, then waits for Take.
    private IEnumerator PlayDoubleDiamondJackpot(SpinResult result)
    {
        uiManager.DisableControlsDuringWinAnimation();
        uiManager.HoldDoubleDiamondJackpotBalance();
        var updateLineWin = uiManager.BeginDoubleDiamondJackpotLineCountUp(result);
        bool groupedWinFinished = false;
        if (slotView != null && slotView.isActiveAndEnabled && result.winLines?.Count > 0)
        {
            slotView.ShowWinLineAnimation(
                result.winLines,
                onComplete: () => groupedWinFinished = true,
                replayLines: false,
                countUpDuration: uiManager.WinDisplayCountUpDuration,
                updateWinCountUp: updateLineWin);
        }
        else
        {
            updateLineWin?.Invoke(1f);
            groupedWinFinished = true;
        }

        yield return uiManager.PlayDoubleDiamondJackpotOverlays(result.jackpotBonusData);
        while (!groupedWinFinished && slotView != null && slotView.isActiveAndEnabled)
            yield return null;

        // The line view may have been disabled before invoking its completion callback.
        if (!groupedWinFinished)
            updateLineWin?.Invoke(1f);

        // The shared popup has one Take callback. Wait for any earlier presentation to close.
        while (uiManager.IsDDTotalWinPopupOpen)
            yield return null;

        bool taken = false;
        if (lastResult == result && uiManager.isActiveAndEnabled)
        {
            uiManager.ShowDDTotalWin(UIManager.TotalWinTitle.Jackpot, result.jackpotBonusData.totalWon,
                () => taken = true);
            while (!taken && uiManager.IsDDTotalWinPopupOpen)
                yield return null;
        }

        if (!taken || lastResult != result)
        {
            jackpotPresentationCoroutine = null;
            ClearDoubleDiamondJackpotPresentation();
            currentState = GameState.Idle;
            yield break;
        }

        // The spin result already contains the settled balance and the jackpot in its win totals.
        playerData = result.playerData;
        uiManager.RevealDoubleDiamondJackpotWin(result);

        // Keep the handoff in this coroutine so a disconnect can cancel it before the round advances.
        yield return ProcessSpecialFeaturesAfterWin();
        if (jackpotPresentationResult == result)
        {
            jackpotPresentationCoroutine = null;
            jackpotPresentationResult = null;
        }
    }

    private void CancelDoubleDiamondJackpotPresentation()
    {
        if (jackpotPresentationCoroutine == null)
            return;

        StopCoroutine(jackpotPresentationCoroutine);
        jackpotPresentationCoroutine = null;
        ClearDoubleDiamondJackpotPresentation();
    }

    private void ClearDoubleDiamondJackpotPresentation()
    {
        uiManager?.CancelDoubleDiamondJackpotPresentation();
        if (lastResult == jackpotPresentationResult)
            lastResult = null;
        jackpotPresentationResult = null;
    }

    private IEnumerator TriggerWinPopupWithDelay(float delay, SpinResult result)
    {
        double totalPay = GetTotalPay();
        double multiplier = totalPay > 0 ? (result.winAmount / totalPay) : 0;
        if (multiplier < bigWinMultiplierThreshold)
        {
            waitingForSpecialWin = false;
            yield break;
        }

        waitingForSpecialWin = true;

        yield return new WaitForSeconds(delay);

        if (lastResult == result && multiplier >= bigWinMultiplierThreshold)
        {
            uiManager.TriggerBigWinPopup(result, () =>
            {
                waitingForSpecialWin = false;
            });
        }
        else
        {
            waitingForSpecialWin = false;
        }
    }

    private void OnWinAnimationComplete()
    {
        if (lastResult != null)
        {
            double totalPay = GetTotalPay();
            double multiplier = totalPay > 0 ? (lastResult.winAmount / totalPay) : 0;

            // Only update UI here if it wasn't already updated in OnReelsStoppedComplete (multiplier < bigWinMultiplierThreshold)
            if (multiplier >= bigWinMultiplierThreshold)
            {
                uiManager.OnSpinStopping(lastResult);
            }
        }

        StartCoroutine(ProcessSpecialFeaturesAfterWin());
    }

    private IEnumerator ProcessSpecialFeaturesAfterWin()
    {
        // Wait for special win popup to finish before starting special features
        while (waitingForSpecialWin || uiManager.IsSpecialWinActive)
        {
            yield return null;
        }

        if (lastResult != null && lastResult.uSpinData != null && lastResult.uSpinData.triggered)
        {
            yield return StartCoroutine(DelayUSpinTriggerResult());
            yield break;
        }

        if (lastResult != null && lastResult.moneyBagData != null && lastResult.moneyBagData.triggered)
        {
            yield return StartCoroutine(DelayMoneyBagTriggerResult());
            yield break;
        }

        if (lastResult != null && lastResult.freeSpinData != null && lastResult.freeSpinData.isTriggered && !isInFreeSpins)
        {
            if (lastResult.isDoubleDiamondResult)
                yield return PlayDoubleDiamondFreeSpinTrigger(lastResult);
            else
                yield return StartCoroutine(DelayScatterTriggerResult()); // Legacy CNY.

            yield break;
        }

        yield return ResumeAfterSpecialFeature();
    }

    private IEnumerator ResumeAfterSpecialFeature()
    {
        if (isAutoPlaying || isInFreeSpins)
            yield return DelayBeforeNextRound();
        else
            ProcessSpinResult();
    }

    private IEnumerator PlayDoubleDiamondWinWithoutLines(SpinResult result, float countUpDuration,
        System.Action<float> updateWinCountUp)
    {
        if (result.freeSpinData?.isTriggered == true && !isInFreeSpins)
        {
            yield return PlayDoubleDiamondFreeSpinTrigger(result, countUpDuration, updateWinCountUp);
            yield break;
        }

        yield return slotView.AnimateWinCountUp(countUpDuration, updateWinCountUp);
        if (lastResult == result)
            yield return ProcessSpecialFeaturesAfterWin();
    }

    internal void FadeBonusReelOutlines() => slotView?.FadeBonusReelOutlines();

    // Double Diamond plays the Bonus Diamonds before opening the award popup.
    private IEnumerator PlayDoubleDiamondFreeSpinTrigger(SpinResult trigger, float countUpDuration = 0f,
        System.Action<float> updateWinCountUp = null)
    {
        currentState = GameState.Stopping;
        uiManager.DisableControlsDuringWinAnimation();

        var bonusPositions = SlotView.FindBonusDiamondPositions(trigger.resultMatrix, gameConfig?.bonusSymbolId ?? -1);
        if (slotView != null && (bonusPositions.Count > 0 || updateWinCountUp != null))
            yield return slotView.AnimateBonusTrigger(bonusPositions, countUpDuration, updateWinCountUp);
        else
            updateWinCountUp?.Invoke(1f);

        if (lastResult == trigger)
            ProcessSpinResult(bonusPositions.Count);
    }

    // Legacy CNY scatter animation and sound.
    private IEnumerator DelayScatterTriggerResult()
    {
        // Play special feature trigger sound AFTER all reels have stopped
        AudioManager.Instance?.Play3UspinWinLineLoop();

        // Start scatter animations together AFTER all reels have stopped
        // Using 4 loops to match the 6-second delay (4 * 1.5s = 6s)
        slotView.AnimateAllScatters(4);

        // Wait for scatter hit animations to play
        yield return new WaitForSeconds(3.5f);
        ProcessSpinResult();
    }

    private IEnumerator DelayUSpinTriggerResult()
    {
        AudioManager.Instance?.Play3UspinWinLineLoop();

        bool animFinished = false;
        if (slotView != null)
        {
            slotView.AnimateUSpinWin(() =>
            {
                animFinished = true;
            });
        }
        else
        {
            animFinished = true;
        }

        yield return new WaitUntil(() => animFinished);

        uiManager.TriggerUSpinBonus(lastResult.uSpinData, () =>
        {
            AudioManager.Instance?.Stop3UspinWinLineLoop();
            lastResult.uSpinData.triggered = false;
            StartCoroutine(ResumeAfterSpecialFeature());
        });
    }

    private IEnumerator DelayMoneyBagTriggerResult()
    {
        AudioManager.Instance?.Play3UspinWinLineLoop();

        if (slotView != null)
        {
            slotView.AnimateMoneyBagWin();
        }

        yield return new WaitForSeconds(3.5f);

        uiManager.TriggerMoneyBagBonus(lastResult.moneyBagData, () =>
        {
            lastResult.moneyBagData.triggered = false;
            StartCoroutine(ResumeAfterSpecialFeature());
        });
    }

    private IEnumerator DelayBeforeNextRound()
    {
        var result = lastResult;
        float delayTime = currentSpinSpeed == SpinSpeed.QuickSpin ? 0.3f : 0.5f;
        yield return new WaitForSeconds(delayTime);

        // A delayed continuation belongs to the result that scheduled it.
        while (lastResult == result && (waitingForSpecialWin || uiManager.IsSpecialWinActive))
        {
            yield return null;
        }

        if (result != null && lastResult == result)
            ProcessSpinResult();
    }

    private float GetSpinDuration()
    {
        return currentSpinSpeed switch
        {
            SpinSpeed.Normal => normalSpinDuration,
            SpinSpeed.Turbo => turboSpinDuration,
            SpinSpeed.QuickSpin => quickSpinCycleDuration,
            _ => normalSpinDuration
        };
    }

    internal void OnSpinResultReceived(SpinResult result)
    {
        if (result.isDoubleDiamondResult)
        {
            // Only the first result for the active spin can alter its presentation.
            if (currentState != GameState.Spinning || lastResult != null)
                return;

            if (slotView != null && slotView.TryGetUnmappedResultSymbol(result, out int symbolId))
            {
                HandleUnmappedSpinSymbol(symbolId);
                return;
            }
        }

        lastResult = result;

        // Double Diamond shows the remaining count reported by the server.
        if (isInFreeSpins && result.isDoubleDiamondResult && result.isFreeSpin &&
            result.serverSpinsRemaining >= 0)
        {
            doubleDiamondFreeSpinsAwarded += result.freeSpinData?.spinsAwarded ?? 0;
            freeSpinsRemaining = result.serverSpinsRemaining;
            freeSpinsUsed = doubleDiamondFreeSpinsAwarded - freeSpinsRemaining;
            uiManager.UpdateDoubleDiamondFreeSpinCount(freeSpinsRemaining);
        }
        // Legacy CNY receives played and awarded counts directly.
        else if (isInFreeSpins && !result.isDoubleDiamondResult &&
                 result.serverSpinsRemaining >= 0)
        {
            freeSpinsRemaining = result.serverSpinsRemaining;
            freeSpinsUsed = result.serverSpinsUsed;
            int displayTotalSpins = result.serverTotalSpins;

            if (result.uSpinData != null && result.uSpinData.triggered && result.uSpinData.freeGamesAwarded > 0)
            {
                // Defer adding the newly won free spins to total count until wheel spin completes and user presses Take!
                displayTotalSpins -= result.uSpinData.freeGamesAwarded;
                freeSpinsRemaining -= result.uSpinData.freeGamesAwarded;
            }

            uiManager.UpdateFreeSpinCount(freeSpinsUsed, displayTotalSpins);
        }

        if (result.winLines != null)
        {
            for (int i = 0; i < result.winLines.Count; i++)
            {
                var line = result.winLines[i];

            }
        }
    }

    private void HandleUnmappedSpinSymbol(int symbolId)
    {
        Debug.LogError($"[GameManager] DD spin result contains unmapped symbol ID {symbolId}.");
        currentState = GameState.SpinResultError;

        if (spinCoroutine != null)
        {
            StopCoroutine(spinCoroutine);
            spinCoroutine = null;
        }

        slotView.CancelSpin();
        if (isAutoPlaying)
            StopAutoPlay();

        wasAutoPlayingBeforeFreeSpins = false;
        uiManager.DisableControlsDuringWinAnimation();
        popupManager?.ShowServerError("The spin result contains an unsupported symbol. Please exit and reopen the game.");
    }

    private void ProcessSpinResult(int bonusDiamondCount = 0)
    {
        playerData = lastResult.playerData;
        bool startsFreeSpins = lastResult.freeSpinData?.isTriggered == true && !isInFreeSpins;

        // Hold the balance for the bonus once its trigger presentation is ready to hand off.
        if (startsFreeSpins && lastResult.isDoubleDiamondResult)
            uiManager.HoldDoubleDiamondFreeSpinBalance();
        uiManager.OnSpinCompleted(lastResult);

        // Extract server-authoritative values before nullifying lastResult
        int serverSpinsRemaining = lastResult.serverSpinsRemaining;
        int serverSpinsUsed = lastResult.serverSpinsUsed;
        double serverTotalRoundWin = lastResult.serverTotalRoundWin;
        bool isRoundOver = lastResult.isRoundOver;
        bool isDoubleDiamondResult = lastResult.isDoubleDiamondResult;

        // Note: freeSpinsRemaining already updated in OnSpinResultReceived
        // Keeping this for safety in case OnSpinResultReceived wasn't called
        if (isInFreeSpins && freeSpinsRemaining != serverSpinsRemaining)
        {
            freeSpinsRemaining = serverSpinsRemaining;
        }


        // Check if free spins were just triggered (initial trigger from base game)
        if (startsFreeSpins)
        {
            var trigger = lastResult;
            lastResult = null;
            StartFreeSpins(trigger, bonusDiamondCount);
            return;
        }

        lastResult = null;

        if (isAutoPlaying && !isInFreeSpins)
        {
            if (autoPlayTotalRounds != -1)
            {
                autoPlayRemainingRounds--;
            }

            uiManager.UpdateAutoPlayCount();

            if (autoPlayTotalRounds != -1 && autoPlayRemainingRounds <= 0)
            {
                currentState = GameState.Idle;
                StopAutoPlay();
            }
            else
            {
                // Before requesting the next spin, verify the player can still afford it.
                // If not, stop autoplay (restores all UI) then show the popup.
                double totalPay = GetTotalPay();
                if (playerData.balance < totalPay)
                {
                    currentState = GameState.Idle;
                    StopAutoPlay();
                    if (popupManager != null) popupManager.ShowInsufficientFundsError();
                }
                else
                {
                    currentState = GameState.Idle;
                    RequestSpin();
                }
            }
        }
        else if (isInFreeSpins)
        {
            // Free spin counter already updated in OnSpinResultReceived
            // No need to update again here

            if (isRoundOver || freeSpinsRemaining <= 0)
            {
                EndFreeSpins(serverTotalRoundWin, serverSpinsUsed, isDoubleDiamondResult);
            }
            else
            {
                currentState = GameState.Idle;
                StartCoroutine(DelayBeforeNextFreeSpin());
            }
        }
        else
        {
            currentState = GameState.Idle;
        }
    }

    #endregion

    #region Spin Speed Control

    internal void SetSpinSpeed(SpinSpeed speed)
    {
        currentSpinSpeed = speed;
    }

    #endregion



    #region Auto Play

    internal void StartAutoPlay(int rounds)
    {
        if (currentState != GameState.Idle) return;

        // Check balance BEFORE locking any UI — if insufficient, show popup and bail.
        double totalPay = GetTotalPay();
        if (playerData.balance < totalPay)
        {
            if (popupManager != null) popupManager.ShowInsufficientFundsError();
            return;
        }

        isAutoPlaying = true;
        autoPlayTotalRounds = rounds;
        autoPlayRemainingRounds = rounds;
        wasAutoPlayingBeforeFreeSpins = false;

        uiManager.OnAutoPlayStarted();
        RequestSpin();
    }

    internal void StopAutoPlay()
    {
        isAutoPlaying = false;
        autoPlayRemainingRounds = 0;
        wasAutoPlayingBeforeFreeSpins = false;

        uiManager.OnAutoPlayStopped();
    }

    internal bool ShouldResumeAutoPlay()
    {
        return wasAutoPlayingBeforeFreeSpins && (savedAutoPlayTotalRounds == -1 || savedAutoPlayRemainingRounds > 0);
    }

    internal void ResumeAutoPlay()
    {
        if (!ShouldResumeAutoPlay()) return;

        int remaining = savedAutoPlayRemainingRounds;
        int total = savedAutoPlayTotalRounds;
        wasAutoPlayingBeforeFreeSpins = false;

        if (currentState != GameState.Idle) return;

        double totalPay = GetTotalPay();
        if (playerData.balance < totalPay)
        {
            if (popupManager != null) popupManager.ShowInsufficientFundsError();
            return;
        }

        isAutoPlaying = true;
        autoPlayTotalRounds = total;
        autoPlayRemainingRounds = remaining;

        uiManager.OnAutoPlayStarted();
        RequestSpin();
    }

    #endregion

    #region Free Spins

    private void StartFreeSpins(SpinResult trigger, int bonusDiamondCount)
    {
        int spins = trigger.freeSpinData.spinsAwarded;
        isInFreeSpins = true;
        freeSpinsRemaining = trigger.isDoubleDiamondResult ? trigger.freeSpinData.remainingSpins : spins;
        freeSpinsUsed = 0;
        doubleDiamondFreeSpinsAwarded = trigger.isDoubleDiamondResult ? spins : 0;
        waitingForFreeSpinStart = true;
        AudioManager.Instance?.PlayFreeSpinBg(trigger.isDoubleDiamondResult);

        int prevTotal = autoPlayTotalRounds;
        int prevRemaining = autoPlayRemainingRounds;

        if (isAutoPlaying)
        {
            StopAutoPlay();
            wasAutoPlayingBeforeFreeSpins = true;
            savedAutoPlayTotalRounds = prevTotal;
            savedAutoPlayRemainingRounds = (prevTotal != -1) ? (prevRemaining - 1) : -1;
        }

        currentState = GameState.Idle;
        if (trigger.isDoubleDiamondResult)
        {
            // DD carries the paid trigger's line wins through its UI totals.
            double triggerLineWin = trigger.winLines?.Sum(line => line.winAmount) ?? 0d;
            uiManager.OnDoubleDiamondFreeSpinsStarted(
                spins, freeSpinsRemaining, bonusDiamondCount, trigger.serverTotalRoundWin, triggerLineWin);
        }
        else
            uiManager.OnFreeSpinsStarted(spins); // Legacy CNY award popup.
    }

    internal void StartFirstFreeSpin()
    {
        if (firstFreeSpinCoroutine != null)
            return;

        waitingForFreeSpinStart = false;

        firstFreeSpinCoroutine = StartCoroutine(DelayBeforeFirstFreeSpin());
    }


    private IEnumerator DelayBeforeFirstFreeSpin()
    {
        yield return new WaitForSeconds(0.5f);
        firstFreeSpinCoroutine = null;
        RequestSpin();
    }

    private IEnumerator DelayBeforeNextFreeSpin()
    {
        yield return new WaitForSeconds(0.3f);

        // Wait for special win popup if it's still active or pending
        while (waitingForSpecialWin || uiManager.IsSpecialWinActive)
        {
            yield return null;
        }

        RequestSpin();
    }

    private void EndFreeSpins(double totalRoundWin, int totalSpinsUsed, bool isDoubleDiamondResult)
    {
        isInFreeSpins = false;
        freeSpinsRemaining = 0;
        doubleDiamondFreeSpinsAwarded = 0;
        AudioManager.Instance?.PlayMainBg();

        if (isDoubleDiamondResult)
        {
            // Double Diamond holds the next spin until the total-win popup and return transition finish.
            currentState = GameState.Stopping;
            uiManager.OnDoubleDiamondFreeSpinsEnded(totalRoundWin, () => currentState = GameState.Idle);
            return;
        }

        // Legacy CNY completes free spins through the universal popup.
        uiManager.OnFreeSpinsEnded(totalRoundWin, totalSpinsUsed);
        currentState = GameState.Idle;
    }

    #endregion

    #region Connection Events

    internal void OnDisconnected()
    {
        // Invalidate the Double Diamond result before stopping reel work or closing feature popups.
        currentState = GameState.Idle;
        if (lastResult?.isDoubleDiamondResult == true)
            lastResult = null;

        CancelDoubleDiamondJackpotPresentation();
        if (spinCoroutine != null)
        {
            StopCoroutine(spinCoroutine);
            spinCoroutine = null;
        }

        // Cancel a Start click that is still waiting to request the first free spin.
        if (firstFreeSpinCoroutine != null)
        {
            StopCoroutine(firstFreeSpinCoroutine);
            firstFreeSpinCoroutine = null;
        }
        uiManager?.HideDoubleDiamondFreeSpinStart();

        if (slotView != null)
            slotView.CancelSpin();
        else
            AudioManager.Instance?.StopDDReelAudio();

        wasAutoPlayingBeforeFreeSpins = false;
        if (isAutoPlaying)
        {
            StopAutoPlay();
        }

        uiManager?.CancelDoubleDiamondFreeSpinCompletion();

        // Note: The disconnection popup is shown by SocketIOManager.OnSocketDisconnected()
        // to avoid duplicates. GameManager only cleans up state here.
    }

    private void OnDisable()
    {
        if (jackpotPresentationCoroutine == null)
            return;

        CancelDoubleDiamondJackpotPresentation();
        slotView?.CancelSpin();
    }

    internal void ExitGame()
    {
        socketManager.CloseSocket();

    }

    #endregion

    #region Helper Methods

    // The symbol card and Double Diamond Info Page show payouts at the selected bet.
    internal IEnumerable<(int matchCount, double payout)> GetSymbolPayoutRows(
        SymbolInfo symbol, bool applySelectedBet = true)
    {
        if (symbol?.multipliers == null) yield break;

        double betFactor = 1d;
        if (applySelectedBet)
        {
            var bets = gameConfig?.availableBets;
            double fallbackBet = bets != null && currentBetIndex >= 0 && currentBetIndex < bets.Count
                ? bets[currentBetIndex]
                : currentBetIndex + 1;
            betFactor = currentBetAmount > 0 ? currentBetAmount : fallbackBet;
        }

        for (int i = 0; i < symbol.multipliers.Count; i++)
            yield return (5 - i, symbol.multipliers[i] * betFactor);
    }

    internal double GetTotalPay()
    {
        double multiplier = (gameConfig != null && gameConfig.totalBetMultiplier > 0) ? gameConfig.totalBetMultiplier : 25;
        return currentBetAmount * multiplier;
    }

    internal bool CanAffordBet()
    {
        double totalPay = GetTotalPay();
        return playerData.balance >= totalPay;
    }

    internal bool IsSpinning()
    {
        return currentState == GameState.Spinning || currentState == GameState.Stopping;
    }

    /// <summary>
    /// Returns true if at least one scatter symbol appears anywhere in the result matrix.
    /// Uses the server-configured scatterSymbolId (default 12) as the reference ID.
    /// </summary>
    private bool ResultMatrixHasScatter(List<List<int>> matrix)
    {
        if (matrix == null) return false;

        int scatterId = gameConfig != null ? gameConfig.scatterSymbolId : 12;

        foreach (var col in matrix)
        {
            if (col == null) continue;
            foreach (int sym in col)
            {
                if (sym == scatterId) return true;
            }
        }

        return false;
    }

    #endregion
}
