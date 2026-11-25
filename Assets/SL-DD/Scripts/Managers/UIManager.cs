using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PopupManager popupManager;
    [SerializeField] private JSFunctCalls jsFunctCalls;

    [Header("Loading & Intro")]
    [SerializeField] private GameObject gameScreen;
    [SerializeField] private GameObject gameLogoObject;



    [Header("Bet Controls")]
    [SerializeField] private TMP_Text betAmountText;
    [SerializeField] private Button betPlusButton;
    [SerializeField] private Button betMinusButton;
    [Header("Bet Controls - Portrait")]
    [SerializeField] private TMP_Text betAmountTextPortrait;
    [SerializeField] private Button betPlusButtonPortrait;
    [SerializeField] private Button betMinusButtonPortrait;

    [Header("Balance & Win")]
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text winAmountText;
    [SerializeField] private GameObject winTextObject;
    [SerializeField] private GameObject goodLuckObject;
    [Header("Balance & Win - Portrait")]
    [SerializeField] private TMP_Text balanceTextPortrait;
    [SerializeField] private TMP_Text winAmountTextPortrait;
    [SerializeField] private GameObject winTextObjectPortrait;
    [SerializeField] private GameObject goodLuckObjectPortrait;

    [Header("Shared Start Buttons")]
    // Double Diamond free spins and the legacy CNY wheel use the same Start buttons.
    [FormerlySerializedAs("wheelSpinButton")]
    [SerializeField] private Button sharedStartButton;
    [FormerlySerializedAs("wheelSpinButtonPortrait")]
    [SerializeField] private Button sharedStartButtonPortrait;

    [Header("Bonus Wheel")]
    [SerializeField] private WheelSpinController mainWheel;
    [SerializeField] private GameObject wheelScreen;
    [SerializeField] private Button wheelSpinButton2;
    [SerializeField] private Transform wheelTitleTransform;
    [SerializeField] private List<Transform> wheelTitleObjects = new List<Transform>();
    [SerializeField] private CanvasGroup transitionBackFilm;
    [SerializeField] private StarFountain wheelCoinFountain;
    [SerializeField] private Transform wheelAnticlockwiseRotatingObject;
    [SerializeField] private float wheelRotationDuration = 8f;
    private Tween wheelAnticlockwiseRotationTween;
    private List<Tween> wheelTitleTweens = new List<Tween>();
    private Dictionary<Transform, Vector3> wheelTitleInitialScales = new Dictionary<Transform, Vector3>();
    [Header("Bonus Wheel - Portrait")]
    [SerializeField] private Button wheelSpinButtonPortrait2;
    [SerializeField] private Transform wheelTitleTransformPortrait;
    
    [Header("Money Bag Bonus")]
    [SerializeField] private MoneyBagController moneyBagController;

    [Header("Universal Win Popup")]
    [SerializeField] private GameObject universalWinPopup;
    [SerializeField] private RectTransform universalWinPopupRect;
    [SerializeField] private GameObject uwpCongratulationsTitle;
    [SerializeField] private GameObject uwpYouWonSubtitle;
    [SerializeField] private GameObject uwpBigWinTitle;
    [SerializeField] private TMP_Text uwpWinAmountText;
    [SerializeField] private TMP_Text uwpFreeSpinCountText;
    [SerializeField] private GameObject uwpFreeSpinObject;
    [Header("DD and CNY Shared Win Take Buttons")]
    [FormerlySerializedAs("uwpTakeButton")]
    [SerializeField] private Button sharedWinTakeButton;
    [FormerlySerializedAs("uwpTakeButtonPortrait")]
    [SerializeField] private Button sharedWinTakeButtonPortrait;
    [Header("Universal Win Popup - Star Particle Burst")]
    [SerializeField] private StarFountain starFountain;

    [Header("Double Diamond Line Wins")]
    [SerializeField] private GameObject lineWinPopup;
    [SerializeField] private Image lineWinSymbolImage;
    [SerializeField] private TMP_Text lineWinNumberText;
    [SerializeField] private TMP_Text lineWinAmountText;
    [SerializeField] private OCController lineWinOrientationController;

    [Header("Spin Button")]
    [SerializeField] private Button spinButton;
    [SerializeField] private Button stopButton;
    [Header("Spin Button - Portrait")]
    [SerializeField] private Button spinButtonPortrait;
    [SerializeField] private Button stopButtonPortrait;

    [Header("Auto Play Stop Control")]
    [SerializeField] private Button autoSpinStopButton;
    [SerializeField] private TMP_Text autoSpinRemainingText;
    [Header("Auto Play Stop Control - Portrait")]
    [SerializeField] private Button autoSpinStopButtonPortrait;
    [SerializeField] private TMP_Text autoSpinRemainingTextPortrait;

    [Header("Auto Play Panel")]
    [SerializeField] private GameObject autoPlayPanel;
    [SerializeField] private RectTransform autoPlayPanelRect;
    [SerializeField] private Button autoPlayCloseButton;
    [Header("Auto Play Selection Buttons")]
    [SerializeField] private Button autoPlay10Button;
    [SerializeField] private Button autoPlay50Button;
    [SerializeField] private Button autoPlay100Button;
    [SerializeField] private Button autoPlay200Button;
    [SerializeField] private Button autoPlay500Button;
    [SerializeField] private Button autoPlayInfiniteButton;

    [Header("Auto Play Panel - Portrait")]
    [SerializeField] private GameObject autoPlayPanelPortrait;
    [SerializeField] private RectTransform autoPlayPanelRectPortrait;
    [SerializeField] private Button autoPlayCloseButtonPortrait;
    [SerializeField] private Button autoPlay10ButtonPortrait;
    [SerializeField] private Button autoPlay50ButtonPortrait;
    [SerializeField] private Button autoPlay100ButtonPortrait;
    [SerializeField] private Button autoPlay200ButtonPortrait;
    [SerializeField] private Button autoPlay500ButtonPortrait;
    [SerializeField] private Button autoPlayInfiniteButtonPortrait;

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private RectTransform settingsPanelRect;
    [SerializeField] private Button settingsOpenButton;
    [SerializeField] private Button settingsCloseButton;
    [SerializeField] private Button settingsBgCloseButton;
    [SerializeField] private Button gameQuitButton;
    [Header("Settings Panel - Portrait")]
    [SerializeField] private GameObject settingsPanelPortrait;
    [SerializeField] private RectTransform settingsPanelRectPortrait;
    [SerializeField] private Button settingsOpenButtonPortrait;
    [SerializeField] private Button settingsCloseButtonPortrait;
    [SerializeField] private Button settingsBgCloseButtonPortrait;
    [SerializeField] private Button gameQuitButtonPortrait;

    [Header("Speed Buttons (Three-Layer Toggle)")]
    [SerializeField] private Button normalSpeedButton;
    [SerializeField] private Button turboSpeedButton;
    [SerializeField] private Button quickSpeedButton;
    [Header("Speed Buttons - Portrait")]
    [SerializeField] private Button normalSpeedButtonPortrait;
    [SerializeField] private Button turboSpeedButtonPortrait;
    [SerializeField] private Button quickSpeedButtonPortrait;

    [Header("Sound Panel")]
    [SerializeField] private GameObject soundPanel;
    [SerializeField] private RectTransform soundPanelRect;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button soundPanelCloseButton;
    [SerializeField] private Button soundPanelOpenButton;
    [SerializeField] private Button soundPanelOpenButtonPortrait;

    [Header("Game Rules Panel")]
    [SerializeField] private GameObject gameRulesPanel;
    [SerializeField] private RectTransform gameRulesPanelRect;
    [SerializeField] private Button gameRulesOpenButton;
    [SerializeField] private Button gameRulesBackButton;
    [Header("Game Rules Panel - Portrait")]
    [SerializeField] private Button gameRulesOpenButtonPortrait;

    [Header("Guide Panel")]
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private RectTransform guidePanelRect;
    [SerializeField] private Button guideOpenButton;
    [SerializeField] private Button guideBackButton;
    [Header("Guide Panel - Portrait")]
    [SerializeField] private Button guideOpenButtonPortrait;

    [Header("Game Rules Dynamic Texts")]
    [SerializeField] private TMP_Text totalLineCountText;
    [SerializeField] private TMP_Text ruleSymbol0Text;
    [SerializeField] private TMP_Text ruleSymbol1Text;
    [SerializeField] private TMP_Text ruleSymbol2Text;
    [SerializeField] private TMP_Text ruleSymbol3Text;
    [SerializeField] private TMP_Text ruleSymbol4Text;
    [SerializeField] private TMP_Text ruleSymbol5Text;
    [SerializeField] private TMP_Text ruleSymbol6Text;
    [SerializeField] private TMP_Text ruleSymbol7Text;
    [SerializeField] private TMP_Text ruleSymbol8Text;
    [SerializeField] private TMP_Text ruleSymbol9Text;

    [Header("Double Diamond Win Display")]
    [SerializeField, Min(0f)] private float winDisplayCountUpDuration = 1.25f;
    internal float WinDisplayCountUpDuration => Mathf.Max(0f, winDisplayCountUpDuration);

    [Header("Free Spins UI Setup")]
    [SerializeField] private GameObject freeSpinTriggerPopup;
    [SerializeField] private RectTransform freeSpinTriggerPanel;
    [SerializeField] private List<GameObject> bonusDiamondDimmers = new List<GameObject>();
    [SerializeField] private TMP_Text freeSpinAwaredText;
    [SerializeField, Min(0f)] private float freeSpinTriggerPopupDuration = 5f;

    [SerializeField] private GameObject freeSpinCountContainer;
    [SerializeField] private TMP_Text remainingFreeSpinsText;

    [Header("DD Total Win Popup")]
    [FormerlySerializedAs("freeSpinTotalWinPopup")]
    [SerializeField] private GameObject totalWinPopup;
    [FormerlySerializedAs("freeSpinTotalWonTextImage")]
    [SerializeField] private GameObject freeSpinTotalWinTitle;
    [SerializeField] private GameObject jpotTotalWinTitle;
    [FormerlySerializedAs("freeSpinTotalFrame")]
    [SerializeField] private GameObject totalWinFrame;
    [FormerlySerializedAs("freeSpinTotalWonText")]
    [SerializeField] private TMP_Text totalWinAmountText;
    [FormerlySerializedAs("freeSpinTotalWinFountain")]
    [SerializeField] private FountainEmitter totalWinFountain;
    [FormerlySerializedAs("freeSpinTotalWinRipple")]
    [SerializeField] private RippleEmitter totalWinRipple;
    [FormerlySerializedAs("freeSpinTotalWinIntroDuration")]
    [SerializeField, Min(0f)] private float totalWinIntroDuration = 0.6f;
    [FormerlySerializedAs("freeSpinTotalWinCountUpDuration")]
    [SerializeField, Min(0f)] private float totalWinCountUpDuration = 1f;

    [Header("DD Total Win Pulse")]
    [SerializeField, Min(0.1f)] private float totalWinPulseDuration = 0.6f;
    [SerializeField, Range(0f, 0.2f)] private float totalWinPulseAmount = 0.05f;
    [SerializeField] private RectTransform totalWinPulseRays;
    [SerializeField, Min(0.01f)] private float totalWinRaysMinScaleX = 21f;
    [SerializeField, Min(0.01f)] private float totalWinRaysMaxScaleX = 26f;
    [SerializeField, Min(0.01f)] private float totalWinPortraitRaysMinScaleX = 38.58f;
    [SerializeField, Min(0.01f)] private float totalWinPortraitRaysMaxScaleX = 40.58f;
    private Tween ddTotalWinPulse;
    private Coroutine ddTotalWinPulseRestart;
    private Vector3 ddTotalWinImageBaseScale;
    private Vector3 ddTotalWinFrameBaseScale;
    private Vector3 ddTotalWinRaysBaseScale;

    [Header("Ping Display")]
    [SerializeField] private TMP_Text pingText;
    [SerializeField] private TMP_Text pingTextPortrait;

    [Header("DD Jackpot Payout Table")]
    [Tooltip("Payout labels ordered from 12 Inferno symbols at the top to 5 at the bottom.")]
    [SerializeField] private TMP_Text[] jackpotPayoutTexts = new TMP_Text[8];
    [SerializeField] private ImageAnimation jackpotPanelAnimation;
    [Tooltip("Row overlays ordered from 12 Inferno symbols at the top to 5 at the bottom.")]
    [SerializeField] private ImageAnimation[] jackpotRowAnimations = new ImageAnimation[8];
    [Tooltip("Seconds the jackpot payout panel and awarded row repeat their animations.")]
    [SerializeField, Min(0f)] private float jackpotHighlightDuration = 3f;
    private const float JackpotHighlightFramesPerSecond = 24f;
    private ImageAnimation activeJackpotRowAnimation;

    [Header("Platform Jackpot")]
    [SerializeField] private TMP_Text grandJackpotText;
    [SerializeField] private TMP_Text majorJackpotText;
    [SerializeField] private TMP_Text minorJackpotText;
    [SerializeField] private TMP_Text miniJackpotText;

    [Header("Platform Jackpot - Portrait")]
    [SerializeField] private TMP_Text grandJackpotTextPortrait;
    [SerializeField] private TMP_Text majorJackpotTextPortrait;
    [SerializeField] private TMP_Text minorJackpotTextPortrait;
    [SerializeField] private TMP_Text miniJackpotTextPortrait;

    [Header("Expand-Shrink Controls")]
    [SerializeField] private Button expandButton;
    [SerializeField] private Button shrinkButton;
    [Header("Expand-Shrink Controls - Portrait")]
    [SerializeField] private Button expandButtonPortrait;
    [SerializeField] private Button shrinkButtonPortrait;

    private bool isExpanded = false;
    private bool isSettingsPanelOpen = false;
    private bool isAutoPlayPanelOpen;

    private Tween balanceTween;
    private Tween winTween;
    private int totalFreeSpinsAwarded = 0;

    private int initialFreeSpins = 0;
    private bool holdDoubleDiamondFreeSpinBalance;
    private bool holdDoubleDiamondJackpotBalance;
    // DD adds the triggering paid spin's line wins to the displayed free-spin sequence total.
    private double ddFreeSpinTriggerLineWin;
    private bool doubleDiamondFreeSpinStartVisible;
    private System.Action ddTotalWinOnTake;
    private bool ddTotalWinTakeReady;
    private TotalWinTitle? openDDTotalWinTitle;
    private Sequence ddTotalWinPresentation;

    // Optimistic balance: the locally-deducted balance shown while the spin is in flight
    private double optimisticBalance = 0;
    private bool hasOptimisticBalance = false;

    [Header("Rapid Stop Cooldown")]
    [Tooltip("Seconds the player must wait before pressing Stop again after an immediate stop.")]
    [SerializeField] private float rapidStopCooldown = 1f;
    private float lastRapidStopTime = -99f;

    private int currentRulesPage = 0;
    private bool isPageAnimating;
    [Header("UI State")]
    private double currentWinDisplayValue = 0;
    private bool isSpecialWinActive = false;
    public bool IsSpecialWinActive => isSpecialWinActive;
    public System.Action OnSpecialWinComplete;

    // Universal Win Popup state
    private System.Action universalWinPopupCallback;
    private Coroutine uwpAutoCloseCoroutine;
    private Tween uwpWinTween;
    [SerializeField] private float uwpAutoCloseDelay = 5f;

    private void Awake()
    {
        if (jsFunctCalls != null)
        {
            jsFunctCalls.RegisterVisibilityListener(gameObject.name);
        }
    }



    public void OnFocusChanged(string value)
    {
        bool focused = value == "1";
        Debug.Log("UNITY FOCUS CHANGED: " + value + " (focused: " + focused + ")");
        AudioManager.Instance?.SetMuteAll(!focused);
        if (gameManager != null && gameManager.socketManager != null)
        {
            gameManager.socketManager.HandleFocusChange(focused);
        }
    }

    private void Start()
    {
        SetupButtons();
        SetupAutoPlayPanel();
        SetupSettingsPanel();
        SetupGameRulesPanel();
        SetupGuidePanel();

        InitializeExpandShrink();

        if (gameScreen) gameScreen.SetActive(true);
        InitializeUI();
        StartCoroutine(WaitForInitialization());
        RegisterFullscreenListener();
    }

    private void OnEnable()
    {
        OrientationChange.OnOrientationChanged += HandleOrientationChanged;
    }

    private void OnDisable()
    {
        OrientationChange.OnOrientationChanged -= HandleOrientationChanged;
        holdDoubleDiamondJackpotBalance = false;
        StopDoubleDiamondJackpotOverlays();
        CloseDDTotalWinPopup();
        AudioManager.Instance?.StopDDWinAudio();
    }

    private void HandleOrientationChanged(OrientationChange.OrientationMode mode, int width, int height)
    {
        UpdateNewWheelSpinButtonsVisibility();
        if (ddTotalWinTakeReady)
            StartCoroutine(RefreshDDTotalWinTakeAfterOrientationChange());

        if (totalWinPopup != null && totalWinPopup.activeInHierarchy &&
            (ddTotalWinPulse != null || ddTotalWinPulseRestart != null))
        {
            // DD lets OCController apply the new scales before restarting the pulse.
            StopDDTotalWinPulse(restoreScale: false);
            ddTotalWinPulseRestart = StartCoroutine(RestartDDTotalWinPulseAfterOrientationChange(mode));
        }
    }

    private void UpdateNewWheelSpinButtonsVisibility()
    {
        if (wheelScreen == null || !wheelScreen.activeInHierarchy || wheelSpinTriggered) return;

        var oc = Object.FindFirstObjectByType<OrientationChange>();
        bool isPortraitMode = (oc != null && oc.CurrentMode == OrientationChange.OrientationMode.MobilePortrait);

        if (wheelSpinButton2) wheelSpinButton2.gameObject.SetActive(!isPortraitMode);
        if (wheelSpinButtonPortrait2) wheelSpinButtonPortrait2.gameObject.SetActive(isPortraitMode);
    }

    private void InitializeUI()
    {
        if (soundPanel) soundPanel.SetActive(false);
        SetGameObjectActive(autoPlayPanel, autoPlayPanelPortrait, false);
        if (autoPlayPanelRect) autoPlayPanelRect.anchoredPosition = new Vector2(autoPlayPanelRect.anchoredPosition.x, -600f);
        if (autoPlayPanelRectPortrait) autoPlayPanelRectPortrait.anchoredPosition = new Vector2(autoPlayPanelRectPortrait.anchoredPosition.x, -600f);
        SetButtonActive(autoSpinStopButton, autoSpinStopButtonPortrait, false);

        SetSpinStopButtonStates(isSpinningState: false, isInteractable: true);
        UpdateSpeedButtonsVisibility(gameManager.currentSpinSpeed);

        isSettingsPanelOpen = false;
        SetGameObjectActive(settingsPanel, settingsPanelPortrait, false);
        if (gameRulesPanel) gameRulesPanel.SetActive(false);
        if (guidePanel) guidePanel.SetActive(false);
        if (uwpWinTween != null) { uwpWinTween.Kill(); uwpWinTween = null; }
        if (starFountain != null) starFountain.StopStarBurst();
        if (universalWinPopup) universalWinPopup.SetActive(false);

        if (freeSpinCountContainer) freeSpinCountContainer.SetActive(false);
        StopWheelBonusEffects();
        if (wheelScreen) wheelScreen.SetActive(false);
        SetButtonActive(sharedStartButton, sharedStartButtonPortrait, false);
        SetButtonActive(wheelSpinButton2, wheelSpinButtonPortrait2, false);
        if (transitionBackFilm) transitionBackFilm.gameObject.SetActive(false);
        UpdatePingDisplay("-- ms");
    }

    #region Loading & Intro Sequence

    private IEnumerator WaitForInitialization()
    {
        float initializationTimeout = 20f;
        float timer = 0f;
        while (!gameManager.isInitialized && !gameManager.initializationFailed && timer < initializationTimeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (gameManager.initializationFailed || !gameManager.isInitialized)
        {
            if (gameManager.socketManager != null)
            {
                gameManager.socketManager.SetRaycastBlocker(false);
            }

            if (popupManager != null)
            {
                string errorMsg = gameManager.initializationFailed ? "Game failed to initialize." : "Initialization timed out. Please check your connection.";
                popupManager.ShowErrorPopup("Connection Error", errorMsg, true);
            }
        }
        else
        {
            AudioManager.Instance?.PlayBgMusic();
        }
    }

    #endregion

    #region UI Synchronization Helpers

    private void SetTMPText(TMP_Text text1, TMP_Text text2, string content)
    {
        if (text1) text1.text = content;
        if (text2) text2.text = content;
    }

    private void SetGameObjectActive(GameObject obj1, GameObject obj2, bool active)
    {
        if (obj1) obj1.SetActive(active);
        if (obj2) obj2.SetActive(active);
    }

    private void SetButtonInteractable(Button btn1, Button btn2, bool interactable)
    {
        if (btn1) btn1.interactable = interactable;
        if (btn2) btn2.interactable = interactable;
    }

    private void SetButtonActive(Button btn1, Button btn2, bool active)
    {
        if (btn1) btn1.gameObject.SetActive(active);
        if (btn2) btn2.gameObject.SetActive(active);
    }

    #endregion

    #region Button Setup

    private void SetupButtons()
    {
        // Bet buttons
        if (betPlusButton)  betPlusButton.onClick.AddListener(() => gameManager.IncreaseBet());
        if (betMinusButton) betMinusButton.onClick.AddListener(() => gameManager.DecreaseBet());
        if (betPlusButtonPortrait)  betPlusButtonPortrait.onClick.AddListener(() => gameManager.IncreaseBet());
        if (betMinusButtonPortrait) betMinusButtonPortrait.onClick.AddListener(() => gameManager.DecreaseBet());

        // Spin button
        if (spinButton)
        {
            var holdHandler = spinButton.GetComponent<SpinButtonHoldHandler>();
            if (holdHandler != null)
            {
                holdHandler.OnClick.AddListener(OnSpinButtonPressed);
                holdHandler.OnHoldThreeSeconds.AddListener(OnSpinButtonHeld);
            }
            else
            {
                spinButton.onClick.AddListener(OnSpinButtonPressed);
            }
        }
        if (spinButtonPortrait)
        {
            var holdHandler = spinButtonPortrait.GetComponent<SpinButtonHoldHandler>();
            if (holdHandler != null)
            {
                holdHandler.OnClick.AddListener(OnSpinButtonPressed);
                holdHandler.OnHoldThreeSeconds.AddListener(OnSpinButtonHeld);
            }
            else
            {
                spinButtonPortrait.onClick.AddListener(OnSpinButtonPressed);
            }
        }

        // Stop button
        if (stopButton) stopButton.onClick.AddListener(OnStopButtonPressed);
        if (stopButtonPortrait) stopButtonPortrait.onClick.AddListener(OnStopButtonPressed);

        // Auto spin stop button
        if (autoSpinStopButton)
        {
            autoSpinStopButton.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayAutoplayStop();
                gameManager.StopAutoPlay();
            });
        }
        if (autoSpinStopButtonPortrait)
        {
            autoSpinStopButtonPortrait.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayAutoplayStop();
                gameManager.StopAutoPlay();
            });
        }

        if (autoPlayCloseButton) autoPlayCloseButton.onClick.AddListener(CloseAutoPlayPanel);
        if (autoPlayCloseButtonPortrait) autoPlayCloseButtonPortrait.onClick.AddListener(CloseAutoPlayPanel);

        if (gameQuitButton) gameQuitButton.onClick.AddListener(OnExitButtonPressed);
        if (gameQuitButtonPortrait) gameQuitButtonPortrait.onClick.AddListener(OnExitButtonPressed);

        if (expandButton) expandButton.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); OnExpand(); });
        if (shrinkButton) shrinkButton.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); OnShrink(); });
        if (expandButtonPortrait) expandButtonPortrait.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); OnExpand(); });
        if (shrinkButtonPortrait) shrinkButtonPortrait.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); OnShrink(); });

        if (sharedStartButton) sharedStartButton.onClick.AddListener(OnSharedStartButtonClicked);
        if (wheelSpinButton2) wheelSpinButton2.onClick.AddListener(OnWheelSpinClicked);
        if (sharedStartButtonPortrait) sharedStartButtonPortrait.onClick.AddListener(OnSharedStartButtonClicked);
        if (wheelSpinButtonPortrait2) wheelSpinButtonPortrait2.onClick.AddListener(OnWheelSpinClicked);

        // DD total wins and the legacy CNY popup share the Take buttons.
        if (sharedWinTakeButton) sharedWinTakeButton.onClick.AddListener(OnWinTakeButtonClicked);
        if (sharedWinTakeButtonPortrait) sharedWinTakeButtonPortrait.onClick.AddListener(OnWinTakeButtonClicked);

        // Speed buttons setup (Three-layer Toggle)
        if (normalSpeedButton) normalSpeedButton.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); SetSpeedMode(SpinSpeed.Turbo); });
        if (turboSpeedButton)  turboSpeedButton.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); SetSpeedMode(SpinSpeed.QuickSpin); });
        if (quickSpeedButton)  quickSpeedButton.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); SetSpeedMode(SpinSpeed.Normal); });
        if (normalSpeedButtonPortrait) normalSpeedButtonPortrait.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); SetSpeedMode(SpinSpeed.Turbo); });
        if (turboSpeedButtonPortrait)  turboSpeedButtonPortrait.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); SetSpeedMode(SpinSpeed.QuickSpin); });
        if (quickSpeedButtonPortrait)  quickSpeedButtonPortrait.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); SetSpeedMode(SpinSpeed.Normal); });
    }

    private void SetupAutoPlayPanel()
    {
        if (autoPlay10Button)       autoPlay10Button.onClick.AddListener(() => StartAutoplayWithRounds(10));
        if (autoPlay50Button)       autoPlay50Button.onClick.AddListener(() => StartAutoplayWithRounds(50));
        if (autoPlay100Button)      autoPlay100Button.onClick.AddListener(() => StartAutoplayWithRounds(100));
        if (autoPlay200Button)      autoPlay200Button.onClick.AddListener(() => StartAutoplayWithRounds(200));
        if (autoPlay500Button)      autoPlay500Button.onClick.AddListener(() => StartAutoplayWithRounds(500));
        if (autoPlayInfiniteButton) autoPlayInfiniteButton.onClick.AddListener(() => StartAutoplayWithRounds(-1));

        if (autoPlay10ButtonPortrait)       autoPlay10ButtonPortrait.onClick.AddListener(() => StartAutoplayWithRounds(10));
        if (autoPlay50ButtonPortrait)       autoPlay50ButtonPortrait.onClick.AddListener(() => StartAutoplayWithRounds(50));
        if (autoPlay100ButtonPortrait)      autoPlay100ButtonPortrait.onClick.AddListener(() => StartAutoplayWithRounds(100));
        if (autoPlay200ButtonPortrait)      autoPlay200ButtonPortrait.onClick.AddListener(() => StartAutoplayWithRounds(200));
        if (autoPlay500ButtonPortrait)      autoPlay500ButtonPortrait.onClick.AddListener(() => StartAutoplayWithRounds(500));
        if (autoPlayInfiniteButtonPortrait) autoPlayInfiniteButtonPortrait.onClick.AddListener(() => StartAutoplayWithRounds(-1));
    }

    private void SetupSettingsPanel()
    {
        if (settingsOpenButton) settingsOpenButton.onClick.AddListener(OnSettingsToggleClicked);
        if (settingsOpenButtonPortrait) settingsOpenButtonPortrait.onClick.AddListener(OnSettingsToggleClicked);

        if (settingsCloseButton) settingsCloseButton.onClick.AddListener(() => { 
            AudioManager.Instance?.PlayButton(); 
            CloseSettingsPanel(); 
        });
        if (settingsCloseButtonPortrait) settingsCloseButtonPortrait.onClick.AddListener(() => { 
            AudioManager.Instance?.PlayButton(); 
            CloseSettingsPanel(); 
        });
        if (settingsBgCloseButton) settingsBgCloseButton.onClick.AddListener(() => { 
            AudioManager.Instance?.PlayButton(); 
            CloseSettingsPanel(); 
        });
        if (settingsBgCloseButtonPortrait) settingsBgCloseButtonPortrait.onClick.AddListener(() => { 
            AudioManager.Instance?.PlayButton(); 
            CloseSettingsPanel(); 
        });

        SetButtonActive(settingsOpenButton, settingsOpenButtonPortrait, true);
        SetButtonActive(settingsCloseButton, settingsCloseButtonPortrait, false);
        SetButtonActive(settingsBgCloseButton, settingsBgCloseButtonPortrait, false);

        // Sound panel buttons & sliders
        if (soundPanelOpenButton) soundPanelOpenButton.onClick.AddListener(OpenSoundPanel);
        if (soundPanelOpenButtonPortrait) soundPanelOpenButtonPortrait.onClick.AddListener(OpenSoundPanel);
        if (soundPanelCloseButton) soundPanelCloseButton.onClick.AddListener(CloseSoundPanel);

        if (musicSlider)
        {
            if (AudioManager.Instance != null) musicSlider.value = AudioManager.Instance.MusicVolume;
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }
        if (sfxSlider)
        {
            if (AudioManager.Instance != null) sfxSlider.value = AudioManager.Instance.SfxVolume;
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }
    }

    private void SetupGameRulesPanel()
    {
        if (gameRulesOpenButton) gameRulesOpenButton.onClick.AddListener(OpenGameRulesPanel);
        if (gameRulesOpenButtonPortrait) gameRulesOpenButtonPortrait.onClick.AddListener(OpenGameRulesPanel);

        if (gameRulesBackButton) gameRulesBackButton.onClick.AddListener(() => { AudioManager.Instance?.PlayPopupClose(); CloseGameRulesPanel(); });
    }

    private void SetupGuidePanel()
    {
        if (guideOpenButton) guideOpenButton.onClick.AddListener(OpenGuidePanel);
        if (guideOpenButtonPortrait) guideOpenButtonPortrait.onClick.AddListener(OpenGuidePanel);

        if (guideBackButton) guideBackButton.onClick.AddListener(() => { AudioManager.Instance?.PlayPopupClose(); CloseGuidePanel(); });
    }

    #endregion

    #region Game Events

    internal void OnGameInitialized()
    {
        ddFreeSpinTriggerLineWin = 0d;
        currentWinDisplayValue = 0;
        UpdateBetDisplay();
        UpdateBalanceDisplay();
        UpdateWinDisplay(0);
    }

    internal void OnSpinStarted()
    {
#if UNITY_EDITOR
        CloseDDTotalWinPreview();
#endif
        AudioManager.Instance?.StopDDWinAudio();

        if (gameManager.isInFreeSpins)
        {
            HideDoubleDiamondFreeSpinStart();
            SetSpinStopButtonStates(isSpinningState: true, isInteractable: false);
        }
        else
        {
            ddFreeSpinTriggerLineWin = 0d;
            SetSpinStopButtonStates(isSpinningState: true, isInteractable: true);
            SetBetControlsEnabled(false);
            SetButtonInteractable(settingsOpenButton, settingsOpenButtonPortrait, true);
            UpdateWinDisplay(0);
        }

        UpdateBalanceDisplay();

        CloseAutoPlayPanelImmediate();
    }

    internal void OnSpinResultReceived()
    {
        if (gameManager.isAutoPlaying)
        {
            SetSpinStopButtonStates(isSpinningState: true, isInteractable: true);
        }
        else if (gameManager.isInFreeSpins)
        {
            SetSpinStopButtonStates(isSpinningState: true, isInteractable: false);
        }
        else
        {
            SetSpinStopButtonStates(isSpinningState: true, isInteractable: true);
        }
    }

    internal void OnSpinStopping(SpinResult result = null)
    {
        UpdateBalanceDisplay();
        if (result != null)
            UpdateWinDisplay(GetSpinDisplayWin(result));
    }

    internal System.Action<float> BeginWinDisplayCountUp(SpinResult result)
    {
        UpdateBalanceDisplay();
        bool isFreeSpin = result.isFreeSpin || gameManager?.isInFreeSpins == true;
        // DD free spins count up from the previous displayed sequence total.
        double start = isFreeSpin ? currentWinDisplayValue : 0d;
        double target = GetSpinDisplayWin(result);
        return BeginWinDisplayCountUp(start, target);
    }

    internal System.Action<float> BeginDoubleDiamondJackpotLineCountUp(SpinResult result)
    {
        // The jackpot amount is revealed separately. Keep the previous bonus total as the starting point.
        double start = result.isFreeSpin ? currentWinDisplayValue : 0d;
        if (result.linesWinAmount > 0d)
            return BeginWinDisplayCountUp(start, start + result.linesWinAmount);

        UpdateWinDisplay(start, forceWinVisible: true);
        return null;
    }

    internal void HoldDoubleDiamondJackpotBalance() => holdDoubleDiamondJackpotBalance = true;

    internal void RevealDoubleDiamondJackpotWin(SpinResult result)
    {
        holdDoubleDiamondJackpotBalance = false;
        UpdateWinDisplay(GetSpinDisplayWin(result));
        // A jackpot Take reveals the settled balance even during the free-spin balance hold.
        SetBalanceDisplay(result.playerData.balance);
    }

    private System.Action<float> BeginWinDisplayCountUp(double start, double target)
    {
        UpdateWinDisplay(start, forceWinVisible: true);

        AudioManager.Instance?.PlayDDWinCountUp();
        bool completed = false;
        return progress =>
        {
            UpdateWinDisplay(start + (target - start) * progress, forceWinVisible: true);
            if (progress >= 1f && !completed)
            {
                completed = true;
                AudioManager.Instance?.CompleteDDWinCountUp();
            }
        };
    }

    internal void OnSpinCompleted(SpinResult result = null)
    {
        if (result != null)
            UpdateWinDisplay(GetSpinDisplayWin(result));
        UpdateBalanceDisplay();

        if (gameManager.isAutoPlaying)
        {
            SetSpinStopButtonStates(isSpinningState: true, isInteractable: true);
        }
        else if (gameManager.isInFreeSpins)
        {
            SetSpinStopButtonStates(isSpinningState: true, isInteractable: false);
        }
        else
        {
            SetSpinStopButtonStates(isSpinningState: false, isInteractable: true);

            SetBetControlsEnabled(true);
            SetButtonInteractable(settingsOpenButton, settingsOpenButtonPortrait, true);
        }
    }

    private double GetSpinDisplayWin(SpinResult result)
    {
        if (result.isFreeSpin || gameManager?.isInFreeSpins == true)
            return result.serverTotalRoundWin + (result.isDoubleDiamondResult ? ddFreeSpinTriggerLineWin : 0d);

        return result.winAmount;
    }

    // Double Diamond displays these details while the active winning line is highlighted.
    internal void ShowLineWinPopup(WinLine line, Sprite symbol)
    {
        if (lineWinPopup == null) return;

        if (lineWinSymbolImage != null) lineWinSymbolImage.sprite = symbol;
        if (lineWinNumberText != null) lineWinNumberText.text = FormatWinDigits(line.lineId + 1);
        if (lineWinAmountText != null) lineWinAmountText.text = FormatWinDigits(line.winAmount);

        if (!lineWinPopup.activeSelf)
        {
            lineWinPopup.SetActive(true);
            lineWinOrientationController?.SetLineWinPopupVisible(true);
        }
    }

    internal void CloseLineWinPopup()
    {
        if (lineWinPopup == null || !lineWinPopup.activeSelf) return;

        lineWinPopup.SetActive(false);
        lineWinOrientationController?.SetLineWinPopupVisible(false);
    }

    internal void SetWinSpriteDigits(TMP_Text label, double value)
    {
        var spriteAsset = lineWinAmountText != null ? lineWinAmountText.spriteAsset : null;
        if (spriteAsset != null)
            label.spriteAsset = spriteAsset;

        label.text = label.spriteAsset != null ? FormatWinDigits(value) : value.ToString("0.###");
    }

    private static string FormatWinDigits(double value)
    {
        return FormatWinDigits(value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
    }

    private static string FormatWinDigits(string formatted)
    {
        var digits = new System.Text.StringBuilder();
        foreach (char character in formatted)
        {
            if (character == '.')
            {
                digits.Append("<sprite index=10>"); // Decimal point.
            }
            else if (character >= '0' && character <= '9')
            {
                digits.Append("<sprite index=").Append(character).Append('>');
            }
            else
            {
                digits.Append(character);
            }
        }

        return digits.ToString();
    }

    // Legacy CNY universal popup used by the existing Big Win flow.
    internal void TriggerBigWinPopup(SpinResult result, System.Action onComplete = null)
    {
        double winAmount = (result != null) ? result.winAmount : 0;
        ShowUniversalWinPopup(WinPopupType.BigWin, winAmount, 0, onComplete);
    }

    internal void DisableControlsDuringWinAnimation()
    {
        SetBetControlsEnabled(false);
        SetSpinStopButtonStates(isSpinningState: false, isInteractable: false);
    }

    internal void EnableControlsAfterWinAnimation()
    {
        if (isSpecialWinActive) return;

        if (gameManager.isAutoPlaying)
        {
            SetSpinStopButtonStates(isSpinningState: true, isInteractable: true);
        }
        else if (gameManager.isInFreeSpins)
        {
            SetSpinStopButtonStates(isSpinningState: true, isInteractable: false);
        }
        else
        {
            SetBetControlsEnabled(true);
            SetButtonInteractable(settingsOpenButton, settingsOpenButtonPortrait, true);
            SetSpinStopButtonStates(isSpinningState: false, isInteractable: true);
        }
    }

    #endregion

    #region Spin Button

    public void OnSpinButtonPressed()
    {
        if (gameManager.isAutoPlaying)
        {
            AudioManager.Instance?.PlayAutoplayStop();
            gameManager.StopAutoPlay();
            return;
        }

        if (!gameManager.IsSpinning())
        {
            gameManager.RequestSpin(playButtonCue: true);
        }
    }

    private void OnStopButtonPressed()
    {
        if (gameManager.isAutoPlaying)
        {
            AudioManager.Instance?.PlayAutoplayStop();
            gameManager.StopAutoPlay();
            return;
        }

        if (gameManager.IsSpinning())
        {
            // Rapid-stop cooldown: prevent the player from spamming the stop button
            if (Time.unscaledTime - lastRapidStopTime < rapidStopCooldown)
                return;

            lastRapidStopTime = Time.unscaledTime;
            AudioManager.Instance?.PlaySpinStop();
            gameManager.RequestStop();
        }
    }

    internal void DisableSpinButtonDuringStop()
    {
        if (gameManager.isAutoPlaying)
        {
            SetButtonActive(spinButton, spinButtonPortrait, false);
            SetButtonActive(stopButton, stopButtonPortrait, false);
            SetButtonActive(autoSpinStopButton, autoSpinStopButtonPortrait, true);
            SetButtonInteractable(autoSpinStopButton, autoSpinStopButtonPortrait, false);
        }
        else
        {
            SetButtonActive(autoSpinStopButton, autoSpinStopButtonPortrait, false);
            SetButtonActive(spinButton, spinButtonPortrait, false);
            SetButtonActive(stopButton, stopButtonPortrait, true);
            SetButtonInteractable(stopButton, stopButtonPortrait, false);
        }
    }

    internal void SetSpinStopButtonStates(bool isSpinningState, bool isInteractable)
    {
        if (gameManager.isAutoPlaying)
        {
            SetButtonActive(spinButton, spinButtonPortrait, false);
            SetButtonActive(stopButton, stopButtonPortrait, false);
            SetButtonActive(autoSpinStopButton, autoSpinStopButtonPortrait, true);
            SetButtonInteractable(autoSpinStopButton, autoSpinStopButtonPortrait, isInteractable);
        }
        else
        {
            SetButtonActive(autoSpinStopButton, autoSpinStopButtonPortrait, false);
            
            if (isSpinningState)
            {
                SetButtonActive(spinButton, spinButtonPortrait, false);
                SetButtonActive(stopButton, stopButtonPortrait, true);
                SetButtonInteractable(stopButton, stopButtonPortrait, isInteractable);
            }
            else
            {
                SetButtonActive(stopButton, stopButtonPortrait, false);
                SetButtonActive(spinButton, spinButtonPortrait, true);
                SetButtonInteractable(spinButton, spinButtonPortrait, isInteractable);
            }
        }
    }

    #endregion

    #region Bet Controls

    internal void UpdateBetDisplay()
    {
        if (gameManager.gameConfig == null) return;

        double totalPay = gameManager.GetTotalPay();

        if (betAmountText) betAmountText.text = FormatAmount(totalPay);
        if (betAmountTextPortrait) betAmountTextPortrait.text = "TOTAL PAY : " + FormatAmount(totalPay);

        UpdateBetButtonStates();
        UpdateGameRulesDynamicTexts();
        UpdateJackpotPayoutTable();
    }

    // DD payout rows are ordered from 12 Infernos down to 5.
    private const int TopJackpotScatterCount = 12;

    private void UpdateJackpotPayoutTable()
    {
        var tiers = gameManager.gameConfig.jackpotTiers;
        if (tiers == null || jackpotPayoutTexts == null)
            return;

        double totalBet = gameManager.GetTotalPay();
        bool useThreeDecimals = tiers.Exists(tier => tier != null && tier.minScatters >= 5 && tier.minScatters <= 12 &&
            System.Math.Round(tier.baseMultiplier * totalBet, 3) != System.Math.Round(tier.baseMultiplier * totalBet, 2));
        string numberFormat = useThreeDecimals ? "0.000" : "0.00";

        for (int i = 0; i < jackpotPayoutTexts.Length; i++)
        {
            TMP_Text label = jackpotPayoutTexts[i];
            if (label == null)
                continue;

            int scatterCount = TopJackpotScatterCount - i;
            SlotJackpotTier tier = tiers.Find(item => item != null && item.minScatters == scatterCount);
            label.text = tier == null ? string.Empty :
                FormatWinDigits((tier.baseMultiplier * totalBet).ToString(numberFormat, System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    internal IEnumerator PlayDoubleDiamondJackpotOverlays(JackpotBonusData jackpot)
    {
        float duration = Mathf.Max(0f, jackpotHighlightDuration);
        if (duration == 0f || !isActiveAndEnabled)
            yield break;

        // Use the awarded tier's table row, even if the result has a different scatter count.
        var tier = gameManager?.gameConfig?.jackpotTiers?.Find(item => item != null && item.tier == jackpot.tier);
        if (tier?.minScatters is int scatterCount)
        {
            int rowIndex = TopJackpotScatterCount - scatterCount;
            if (jackpotRowAnimations != null && rowIndex >= 0 && rowIndex < jackpotRowAnimations.Length)
                activeJackpotRowAnimation = jackpotRowAnimations[rowIndex];
        }

        bool panelStarted = TryPlayJackpotOverlay(jackpotPanelAnimation);
        bool rowStarted = TryPlayJackpotOverlay(activeJackpotRowAnimation);
        if (panelStarted || rowStarted)
            yield return new WaitForSeconds(duration);

        StopDoubleDiamondJackpotOverlays();
    }

    private static bool TryPlayJackpotOverlay(ImageAnimation animation)
    {
        if (animation == null)
            return false;

        animation.gameObject.SetActive(true);
        if (animation.TryStartAtFrameRate(JackpotHighlightFramesPerSecond, loopOverride: true))
            return true;

        animation.gameObject.SetActive(false);
        return false;
    }

    private void StopDoubleDiamondJackpotOverlays()
    {
        if (jackpotPanelAnimation != null)
            jackpotPanelAnimation.gameObject.SetActive(false);
        if (activeJackpotRowAnimation != null)
            activeJackpotRowAnimation.gameObject.SetActive(false);
        activeJackpotRowAnimation = null;
    }

    internal void CancelDoubleDiamondJackpotPresentation()
    {
        holdDoubleDiamondJackpotBalance = false;
        StopDoubleDiamondJackpotOverlays();
        if (openDDTotalWinTitle == TotalWinTitle.Jackpot)
            CloseDDTotalWinPopup();
        AudioManager.Instance?.StopDDWinAudio();
    }

    private void UpdateBetButtonStates()
    {
        SetButtonInteractable(betMinusButton, betMinusButtonPortrait, true);
        SetButtonInteractable(betPlusButton, betPlusButtonPortrait, true);
    }

    #endregion

    #region Auto Play Panel

    public void OnSpinButtonHeld()
    {
        if (gameManager.currentState == GameState.Idle && !gameManager.isAutoPlaying)
        {
            OpenAutoPlayPanel();
        }
    }

    private void OpenAutoPlayPanel()
    {
        if (isAutoPlayPanelOpen) return;

        isAutoPlayPanelOpen = true;
        AudioManager.Instance?.PlayAutoplayPanelToggle();
        if (isSettingsPanelOpen)
            CloseSettingsPanelImmediate();

        SetGameObjectActive(autoPlayPanel, autoPlayPanelPortrait, true);
        if (autoPlayPanelRect)
        {
            autoPlayPanelRect.DOKill();
            autoPlayPanelRect.anchoredPosition = new Vector2(autoPlayPanelRect.anchoredPosition.x, -600f);
            autoPlayPanelRect.DOAnchorPosY(0f, 0.35f).SetEase(Ease.OutCubic);
        }
        if (autoPlayPanelRectPortrait)
        {
            autoPlayPanelRectPortrait.DOKill();
            autoPlayPanelRectPortrait.anchoredPosition = new Vector2(autoPlayPanelRectPortrait.anchoredPosition.x, -600f);
            autoPlayPanelRectPortrait.DOAnchorPosY(0f, 0.35f).SetEase(Ease.OutCubic);
        }
    }

    private void CloseAutoPlayPanel()
    {
        if (!isAutoPlayPanelOpen) return;

        isAutoPlayPanelOpen = false;
        AudioManager.Instance?.PlayAutoplayPanelToggle();

        if (autoPlayPanelRect)
        {
            autoPlayPanelRect.DOKill();
            autoPlayPanelRect.DOAnchorPosY(-600f, 0.35f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                if (autoPlayPanel) autoPlayPanel.SetActive(false);
            });
        }
        else
        {
            if (autoPlayPanel) autoPlayPanel.SetActive(false);
        }

        if (autoPlayPanelRectPortrait)
        {
            autoPlayPanelRectPortrait.DOKill();
            autoPlayPanelRectPortrait.DOAnchorPosY(-600f, 0.35f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                if (autoPlayPanelPortrait) autoPlayPanelPortrait.SetActive(false);
            });
        }
        else
        {
            if (autoPlayPanelPortrait) autoPlayPanelPortrait.SetActive(false);
        }
    }

    private void StartAutoplayWithRounds(int rounds)
    {
        CloseAutoPlayPanel();
        gameManager.StartAutoPlay(rounds);
    }

    internal void OnAutoPlayStarted()
    {
        UpdateAutoPlayCount();
        SetSpinStopButtonStates(isSpinningState: true, isInteractable: true);
        SetBetControlsEnabled(false);
    }

    internal void OnAutoPlayStopped()
    {
        SetButtonActive(autoSpinStopButton, autoSpinStopButtonPortrait, false);

        bool isRoundActive = gameManager.IsSpinning() || gameManager.lastResult != null;

        if (!isRoundActive && !gameManager.isInFreeSpins)
        {
            SetSpinStopButtonStates(isSpinningState: false, isInteractable: true);
            SetBetControlsEnabled(true);
            SetButtonInteractable(settingsOpenButton, settingsOpenButtonPortrait, true);
        }
        else if (isRoundActive)
        {
            SetButtonActive(spinButton, spinButtonPortrait, false);
            SetButtonActive(stopButton, stopButtonPortrait, false);
            SetButtonActive(autoSpinStopButton, autoSpinStopButtonPortrait, true);
            SetButtonInteractable(autoSpinStopButton, autoSpinStopButtonPortrait, false);
        }
    }

    internal void UpdateAutoPlayCount()
    {
        string displayStr = "";
        if (gameManager.autoPlayTotalRounds == -1 || gameManager.autoPlayRemainingRounds < 0)
        {
            displayStr = "∞";
        }
        else
        {
            displayStr = $"{gameManager.autoPlayRemainingRounds}";
        }

        SetTMPText(autoSpinRemainingText, autoSpinRemainingTextPortrait, displayStr);
    }

    #endregion

    #region Spin Speed Universal Toggle Logic

    public void SetSpeedMode(SpinSpeed speed)
    {
        gameManager.SetSpinSpeed(speed);
        UpdateSpeedButtonsVisibility(speed);
    }

    private void UpdateSpeedButtonsVisibility(SpinSpeed speed)
    {
        SetButtonActive(normalSpeedButton, normalSpeedButtonPortrait, speed == SpinSpeed.Normal);
        SetButtonActive(turboSpeedButton, turboSpeedButtonPortrait, speed == SpinSpeed.Turbo);
        SetButtonActive(quickSpeedButton, quickSpeedButtonPortrait, speed == SpinSpeed.QuickSpin);
    }

    #endregion

    #region Sound Panel

    private void OpenSoundPanel()
    {
        AudioManager.Instance?.PlayButton();
        if (soundPanel == null) return;
        soundPanel.SetActive(true);
        if (soundPanelRect != null)
        {
            AnimatePopupOpen(soundPanelRect);
        }
        if (musicSlider && AudioManager.Instance != null)
        {
            musicSlider.value = AudioManager.Instance.MusicVolume;
        }
        if (sfxSlider && AudioManager.Instance != null)
        {
            sfxSlider.value = AudioManager.Instance.SfxVolume;
        }
    }

    private void CloseSoundPanel()
    {
        if (soundPanel == null || !soundPanel.activeSelf) return;
        if (soundPanelRect != null)
        {
            AnimatePopupClose(soundPanelRect, () =>
            {
                soundPanel.SetActive(false);
            });
        }
        else
        {
            AudioManager.Instance?.PlayButton();
            soundPanel.SetActive(false);
        }
    }

    private void OnMusicSliderChanged(float val)
    {
        AudioManager.Instance?.SetMusicVolume(val);
    }

    private void OnSfxSliderChanged(float val)
    {
        AudioManager.Instance?.SetSfxVolume(val);
    }

    #endregion

    #region Settings Panel

    private void OnSettingsToggleClicked()
    {
        if (isAutoPlayPanelOpen)
            CloseAutoPlayPanelImmediate(playCue: true);
        else
            AudioManager.Instance?.PlayButton();

        if (isSettingsPanelOpen)
            CloseSettingsPanel();
        else
            OpenSettingsPanel();
    }

    private void OpenSettingsPanel()
    {
        if ((autoPlayPanel && autoPlayPanel.activeSelf) || (autoPlayPanelPortrait && autoPlayPanelPortrait.activeSelf))
            CloseAutoPlayPanelImmediate();

        isSettingsPanelOpen = true;

        SetButtonActive(settingsOpenButton, settingsOpenButtonPortrait, false);
        SetButtonActive(settingsCloseButton, settingsCloseButtonPortrait, true);
        SetButtonActive(settingsBgCloseButton, settingsBgCloseButtonPortrait, true);

        if (settingsPanel)
        {
            settingsPanel.SetActive(true);
            CanvasGroup cg = settingsPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = settingsPanel.AddComponent<CanvasGroup>();
            cg.DOKill();
            cg.DOFade(1f, 0.35f);
        }

        if (settingsPanelPortrait)
        {
            settingsPanelPortrait.SetActive(true);
            CanvasGroup cg = settingsPanelPortrait.GetComponent<CanvasGroup>();
            if (cg == null) cg = settingsPanelPortrait.AddComponent<CanvasGroup>();
            cg.DOKill();
            cg.DOFade(1f, 0.35f);
        }
    }

    private void CloseSettingsPanel()
    {
        isSettingsPanelOpen = false;

        SetButtonActive(settingsOpenButton, settingsOpenButtonPortrait, true);
        SetButtonActive(settingsCloseButton, settingsCloseButtonPortrait, false);
        SetButtonActive(settingsBgCloseButton, settingsBgCloseButtonPortrait, false);

        if (settingsPanel)
        {
            CanvasGroup cg = settingsPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = settingsPanel.AddComponent<CanvasGroup>();
            cg.DOKill();
            cg.DOFade(0f, 0.35f).OnComplete(() =>
            {
                settingsPanel.SetActive(false);
            });
        }

        if (settingsPanelPortrait)
        {
            CanvasGroup cg = settingsPanelPortrait.GetComponent<CanvasGroup>();
            if (cg == null) cg = settingsPanelPortrait.AddComponent<CanvasGroup>();
            cg.DOKill();
            cg.DOFade(0f, 0.35f).OnComplete(() =>
            {
                settingsPanelPortrait.SetActive(false);
            });
        }
    }

    private void CloseSettingsPanelImmediate()
    {
        isSettingsPanelOpen = false;

        SetButtonActive(settingsOpenButton, settingsOpenButtonPortrait, true);
        SetButtonActive(settingsCloseButton, settingsCloseButtonPortrait, false);
        SetButtonActive(settingsBgCloseButton, settingsBgCloseButtonPortrait, false);

        if (settingsPanel)
        {
            CanvasGroup cg = settingsPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.DOKill();
                cg.alpha = 0f;
            }
            settingsPanel.SetActive(false);
        }

        if (settingsPanelPortrait)
        {
            CanvasGroup cg = settingsPanelPortrait.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.DOKill();
                cg.alpha = 0f;
            }
            settingsPanelPortrait.SetActive(false);
        }
    }

    private void CloseAutoPlayPanelImmediate(bool playCue = false)
    {
        if (isAutoPlayPanelOpen && playCue)
            AudioManager.Instance?.PlayAutoplayPanelToggle();
        isAutoPlayPanelOpen = false;
        if (autoPlayPanelRect)
        {
            autoPlayPanelRect.DOKill();
            autoPlayPanelRect.localScale = Vector3.one;
        }
        if (autoPlayPanelRectPortrait)
        {
            autoPlayPanelRectPortrait.DOKill();
            autoPlayPanelRectPortrait.localScale = Vector3.one;
        }
        SetGameObjectActive(autoPlayPanel, autoPlayPanelPortrait, false);
    }

    #endregion

    #region Game Rules Panel

    private void OpenGameRulesPanel()
    {
        AudioManager.Instance?.PlayButton();
        if (isSettingsPanelOpen)
        {
            CloseSettingsPanelImmediate();
        }
        ShowGameRulesPanel();
    }

    private void ShowGameRulesPanel()
    {
        if (gameRulesPanel == null) return;
        gameRulesPanel.SetActive(true);
    }

    private void CloseGameRulesPanel()
    {
        if (gameRulesPanel == null) return;
        gameRulesPanel.SetActive(false);
    }

    #endregion

    #region Guide Panel

    private void OpenGuidePanel()
    {
        AudioManager.Instance?.PlayButton();
        if (isSettingsPanelOpen)
        {
            CloseSettingsPanelImmediate();
        }
        ShowGuidePanel();
    }

    private void ShowGuidePanel()
    {
        if (guidePanel == null) return;
        guidePanel.SetActive(true);
    }

    private void CloseGuidePanel()
    {
        if (guidePanel == null) return;
        guidePanel.SetActive(false);
    }

    #endregion

    #region Free Spins Flow

    internal void HoldDoubleDiamondFreeSpinBalance()
    {
        holdDoubleDiamondFreeSpinBalance = true;
    }

    internal void OnDoubleDiamondFreeSpinsStarted(int spinsAwarded, int spinsRemaining, int bonusDiamondCount,
        double roundWin, double triggerLineWin)
    {
        ddFreeSpinTriggerLineWin = triggerLineWin;
        DisableControlsDuringWinAnimation();
        StartCoroutine(ShowDoubleDiamondFreeSpinTrigger(
            spinsAwarded, spinsRemaining, bonusDiamondCount, roundWin + ddFreeSpinTriggerLineWin));
    }

    private IEnumerator ShowDoubleDiamondFreeSpinTrigger(int spinsAwarded, int spinsRemaining, int bonusDiamondCount, double displayWin)
    {
        if (gameLogoObject) gameLogoObject.SetActive(false);
        lineWinOrientationController?.SetDoubleDiamondFreeSpinPresentation(true);
        if (freeSpinTriggerPopup != null)
        {
            if (freeSpinAwaredText != null)
                freeSpinAwaredText.text = FormatWinDigits(spinsAwarded);

            // Double Diamond keeps all five diamonds visible and dims the unawarded positions.
            for (int i = 0; i < bonusDiamondDimmers.Count; i++)
            {
                var dimmer = bonusDiamondDimmers[i];
                if (dimmer != null)
                    dimmer.SetActive(i >= bonusDiamondCount);
            }

            yield return PlayDoubleDiamondFreeSpinTriggerPopup();
        }

        lineWinOrientationController?.SetDoubleDiamondFreeSpinPresentation(true, displayVisible: true);
        UpdateDoubleDiamondFreeSpinCount(spinsRemaining);
        UpdateWinDisplay(displayWin);

        // Reuse the Start controls already positioned over Spin in both orientations.
        doubleDiamondFreeSpinStartVisible = true;
        SetButtonActive(spinButton, spinButtonPortrait, false);
        SetButtonActive(sharedStartButton, sharedStartButtonPortrait, true);
        SetButtonInteractable(sharedStartButton, sharedStartButtonPortrait, true);
    }

    private IEnumerator PlayDoubleDiamondFreeSpinTriggerPopup()
    {
        var panel = freeSpinTriggerPanel;
        Vector3 restScale = panel != null ? panel.localScale : Vector3.one;
        if (panel != null)
            panel.localScale = Vector3.zero;

        freeSpinTriggerPopup.SetActive(true);
        gameManager?.FadeBonusReelOutlines();

        if (panel != null)
        {
            // Reuse the legacy CNY universal popup's scale and easing on the DD panel.
            const float closeOvershootDuration = 0.1f;
            const float closeShrinkDuration = 0.2f;
            var transition = DOTween.Sequence().SetLink(freeSpinTriggerPopup);
            transition.Append(panel.DOScale(restScale * 1.2f, 0.5f).SetEase(Ease.OutCubic));
            transition.Append(panel.DOScale(restScale, 0.3f).SetEase(Ease.InOutSine));
            float holdDuration = Mathf.Max(0f, freeSpinTriggerPopupDuration - transition.Duration()
                - closeOvershootDuration - closeShrinkDuration);
            transition.AppendInterval(holdDuration);
            transition.Append(panel.DOScale(restScale * 1.1f, closeOvershootDuration));
            transition.Append(panel.DOScale(Vector3.zero, closeShrinkDuration).SetEase(Ease.InBack));
            yield return transition.WaitForCompletion(returnCustomYieldInstruction: true);
        }
        else
        {
            yield return new WaitForSeconds(freeSpinTriggerPopupDuration);
        }

        freeSpinTriggerPopup.SetActive(false);
        if (panel != null)
            panel.localScale = restScale;
    }

    private void OnSharedStartButtonClicked()
    {
        if (doubleDiamondFreeSpinStartVisible)
        {
            if (!gameManager.waitingForFreeSpinStart)
                return;

            // Keep Start visible until the first free spin replaces it with Stop.
            SetButtonInteractable(sharedStartButton, sharedStartButtonPortrait, false);
            AudioManager.Instance?.PlaySpinStart();
            gameManager.StartFirstFreeSpin();
            return;
        }

        // Legacy CNY U-Spin wheel Start.
        OnWheelSpinClicked();
    }

    internal void HideDoubleDiamondFreeSpinStart()
    {
        if (!doubleDiamondFreeSpinStartVisible)
            return;

        doubleDiamondFreeSpinStartVisible = false;
        SetButtonActive(sharedStartButton, sharedStartButtonPortrait, false);
    }

    // Legacy CNY starts free spins through the universal popup.
    internal void OnFreeSpinsStarted(int spins)
    {
        OnFreeSpinsTriggered(spins);
    }

    internal void OnFreeSpinsTriggered(int spinsAwarded)
    {
        ShowUniversalWinPopup(WinPopupType.FreeSpinTrigger, 0, spinsAwarded, () =>
        {
            StartFreeSpinsSequence(spinsAwarded);
        });
    }

    private void StartFreeSpinsSequence(int spinsAwarded)
    {
        initialFreeSpins = spinsAwarded;
        totalFreeSpinsAwarded = spinsAwarded;
        
        if (gameLogoObject) gameLogoObject.SetActive(false);

        UpdateFreeSpinCount(0, spinsAwarded);
        UpdateWinDisplay(0);
        gameManager.StartFirstFreeSpin();
    }

    internal void OnDoubleDiamondFreeSpinsEnded(double serverTotalRoundWin, System.Action onTransitionComplete)
    {
        initialFreeSpins = 0;
        totalFreeSpinsAwarded = 0;

        // DD free spins include the paid spin's line wins in their displayed total.
        double displayTotalWin = serverTotalRoundWin + ddFreeSpinTriggerLineWin;
        ShowDDTotalWin(TotalWinTitle.FreeSpins, displayTotalWin, () =>
        {
            ddFreeSpinTriggerLineWin = 0d;
            holdDoubleDiamondFreeSpinBalance = false;
            UpdateBalanceDisplay();
            lineWinOrientationController?.SetDoubleDiamondFreeSpinPresentation(false);
            RestoreBaseGameHudAfterFreeSpins();
            onTransitionComplete?.Invoke();
            gameManager?.ResumeAutoPlay();
        });
    }

    internal void CancelDoubleDiamondFreeSpinCompletion()
    {
        if (openDDTotalWinTitle != TotalWinTitle.FreeSpins)
            return;
#if UNITY_EDITOR
        if (isDDTotalWinPreview)
            return;
#endif

        // Disconnect closes the popup directly. Its Take callback would resume normal play.
        CloseDDTotalWinPopup();
        AudioManager.Instance?.StopDDWinAudio();
        ddFreeSpinTriggerLineWin = 0d;
        holdDoubleDiamondFreeSpinBalance = false;
    }

    // Legacy CNY uses the universal completion popup and its Take button.
    internal void OnFreeSpinsEnded(double serverTotalRoundWin, int serverTotalSpinsUsed)
    {
        initialFreeSpins = 0;
        totalFreeSpinsAwarded = 0;

        ShowUniversalWinPopup(WinPopupType.FreeSpinComplete, serverTotalRoundWin, 0, () =>
        {
            StartCoroutine(EndFreeSpinsTransitionSequence());
        });
    }

    private IEnumerator EndFreeSpinsTransitionSequence()
    {
        // 1. Fade in back film
        if (transitionBackFilm != null)
        {
            transitionBackFilm.gameObject.SetActive(true);
            transitionBackFilm.alpha = 0f;
            yield return transitionBackFilm.DOFade(1f, 0.5f).WaitForCompletion();
            yield return new WaitForSeconds(0.2f);
        }

        // 2. Setup main slot UI state behind back film
        if (gameLogoObject) gameLogoObject.SetActive(true);
        RestoreBaseGameHudAfterFreeSpins();

        // 3. Fade out back film
        if (transitionBackFilm != null)
        {
            yield return transitionBackFilm.DOFade(0f, 0.5f).WaitForCompletion();
            transitionBackFilm.gameObject.SetActive(false);
        }

        gameManager?.ResumeAutoPlay();
    }

    private void RestoreBaseGameHudAfterFreeSpins()
    {
        if (freeSpinCountContainer) freeSpinCountContainer.SetActive(false);
        UpdateWinDisplay(0);
        SetSpinStopButtonStates(isSpinningState: false, isInteractable: true);
        SetButtonInteractable(settingsOpenButton, settingsOpenButtonPortrait, true);
        SetBetControlsEnabled(true);
    }

    internal void UpdateDoubleDiamondFreeSpinCount(int spinsRemaining)
    {
        if (freeSpinCountContainer) freeSpinCountContainer.SetActive(true);
        if (remainingFreeSpinsText) remainingFreeSpinsText.text = FormatWinDigits(spinsRemaining);
    }

    // Legacy CNY displays played and awarded spins.
    internal void UpdateFreeSpinCount(int playedSpins, int totalSpins = -1)
    {
        if (totalSpins > 0)
        {
            totalFreeSpinsAwarded = totalSpins;
        }

        if (freeSpinCountContainer) freeSpinCountContainer.SetActive(true);
        if (remainingFreeSpinsText) remainingFreeSpinsText.text = $"FREE GAME  {playedSpins}  OF  {totalFreeSpinsAwarded}";
    }

    #endregion

    #region DD Total Win Popup

    internal enum TotalWinTitle
    {
        FreeSpins,
        Jackpot
    }

    internal bool IsDDTotalWinPopupOpen => totalWinPopup != null && totalWinPopup.activeInHierarchy;

    private GameObject ActiveDDTotalWinTitle => openDDTotalWinTitle switch
    {
        TotalWinTitle.Jackpot => jpotTotalWinTitle,
        TotalWinTitle.FreeSpins => freeSpinTotalWinTitle,
        _ => null
    };

    internal void ShowDDTotalWin(TotalWinTitle title, double amount, System.Action onTake)
    {
        if (!TryOpenDDTotalWinPopup(title))
        {
            if (totalWinPopup == null || !totalWinPopup.activeSelf)
                onTake?.Invoke();
            return;
        }

        DisableControlsDuringWinAnimation();
        SetButtonActive(spinButton, spinButtonPortrait, false);
        ddTotalWinOnTake = onTake;
        AudioManager.Instance?.PlayDDTotalWin();
        AnimateDDTotalWinAmount(amount, ShowDDTotalWinTake);
    }

    private bool TryOpenDDTotalWinPopup(TotalWinTitle title)
    {
        if (totalWinPopup == null || totalWinPopup.activeSelf)
            return false;

        openDDTotalWinTitle = title;
        if (freeSpinTotalWinTitle != null)
            freeSpinTotalWinTitle.SetActive(title == TotalWinTitle.FreeSpins);
        if (jpotTotalWinTitle != null)
            jpotTotalWinTitle.SetActive(title == TotalWinTitle.Jackpot);
        if (totalWinAmountText != null)
            totalWinAmountText.gameObject.SetActive(false);

        totalWinPopup.SetActive(true);
        if (!totalWinPopup.activeInHierarchy)
        {
            totalWinPopup.SetActive(false);
            openDDTotalWinTitle = null;
            return false;
        }

        var orientation = Object.FindFirstObjectByType<OrientationChange>();
        StartDDTotalWinPulse(orientation?.CurrentMode ?? OrientationChange.OrientationMode.Landscape);
        totalWinFountain?.StartFountain();
        totalWinRipple?.StartRipple();
        return true;
    }

    private void AnimateDDTotalWinAmount(double amount, System.Action onPresented)
    {
        float introDuration = Mathf.Max(0f, totalWinIntroDuration);
        float countUpDuration = totalWinAmountText != null && amount > 0d
            ? Mathf.Max(0f, totalWinCountUpDuration)
            : 0f;

        if (introDuration == 0f && countUpDuration == 0f)
        {
            SetDDTotalWinAmount(amount);
            onPresented?.Invoke();
            return;
        }

        var presentation = DOTween.Sequence().SetLink(totalWinPopup, LinkBehaviour.KillOnDisable).Pause();
        var titleObject = ActiveDDTotalWinTitle;
        Image titleImage = titleObject != null ? titleObject.GetComponent<Image>() : null;
        Image frameImage = totalWinFrame != null ? totalWinFrame.GetComponent<Image>() : null;
        float titleAlpha = titleImage != null ? titleImage.color.a : 0f;
        float frameAlpha = frameImage != null ? frameImage.color.a : 0f;

        if (introDuration > 0f)
        {
            presentation.AppendInterval(introDuration);
            if (titleImage != null)
                presentation.Insert(0f, titleImage.DOFade(titleAlpha, introDuration).From(0f));
            if (frameImage != null)
                presentation.Insert(0f, frameImage.DOFade(frameAlpha, introDuration).From(0f));
        }

        if (countUpDuration > 0f)
        {
            presentation.AppendCallback(() => SetDDTotalWinAmount(0d));
            presentation.Append(DOVirtual.Float(0f, 1f, countUpDuration, progress =>
            {
                totalWinAmountText.text = FormatWinDigits(amount * progress);
            }).SetEase(Ease.Linear));
        }

        presentation.OnComplete(() =>
        {
            SetDDTotalWinAmount(amount);
            onPresented?.Invoke();
        });
        presentation.OnKill(() =>
        {
            // Closing the preview during its intro must preserve the authored opacity for next time.
            if (introDuration > 0f)
            {
                if (titleImage != null)
                    titleImage.color = new Color(titleImage.color.r, titleImage.color.g, titleImage.color.b, titleAlpha);
                if (frameImage != null)
                    frameImage.color = new Color(frameImage.color.r, frameImage.color.g, frameImage.color.b, frameAlpha);
            }
            ddTotalWinPresentation = null;
        });
        ddTotalWinPresentation = presentation;
        presentation.Play();
    }

    private void SetDDTotalWinAmount(double amount)
    {
        if (totalWinAmountText == null)
            return;

        totalWinAmountText.text = FormatWinDigits(amount);
        totalWinAmountText.gameObject.SetActive(true);
    }

    private void ShowDDTotalWinTake()
    {
        ddTotalWinTakeReady = true;
        SetButtonActive(sharedWinTakeButton, sharedWinTakeButtonPortrait, true);
        SetDDTotalWinTakeAboveModal(sharedWinTakeButton, true);
        SetDDTotalWinTakeAboveModal(sharedWinTakeButtonPortrait, true);
        SetButtonInteractable(sharedWinTakeButton, sharedWinTakeButtonPortrait, true);
    }

    private void TakeDDTotalWin()
    {
        var onTake = ddTotalWinOnTake;
        CloseDDTotalWinPopup();
        AudioManager.Instance?.StopDDWinAudio();
        if (onTake != null)
            onTake();
        else
            EnableControlsAfterWinAnimation();
    }

    private void CloseDDTotalWinPopup()
    {
        ddTotalWinPresentation?.Kill();
        StopDDTotalWinPulse();
        totalWinFountain?.StopFountain();
        totalWinRipple?.StopRipple();

        if (ddTotalWinTakeReady)
        {
            SetDDTotalWinTakeAboveModal(sharedWinTakeButton, false);
            SetDDTotalWinTakeAboveModal(sharedWinTakeButtonPortrait, false);
            SetButtonActive(sharedWinTakeButton, sharedWinTakeButtonPortrait, false);
        }

        ddTotalWinTakeReady = false;
        ddTotalWinOnTake = null;
        openDDTotalWinTitle = null;
#if UNITY_EDITOR
        isDDTotalWinPreview = false;
#endif
        if (totalWinPopup != null)
            totalWinPopup.SetActive(false);
    }

#if UNITY_EDITOR
    private bool isDDTotalWinPreview;

    internal bool CanPreviewDDPresentation => Application.isPlaying && isActiveAndEnabled &&
        (gameManager == null || (!gameManager.isInFreeSpins && !gameManager.isAutoPlaying &&
            (gameManager.currentState == GameState.Idle || gameManager.currentState == GameState.Initializing)));

    internal bool IsDDTotalWinPreviewOpen => isDDTotalWinPreview && IsDDTotalWinPopupOpen;

    // DD preview uses the shared artwork and count-up without taking a payout.
    internal bool TryOpenDDTotalWinPreview(TotalWinTitle title, double amount)
    {
        if (!CanPreviewDDPresentation || !TryOpenDDTotalWinPopup(title))
            return false;

        isDDTotalWinPreview = true;
        AnimateDDTotalWinAmount(amount, null);
        return true;
    }

    internal void CloseDDTotalWinPreview()
    {
        if (isDDTotalWinPreview)
            CloseDDTotalWinPopup();
    }
#endif

    private void StartDDTotalWinPulse(OrientationChange.OrientationMode mode)
    {
        StopDDTotalWinPulse();
        var titleObject = ActiveDDTotalWinTitle;
        if (titleObject == null || totalWinFrame == null)
            return;

        Transform image = titleObject.transform;
        Transform frame = totalWinFrame.transform;

        // DD uses each orientation's configured scales as its resting size.
        ddTotalWinImageBaseScale = image.localScale;
        ddTotalWinFrameBaseScale = frame.localScale;

        RectTransform rays = totalWinPulseRays;
        if (rays != null)
            ddTotalWinRaysBaseScale = rays.localScale;

        bool isMobilePortrait = mode == OrientationChange.OrientationMode.MobilePortrait;
        ddTotalWinPulse = DOVirtual.Float(-1f, 1f, Mathf.Max(0.1f, totalWinPulseDuration), progress =>
        {
            float change = totalWinPulseAmount * progress;
            image.localScale = ddTotalWinImageBaseScale * (1f + change);
            frame.localScale = ddTotalWinFrameBaseScale * (1f - change);

            if (rays != null && Mathf.Abs(ddTotalWinRaysBaseScale.x) > Mathf.Epsilon)
            {
                // DD Rays use an absolute X range while preserving the artwork's proportions.
                float minScaleX = Mathf.Max(0.01f, isMobilePortrait ? totalWinPortraitRaysMinScaleX : totalWinRaysMinScaleX);
                float maxScaleX = Mathf.Max(minScaleX, isMobilePortrait ? totalWinPortraitRaysMaxScaleX : totalWinRaysMaxScaleX);
                float scaleX = Mathf.Lerp(minScaleX, maxScaleX, (progress + 1f) * 0.5f);
                rays.localScale = ddTotalWinRaysBaseScale * (scaleX / ddTotalWinRaysBaseScale.x);
            }
        })
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(totalWinPopup, LinkBehaviour.KillOnDisable);
    }

    private void StopDDTotalWinPulse(bool restoreScale = true)
    {
        if (ddTotalWinPulseRestart != null)
        {
            StopCoroutine(ddTotalWinPulseRestart);
            ddTotalWinPulseRestart = null;
        }

        if (ddTotalWinPulse == null)
            return;

        ddTotalWinPulse.Kill();
        ddTotalWinPulse = null;

        // Orientation changes supply the new resting scales through OCController.
        if (!restoreScale)
            return;

        if (totalWinPulseRays != null)
            totalWinPulseRays.localScale = ddTotalWinRaysBaseScale;

        var titleObject = ActiveDDTotalWinTitle;
        if (titleObject != null)
            titleObject.transform.localScale = ddTotalWinImageBaseScale;
        if (totalWinFrame != null)
            totalWinFrame.transform.localScale = ddTotalWinFrameBaseScale;
    }

    private IEnumerator RestartDDTotalWinPulseAfterOrientationChange(OrientationChange.OrientationMode mode)
    {
        // DD waits for all orientation handlers before capturing the new resting scales.
        yield return null;
        ddTotalWinPulseRestart = null;

        if (totalWinPopup != null && totalWinPopup.activeInHierarchy)
            StartDDTotalWinPulse(mode);
    }

    private IEnumerator RefreshDDTotalWinTakeAfterOrientationChange()
    {
        // Wait for the orientation controller to activate the new bottom bar.
        yield return null;
        if (!ddTotalWinTakeReady) yield break;

        SetDDTotalWinTakeAboveModal(sharedWinTakeButton, true);
        SetDDTotalWinTakeAboveModal(sharedWinTakeButtonPortrait, true);
    }

    private static void SetDDTotalWinTakeAboveModal(Button button, bool aboveModal)
    {
        if (button == null) return;
        button.TryGetComponent(out Canvas buttonCanvas);
        if (!aboveModal)
        {
            if (buttonCanvas != null)
            {
                buttonCanvas.overrideSorting = false;
                buttonCanvas.enabled = false;
                button.GetComponent<GraphicRaycaster>().enabled = false;
            }
            return;
        }

        if (!button.gameObject.activeInHierarchy) return;
        var parentCanvas = button.transform.parent.GetComponentInParent<Canvas>();
        if (parentCanvas == null) return;

        // The existing Take buttons are under the bottom bars. Give them priority over the modal backdrop.
        if (buttonCanvas == null)
        {
            buttonCanvas = button.gameObject.AddComponent<Canvas>();
            button.gameObject.AddComponent<GraphicRaycaster>();
        }

        buttonCanvas.enabled = true;
        button.GetComponent<GraphicRaycaster>().enabled = true;
        buttonCanvas.overrideSorting = true;
        buttonCanvas.worldCamera = parentCanvas.worldCamera;
        buttonCanvas.sortingLayerID = parentCanvas.sortingLayerID;
        buttonCanvas.sortingOrder = parentCanvas.sortingOrder + 1;
    }

    #endregion

    #region Expand / Shrink

    private void InitializeExpandShrink()
    {
        SetExpandShrinkButtons(isExpanded: false);
    }

    private void OnExpand()
    {
        isExpanded = true;
        jsFunctCalls?.RequestExpandGame();
        SetExpandShrinkButtons(isExpanded: true);
    }

    private void OnShrink()
    {
        isExpanded = false;
        jsFunctCalls?.RequestShrinkGame();
        SetExpandShrinkButtons(isExpanded: false);
    }

    private void SetExpandShrinkButtons(bool isExpanded)
    {
        SetButtonActive(expandButton, expandButtonPortrait, !isExpanded);
        SetButtonActive(shrinkButton, shrinkButtonPortrait, isExpanded);
    }

    private void RegisterFullscreenListener()
    {
        jsFunctCalls?.RegisterFullscreenListener(gameObject.name);
    }

    internal void OnFullscreenChanged(string isFullscreen)
    {
        bool newExpandedState = isFullscreen == "1";
        Debug.Log($"[UI] OnFullscreenChanged callback: isFullscreen={isFullscreen}, newState={newExpandedState}");

        if (isExpanded != newExpandedState)
        {
            isExpanded = newExpandedState;
            SetExpandShrinkButtons(isExpanded);
            Debug.Log($"[UI] Button states synced to fullscreen: {(isExpanded ? "EXPANDED" : "SHRINK")}");
        }
    }
    
    #endregion

    #region Popup Animations (Generic)

    private void AnimatePopupOpen(RectTransform popupRect)
    {
        if (!popupRect) return;
        popupRect.localScale = Vector3.zero;
        popupRect.DOScale(1.4f, 0.3f).SetEase(Ease.OutBack);
    }

    private void AnimatePopupClose(RectTransform popupRect, System.Action onComplete)
    {
        if (!popupRect) return;

        AudioManager.Instance?.PlayPopupClose();

        Sequence closeSeq = DOTween.Sequence();
        closeSeq.Append(popupRect.DOScale(1.1f, 0.1f));
        closeSeq.Append(popupRect.DOScale(0f, 0.2f).SetEase(Ease.InBack));
        closeSeq.OnComplete(() =>
        {
            popupRect.localScale = new Vector3(1.4f, 1.4f, 1.4f);
            onComplete?.Invoke();
        });
    }

    #endregion

    #region Display Updates

    internal void UpdatePingDisplay(int pingMs)
    {
        SetTMPText(pingText, pingTextPortrait, $"{pingMs} ms");
    }

    internal void UpdatePingDisplay(string content)
    {
        SetTMPText(pingText, pingTextPortrait, content);
    }

    internal void UpdateJackpotDisplay(JackpotValues values)
    {
        if (values == null) return;

        SetTMPText(grandJackpotText, grandJackpotTextPortrait, FormatJackpotValue(values.grandJackpot));
        SetTMPText(majorJackpotText, majorJackpotTextPortrait, FormatJackpotValue(values.majorJackpot));
        SetTMPText(minorJackpotText, minorJackpotTextPortrait, FormatJackpotValue(values.minorJackpot));
        SetTMPText(miniJackpotText, miniJackpotTextPortrait, FormatJackpotValue(values.miniJackpot));
    }

    private string FormatJackpotValue(string val)
    {
        if (string.IsNullOrEmpty(val)) return "$0.00";
        return val.StartsWith("$") ? val : "$" + val;
    }

    internal void UpdateBalanceDisplay()
    {
        if (holdDoubleDiamondFreeSpinBalance || holdDoubleDiamondJackpotBalance) return;
        SetBalanceDisplay(gameManager.playerData.balance);
    }

    private void SetBalanceDisplay(double balance)
    {
        SetTMPText(balanceText, balanceTextPortrait, "BALANCE : " + FormatAmount(balance));
    }

    private void UpdateWinDisplay(double amount, bool forceWinVisible = false)
    {
        currentWinDisplayValue = amount;
        if (winAmountText) winAmountText.text = FormatAmount(amount);
        if (winAmountTextPortrait) winAmountTextPortrait.text = "WIN " + FormatAmount(amount);

        bool showWinText = forceWinVisible || amount > 0 || (gameManager != null && gameManager.isInFreeSpins);

        if (showWinText)
        {
            SetGameObjectActive(goodLuckObject, goodLuckObjectPortrait, false);
            SetGameObjectActive(winTextObject, winTextObjectPortrait, true);
        }
        else
        {
            SetGameObjectActive(goodLuckObject, goodLuckObjectPortrait, true);
            SetGameObjectActive(winTextObject, winTextObjectPortrait, false);
        }
    }

    private void AnimateBalanceUpdate(double newBalance, double startBalance = -1f, float durationOverride = -1f)
    {
        if (holdDoubleDiamondFreeSpinBalance || holdDoubleDiamondJackpotBalance) return;
        if (balanceTween != null) balanceTween.Kill();
        hasOptimisticBalance = false;
        SetBalanceDisplay(newBalance);
    }

    private void AnimateWinUpdate(double targetWin, float duration = 0.8f)
    {
        if (winTween != null) winTween.Kill();
        UpdateWinDisplay(targetWin);
    }

    #endregion

    #region Helper Methods

    private string FormatAmount(double amount)
    {
        return amount.ToString("0.###");
    }

    private void SetBetControlsEnabled(bool enabled)
    {
        SetButtonInteractable(betPlusButton, betPlusButtonPortrait, enabled);
        SetButtonInteractable(betMinusButton, betMinusButtonPortrait, enabled);
    }

    #endregion

    #region Dynamic Game Rules Updates

    private void UpdateGameRulesDynamicTexts()
    {
        if (gameManager.gameConfig == null) return;

        if (totalLineCountText != null)
        {
            totalLineCountText.text = gameManager.gameConfig.paylineCount.ToString();
        }

        var symbols = gameManager.gameConfig.symbols;
        if (symbols == null) return;

        TMP_Text[] symbolTexts = {
            ruleSymbol0Text, ruleSymbol1Text, ruleSymbol2Text, ruleSymbol3Text,
            ruleSymbol4Text, ruleSymbol5Text, ruleSymbol6Text, ruleSymbol7Text,
            ruleSymbol8Text, ruleSymbol9Text
        };

        const int doubleDiamondRowCount = 8;
        for (int i = 0; i < symbolTexts.Length; i++)
        {
            var symbolText = symbolTexts[i];
            if (symbolText == null) continue;

            // DD artwork starts with Red Diamond (ID 1). The two legacy template rows keep raw paytable values.
            bool isDoubleDiamondRow = i < doubleDiamondRowCount;
            int symbolId = isDoubleDiamondRow ? i + 1 : i;
            var symbol = symbols.Find(s => s.id == symbolId);
            var lines = new List<string>();
            foreach (var (matchCount, payout) in gameManager.GetSymbolPayoutRows(symbol, applySelectedBet: isDoubleDiamondRow))
                lines.Add(isDoubleDiamondRow ? $"{matchCount}   {payout:0.###}" : $"{matchCount}     {payout:0.###}");

            if (isDoubleDiamondRow || lines.Count > 0)
                symbolText.text = string.Join("\n", lines);
        }
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        if (balanceTween != null) balanceTween.Kill();
        if (winTween != null) winTween.Kill();
        DOTween.KillAll();
    }

    #endregion

    #region Connection Popup Management

    private void OnExitButtonPressed()
    {
        if (popupManager != null)
        {
            popupManager.ShowExitGamePopup();
        }
        else if (gameManager != null)
        {
            AudioManager.Instance?.PlayButton();
            gameManager.ExitGame();
        }
    }

    #endregion

    #region Bonus Game

    internal void TriggerUSpinBonus(USpinResultData resultData, System.Action onComplete)
    {
        StartCoroutine(USpinBonusSequence(resultData, onComplete));
    }

    private bool wheelSpinTriggered = false;

    private void OnWheelSpinClicked()
    {
        if (wheelSpinTriggered) return;
        AudioManager.Instance?.PlayWheelStart();
        wheelSpinTriggered = true;
        SetButtonInteractable(sharedStartButton, sharedStartButtonPortrait, false);
        SetButtonInteractable(wheelSpinButton2, wheelSpinButtonPortrait2, false);
        SetButtonActive(wheelSpinButton2, wheelSpinButtonPortrait2, false);
    }

    private IEnumerator USpinBonusSequence(USpinResultData resultData, System.Action onComplete)
    {
        // 1. Fade in back film
        if (transitionBackFilm != null)
        {
            transitionBackFilm.gameObject.SetActive(true);
            transitionBackFilm.alpha = 0f;
            yield return transitionBackFilm.DOFade(1f, 0.5f).WaitForCompletion();
            yield return new WaitForSeconds(0.5f);
        }
        
        // 2. Open spin wheel screen
        wheelSpinTriggered = false;
        if (wheelScreen) wheelScreen.SetActive(true);
        StartWheelBonusEffects();
        
        // Hide normal spin/stop buttons, show wheel spin button
        SetSpinStopButtonStates(isSpinningState: false, isInteractable: false);
        SetButtonActive(spinButton, spinButtonPortrait, false);
        SetButtonActive(autoSpinStopButton, autoSpinStopButtonPortrait, false);
        SetButtonActive(sharedStartButton, sharedStartButtonPortrait, true);
        SetButtonInteractable(sharedStartButton, sharedStartButtonPortrait, true);
        UpdateNewWheelSpinButtonsVisibility();
        SetButtonInteractable(wheelSpinButton2, wheelSpinButtonPortrait2, true);

        // 3. Fade out back film
        if (transitionBackFilm != null)
        {
            yield return transitionBackFilm.DOFade(0f, 0.5f).WaitForCompletion();
            transitionBackFilm.gameObject.SetActive(false);
        }

        // 4. Wait for user to click wheel spin
        yield return new WaitUntil(() => wheelSpinTriggered);

        // 5. Spin Wheel
        int mainTargetIndex = resultData.sliceIndex;
        bool mainSpinDone = false;
        if (mainWheel != null)
        {
            mainWheel.SpinToIndex(mainTargetIndex, () => mainSpinDone = true);
        }
        else
        {
            mainSpinDone = true;
        }
        yield return new WaitUntil(() => mainSpinDone);
        yield return new WaitForSeconds(0.5f);

        // 6. Handle the two possibilities
        if (resultData.type == "FREE_GAMES")
        {
            // Show freespin trigger popup BEFORE backfilm transition
            bool takePressed = false;
            ShowUniversalWinPopup(WinPopupType.FreeSpinTrigger, 0, resultData.freeGamesAwarded, () =>
            {
                takePressed = true;
            });
            yield return new WaitUntil(() => takePressed);

            // Blackfilm transition fade in to cover the screen
            if (transitionBackFilm != null)
            {
                transitionBackFilm.gameObject.SetActive(true);
                transitionBackFilm.alpha = 0f;
                yield return transitionBackFilm.DOFade(1f, 0.5f).WaitForCompletion();
                yield return new WaitForSeconds(0.5f);
            }
            
            // Turn off wheel screen behind blackfilm
            StopWheelBonusEffects();
            if (wheelScreen) wheelScreen.SetActive(false);
            
            // Setup free spin mode/UI behind blackfilm
            if (gameManager != null)
            {
                if (gameManager.isInFreeSpins)
                {
                    gameManager.freeSpinsRemaining += resultData.freeGamesAwarded;
                    int updatedTotalSpins = totalFreeSpinsAwarded + resultData.freeGamesAwarded;
                    UpdateFreeSpinCount(gameManager.freeSpinsUsed, updatedTotalSpins);
                }
                else
                {
                    if (gameManager.isAutoPlaying)
                    {
                        int prevTotal = gameManager.autoPlayTotalRounds;
                        int prevRemaining = gameManager.autoPlayRemainingRounds;
                        gameManager.StopAutoPlay();
                        gameManager.wasAutoPlayingBeforeFreeSpins = true;
                        gameManager.savedAutoPlayTotalRounds = prevTotal;
                        gameManager.savedAutoPlayRemainingRounds = (prevTotal != -1) ? (prevRemaining - 1) : -1;
                    }

                    gameManager.isInFreeSpins = true;
                    gameManager.freeSpinsRemaining = resultData.freeGamesAwarded;
                    gameManager.freeSpinsUsed = 0;
                    initialFreeSpins = resultData.freeGamesAwarded;
                    totalFreeSpinsAwarded = resultData.freeGamesAwarded;
                    
                    if (gameLogoObject) gameLogoObject.SetActive(false);
                    UpdateFreeSpinCount(0, resultData.freeGamesAwarded);
                    UpdateWinDisplay(0);
                    
                    SetSpinStopButtonStates(isSpinningState: true, isInteractable: false);
                    SetBetControlsEnabled(false);
                    SetButtonInteractable(settingsOpenButton, settingsOpenButtonPortrait, true);

                    if (gameManager.lastResult != null && gameManager.lastResult.freeSpinData != null)
                    {
                        gameManager.lastResult.freeSpinData.isTriggered = false;
                    }
                }
            }

            // Blackfilm transition fade out revealing free spin setup
            if (transitionBackFilm != null)
            {
                yield return transitionBackFilm.DOFade(0f, 0.5f).WaitForCompletion();
                transitionBackFilm.gameObject.SetActive(false);
            }
        }
        else // MULTIPLIER / CREDITS
        {
            bool takePressed = false;
            ShowUniversalWinPopup(WinPopupType.RegularWin, resultData.winInCash, 0, () =>
            {
                takePressed = true;
            });
            yield return new WaitUntil(() => takePressed);

            if (gameManager != null && gameManager.lastResult != null)
            {
                double prevBalance = gameManager.playerData.balance;
                gameManager.playerData.balance += resultData.winInCash;

                double targetWin = gameManager.isInFreeSpins
                    ? gameManager.lastResult.serverTotalRoundWin
                    : (gameManager.lastResult.grandTotalWin > 0 ? gameManager.lastResult.grandTotalWin : (gameManager.lastResult.winAmount + resultData.winInCash));
                AnimateWinUpdate(targetWin);
                AnimateBalanceUpdate(gameManager.playerData.balance, prevBalance);
            }
            
            if (transitionBackFilm != null)
            {
                transitionBackFilm.gameObject.SetActive(true);
                transitionBackFilm.alpha = 0f;
                yield return transitionBackFilm.DOFade(1f, 0.5f).WaitForCompletion();
                yield return new WaitForSeconds(0.5f);
            }
            
            StopWheelBonusEffects();
            if (wheelScreen) wheelScreen.SetActive(false);
            
            if (transitionBackFilm != null)
            {
                yield return transitionBackFilm.DOFade(0f, 0.5f).WaitForCompletion();
                transitionBackFilm.gameObject.SetActive(false);
            }
        }

        SetButtonActive(sharedStartButton, sharedStartButtonPortrait, false);
        SetButtonActive(wheelSpinButton2, wheelSpinButtonPortrait2, false);
        if (gameManager == null || !gameManager.isInFreeSpins)
        {
            if (gameManager != null && gameManager.isAutoPlaying)
            {
                OnAutoPlayStarted();
            }
            else
            {
                SetSpinStopButtonStates(isSpinningState: false, isInteractable: true);
            }
        }
        else
        {
            SetSpinStopButtonStates(isSpinningState: true, isInteractable: false);
        }

        onComplete?.Invoke();
    }

    internal void TriggerMoneyBagBonus(MoneyBagResultData resultData, System.Action onComplete)
    {
        StartCoroutine(MoneyBagBonusSequence(resultData, onComplete));
    }

    private IEnumerator MoneyBagBonusSequence(MoneyBagResultData resultData, System.Action onComplete)
    {
        if (transitionBackFilm != null)
        {
            transitionBackFilm.gameObject.SetActive(true);
            transitionBackFilm.alpha = 0f;
            yield return transitionBackFilm.DOFade(1f, 0.5f).WaitForCompletion();
            yield return new WaitForSeconds(0.5f);
        }
        
        SetButtonActive(autoSpinStopButton, autoSpinStopButtonPortrait, false);
        SetButtonActive(stopButton, stopButtonPortrait, false);
        SetButtonActive(spinButton, spinButtonPortrait, true);
        SetButtonInteractable(spinButton, spinButtonPortrait, false);

        bool moneyBagDone = false;
        
        if (moneyBagController != null)
        {
            moneyBagController.gameObject.SetActive(true);
            moneyBagController.StartMoneyBagBonus(resultData, () => moneyBagDone = true);

            if (transitionBackFilm != null)
            {
                yield return transitionBackFilm.DOFade(0f, 0.5f).WaitForCompletion();
                transitionBackFilm.gameObject.SetActive(false);
            }

            yield return new WaitUntil(() => moneyBagDone);

            bool takePressed = false;
            ShowUniversalWinPopup(WinPopupType.MoneyBagCollect, resultData.winInCash, 0, () =>
            {
                takePressed = true;
            });
            yield return new WaitUntil(() => takePressed);

            if (gameManager != null && gameManager.lastResult != null)
            {
                double prevBalance = gameManager.playerData.balance;
                gameManager.playerData.balance += resultData.winInCash;

                double targetWin = gameManager.isInFreeSpins
                    ? gameManager.lastResult.serverTotalRoundWin
                    : (gameManager.lastResult.grandTotalWin > 0 ? gameManager.lastResult.grandTotalWin : (gameManager.lastResult.winAmount + resultData.winInCash));
                AnimateWinUpdate(targetWin);
                AnimateBalanceUpdate(gameManager.playerData.balance, prevBalance);
            }
            
            if (transitionBackFilm != null)
            {
                transitionBackFilm.gameObject.SetActive(true);
                transitionBackFilm.alpha = 0f;
                yield return transitionBackFilm.DOFade(1f, 0.5f).WaitForCompletion();
                yield return new WaitForSeconds(0.5f);
            }

            if (moneyBagController != null)
            {
                moneyBagController.gameObject.SetActive(false);
            }
        }
        else
        {
            moneyBagDone = true;
            Debug.LogError("MoneyBagController is not assigned in UIManager!");
        }

        if (transitionBackFilm != null)
        {
            yield return transitionBackFilm.DOFade(0f, 0.5f).WaitForCompletion();
            transitionBackFilm.gameObject.SetActive(false);
        }

        if (gameManager != null && gameManager.isInFreeSpins)
        {
            SetSpinStopButtonStates(isSpinningState: true, isInteractable: false);
        }
        else if (gameManager != null && gameManager.isAutoPlaying)
        {
            OnAutoPlayStarted();
        }
        else
        {
            SetSpinStopButtonStates(isSpinningState: false, isInteractable: true);
        }

        onComplete?.Invoke();
    }

    #endregion

    #region Universal Win Popup

    internal void ShowUniversalWinPopup(WinPopupType type, double winAmount, int freeSpinCount = 0, System.Action onTakePressed = null)
    {
        if (universalWinPopup == null) return;

        AudioManager.Instance?.PlayWinObjectBg();
        isSpecialWinActive = true;
        universalWinPopupCallback = onTakePressed;

        if (uwpWinTween != null)
        {
            uwpWinTween.Kill();
            uwpWinTween = null;
        }

        if (uwpCongratulationsTitle) uwpCongratulationsTitle.SetActive(false);
        if (uwpYouWonSubtitle) uwpYouWonSubtitle.SetActive(false);
        if (uwpBigWinTitle) uwpBigWinTitle.SetActive(false);
        if (uwpWinAmountText) uwpWinAmountText.gameObject.SetActive(false);
        if (uwpFreeSpinCountText) uwpFreeSpinCountText.gameObject.SetActive(false);
        if (uwpFreeSpinObject) uwpFreeSpinObject.SetActive(false);

        switch (type)
        {
            case WinPopupType.FreeSpinTrigger:
                if (uwpCongratulationsTitle) uwpCongratulationsTitle.SetActive(true);
                if (uwpYouWonSubtitle) uwpYouWonSubtitle.SetActive(true);
                if (uwpFreeSpinCountText)
                {
                    uwpFreeSpinCountText.gameObject.SetActive(true);
                    uwpFreeSpinCountText.text = freeSpinCount.ToString();
                }
                if (uwpFreeSpinObject) uwpFreeSpinObject.SetActive(true);
                break;

            case WinPopupType.RegularWin:
                if (uwpCongratulationsTitle) uwpCongratulationsTitle.SetActive(true);
                if (uwpYouWonSubtitle) uwpYouWonSubtitle.SetActive(true);
                if (uwpWinAmountText)
                {
                    uwpWinAmountText.gameObject.SetActive(true);
                    uwpWinAmountText.text = FormatAmount(winAmount);
                }
                break;

            case WinPopupType.BigWin:
                if (uwpBigWinTitle) uwpBigWinTitle.SetActive(true);
                if (uwpWinAmountText)
                {
                    uwpWinAmountText.gameObject.SetActive(true);
                    uwpWinAmountText.text = FormatAmount(winAmount);
                    RectTransform bigWinAmountRect = uwpWinAmountText.GetComponent<RectTransform>();
                    if (bigWinAmountRect != null)
                    {
                        Vector2 pos = bigWinAmountRect.anchoredPosition;
                        pos.y = 0f;
                        bigWinAmountRect.anchoredPosition = pos;
                    }
                }
                break;

            case WinPopupType.MoneyBagCollect:
                if (uwpCongratulationsTitle) uwpCongratulationsTitle.SetActive(true);
                if (uwpYouWonSubtitle) uwpYouWonSubtitle.SetActive(true);
                if (uwpWinAmountText)
                {
                    uwpWinAmountText.gameObject.SetActive(true);
                    uwpWinAmountText.text = FormatAmount(winAmount);
                }
                break;

            case WinPopupType.FreeSpinComplete:
                if (uwpCongratulationsTitle) uwpCongratulationsTitle.SetActive(true);
                if (uwpYouWonSubtitle) uwpYouWonSubtitle.SetActive(true);
                if (uwpWinAmountText)
                {
                    uwpWinAmountText.gameObject.SetActive(true);
                    uwpWinAmountText.text = FormatAmount(winAmount);
                }
                break;
        }

        if (type != WinPopupType.BigWin && uwpWinAmountText)
        {
            RectTransform winAmountRect = uwpWinAmountText.GetComponent<RectTransform>();
            if (winAmountRect != null)
            {
                Vector2 pos = winAmountRect.anchoredPosition;
                pos.y = -90f;
                winAmountRect.anchoredPosition = pos;
            }
        }

        SetSpinStopButtonStates(isSpinningState: false, isInteractable: false);

        bool showTakeButton = (type != WinPopupType.BigWin);
        SetButtonActive(sharedWinTakeButton, sharedWinTakeButtonPortrait, showTakeButton);
        SetButtonInteractable(sharedWinTakeButton, sharedWinTakeButtonPortrait, showTakeButton);

        universalWinPopup.SetActive(true);
        if (universalWinPopupRect)
        {
            universalWinPopupRect.localScale = Vector3.zero;
            Sequence openSeq = DOTween.Sequence();
            openSeq.Append(universalWinPopupRect.DOScale(1.2f, 0.5f).SetEase(Ease.OutCubic));
            openSeq.Append(universalWinPopupRect.DOScale(1f, 0.3f).SetEase(Ease.InOutSine));
        }

        if (starFountain != null) starFountain.PlayStarBurst();

        if (uwpWinAmountText != null && uwpWinAmountText.gameObject.activeSelf && winAmount > 0)
        {
            int decimals = GetDecimalPlaces(winAmount);
            string formatStr = decimals > 0 ? "0." + new string('0', decimals) : "0";

            uwpWinAmountText.text = (0.0).ToString(formatStr);

            float countUpDuration = (type == WinPopupType.BigWin) ? 1.5f : 1.0f;

            uwpWinTween = DOVirtual.Float(0f, (float)winAmount, countUpDuration, (val) =>
            {
                if (uwpWinAmountText != null)
                {
                    uwpWinAmountText.text = val.ToString(formatStr);
                }
            }).OnComplete(() =>
            {
                if (uwpWinAmountText != null)
                {
                    uwpWinAmountText.text = FormatAmount(winAmount);
                }
                uwpWinTween = null;
            });
        }

        if (uwpAutoCloseCoroutine != null) StopCoroutine(uwpAutoCloseCoroutine);
        uwpAutoCloseCoroutine = StartCoroutine(AutoCloseUniversalWinPopup());
    }

    private int GetDecimalPlaces(double amount)
    {
        double rounded = System.Math.Round(amount, 4);
        string str = rounded.ToString(System.Globalization.CultureInfo.InvariantCulture);
        int dotIndex = str.IndexOf('.');
        if (dotIndex < 0) return 0;
        return str.Length - dotIndex - 1;
    }

    private IEnumerator AutoCloseUniversalWinPopup()
    {
        yield return new WaitForSeconds(uwpAutoCloseDelay);
        uwpAutoCloseCoroutine = null;
        CloseUniversalWinPopup();
    }

    private void OnWinTakeButtonClicked()
    {
        if (IsDDTotalWinPopupOpen)
        {
            if (!ddTotalWinTakeReady) return;

            AudioManager.Instance?.PlayTakeButton();
            TakeDDTotalWin();
            return;
        }

        AudioManager.Instance?.StopWinObjectBg();
        AudioManager.Instance?.PlayTakeButton();
        CloseUniversalWinPopup();
    }

    private void CloseUniversalWinPopup()
    {
        if (universalWinPopup == null || !universalWinPopup.activeSelf) return;

        AudioManager.Instance?.StopWinObjectBg();

        if (uwpWinTween != null)
        {
            uwpWinTween.Kill();
            uwpWinTween = null;
        }

        if (starFountain != null) starFountain.StopStarBurst();

        if (uwpAutoCloseCoroutine != null)
        {
            StopCoroutine(uwpAutoCloseCoroutine);
            uwpAutoCloseCoroutine = null;
        }

        System.Action callback = universalWinPopupCallback;
        universalWinPopupCallback = null;

        SetButtonInteractable(sharedWinTakeButton, sharedWinTakeButtonPortrait, false);

        if (universalWinPopupRect)
        {
            Sequence closeSeq = DOTween.Sequence();
            closeSeq.Append(universalWinPopupRect.DOScale(1.1f, 0.1f));
            closeSeq.Append(universalWinPopupRect.DOScale(0f, 0.2f).SetEase(Ease.InBack));
            closeSeq.OnComplete(() =>
            {
                universalWinPopupRect.localScale = Vector3.one;
                universalWinPopup.SetActive(false);

                SetButtonActive(sharedWinTakeButton, sharedWinTakeButtonPortrait, false);
                isSpecialWinActive = false;
                EnableControlsAfterWinAnimation();

                callback?.Invoke();
            });
        }
        else
        {
            universalWinPopup.SetActive(false);
            SetButtonActive(sharedWinTakeButton, sharedWinTakeButtonPortrait, false);
            isSpecialWinActive = false;
            EnableControlsAfterWinAnimation();

            callback?.Invoke();
        }
    }

    #endregion

    #region Bonus Wheel Effects

    internal void StartWheelBonusEffects()
    {
        var oc = Object.FindFirstObjectByType<OrientationChange>();
        
        if (wheelCoinFountain != null)
        {
            wheelCoinFountain.PlayStarBurst();
        }

        if (wheelAnticlockwiseRotatingObject != null)
        {
            if (wheelAnticlockwiseRotationTween != null)
            {
                wheelAnticlockwiseRotationTween.Kill();
                wheelAnticlockwiseRotationTween = null;
            }

            // Continuous anticlockwise rotation (positive Z rotation 0 to 360 degrees)
            wheelAnticlockwiseRotationTween = wheelAnticlockwiseRotatingObject
                .DORotate(new Vector3(0f, 0f, 360f), wheelRotationDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
        }

        // Title object popping loop animation (1 -> 1.2 -> 1 scale in loop)
        List<Transform> titlesToAnimate = new List<Transform>();
        if (wheelTitleTransform != null) titlesToAnimate.Add(wheelTitleTransform);
        if (wheelTitleTransformPortrait != null) titlesToAnimate.Add(wheelTitleTransformPortrait);
        if (wheelTitleObjects != null)
        {
            foreach (var t in wheelTitleObjects)
            {
                if (t != null && !titlesToAnimate.Contains(t)) titlesToAnimate.Add(t);
            }
        }

        if (wheelTitleTweens == null) wheelTitleTweens = new List<Tween>();
        else wheelTitleTweens.Clear();

        if (wheelTitleInitialScales == null) wheelTitleInitialScales = new Dictionary<Transform, Vector3>();

        foreach (var t in titlesToAnimate)
        {
            if (t == null) continue;
            t.DOKill();
            if (!wheelTitleInitialScales.ContainsKey(t))
            {
                wheelTitleInitialScales[t] = t.localScale;
            }
            Vector3 initScale = wheelTitleInitialScales[t];
            t.localScale = initScale;

            Tween titleTween = t.DOScale(initScale * 1.2f, 0.6f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

            wheelTitleTweens.Add(titleTween);
        }
    }

    internal void StopWheelBonusEffects()
    {
        if (wheelCoinFountain != null)
        {
            wheelCoinFountain.StopStarBurst();
        }

        if (wheelAnticlockwiseRotationTween != null)
        {
            wheelAnticlockwiseRotationTween.Kill();
            wheelAnticlockwiseRotationTween = null;
        }

        if (wheelTitleTweens != null)
        {
            foreach (var tween in wheelTitleTweens)
            {
                if (tween != null && tween.IsActive())
                {
                    tween.Kill();
                }
            }
            wheelTitleTweens.Clear();
        }

        if (wheelTitleInitialScales != null)
        {
            foreach (var kvp in wheelTitleInitialScales)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.DOKill();
                    kvp.Key.localScale = kvp.Value;
                }
            }
        }
    }

    #endregion
}
