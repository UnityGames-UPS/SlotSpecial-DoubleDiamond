using UnityEngine;
using UnityEngine.Serialization;

public class AudioManager : MonoBehaviour
{
    internal static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _musicEnabled = PlayerPrefs.GetInt(PrefKeyMusic, 1) == 1;
        _sfxEnabled   = PlayerPrefs.GetInt(PrefKeysfx,   1) == 1;
        _musicVolume  = PlayerPrefs.GetFloat(PrefKeyMusicVol, 0.5f);
        _sfxVolume    = PlayerPrefs.GetFloat(PrefKeySfxVol,   1.0f);

        ApplyMusicVolume();
        ApplySfxVolume();
    }

    private const string PrefKeyMusic    = "audio_music_enabled";
    private const string PrefKeysfx      = "audio_sfx_enabled";
    private const string PrefKeyMusicVol = "audio_music_volume";
    private const string PrefKeySfxVol   = "audio_sfx_volume";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgMusicSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource wheelSegmentSource;
    [SerializeField] private AudioSource reserveSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip clipGameMainBg;
    [SerializeField] private AudioClip clipBetPlusMinus;
    [SerializeField] private AudioClip clipMaxBetReached;
    [SerializeField, Tooltip("Manual Spin button and DD free-spin Start.")] private AudioClip clipPrimaryActionButton;
    [SerializeField] private AudioClip clipGeneralButtonClick;
    [SerializeField] private AudioClip clipPopupOpenClose;
    [SerializeField, Tooltip("Plays once when the autoplay panel opens or closes.")] private AudioClip clipAutoplayPanelOpen;
    [SerializeField] private AudioClip clipFreeSpinBg;
    [SerializeField] private AudioClip clipReelStop;

    [Header("DD Win SFX")]
    [SerializeField] private AudioSource ddWinSource;
    [SerializeField] private AudioClip clipDDWinning;
    [SerializeField] private AudioClip clipDDAfterWin;
    [FormerlySerializedAs("clipDDTotalBonusWon")]
    [SerializeField] private AudioClip clipDDTotalWin;

    [Header("DD Reel and Free Spin Audio")]
    [SerializeField] private AudioSource ddReelSource;
    [SerializeField] private AudioClip clipDDFreeSpinBg;
    [SerializeField] private AudioClip clipDDReelSpinning;
    [SerializeField] private AudioClip clipDDInfernoLanding;
    [SerializeField] private AudioClip clipDDBonusLanding;
    [SerializeField] private AudioClip clipDDWinLineReplay;

    private bool _musicEnabled = true;
    private bool _sfxEnabled   = true;
    private float _musicVolume = 0.5f;
    private float _sfxVolume   = 1.0f;

    internal bool MusicEnabled => _musicEnabled;
    internal bool SfxEnabled   => _sfxEnabled;
    internal float MusicVolume => _musicVolume;
    internal float SfxVolume   => _sfxVolume;

    internal void SetMusicEnabled(bool on)
    {
        _musicEnabled = on;
        PlayerPrefs.SetInt(PrefKeyMusic, on ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMusicVolume();
    }

    internal void SetSfxEnabled(bool on)
    {
        _sfxEnabled = on;
        PlayerPrefs.SetInt(PrefKeysfx, on ? 1 : 0);
        PlayerPrefs.Save();
        ApplySfxVolume();
    }

    internal void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(PrefKeyMusicVol, _musicVolume);
        PlayerPrefs.Save();
        ApplyMusicVolume();
    }

    internal void SetSfxVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(PrefKeySfxVol, _sfxVolume);
        PlayerPrefs.Save();
        ApplySfxVolume();
    }

    private void ApplyMusicVolume()
    {
        if (bgMusicSource == null) return;
        bgMusicSource.volume = _musicEnabled ? _musicVolume : 0f;
    }

    private void ApplySfxVolume()
    {
        float v = _sfxEnabled ? _sfxVolume : 0f;
        if (uiSource           != null) uiSource.volume           = v;
        if (wheelSegmentSource != null) wheelSegmentSource.volume = v;
        if (reserveSource      != null) reserveSource.volume      = v;
        if (ddWinSource        != null) ddWinSource.volume        = v;
        if (ddReelSource       != null) ddReelSource.volume       = v;
    }

    /// <summary>
    /// Uses UI source (AudioSource 2). If busy/playing, falls back to reserve source (AudioSource 4).
    /// </summary>
    private void PlayUISound(AudioClip clip)
    {
        if (!_sfxEnabled || clip == null) return;

        if (uiSource != null && !uiSource.isPlaying)
        {
            uiSource.PlayOneShot(clip);
        }
        else if (reserveSource != null)
        {
            reserveSource.PlayOneShot(clip);
        }
        else if (uiSource != null)
        {
            uiSource.PlayOneShot(clip);
        }
    }

    private void PlayLoop(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null) return;
        source.clip   = clip;
        source.loop   = true;
        source.volume = _musicEnabled ? _musicVolume : 0f;
        source.Play();
    }

    private void StopSource(AudioSource source)
    {
        if (source == null) return;
        source.Stop();
        source.loop = false;
    }

    // 1. Game Main BG
    internal void PlayBgMusic()
    {
        if (bgMusicSource == null || clipGameMainBg == null) return;
        if (bgMusicSource.isPlaying && bgMusicSource.clip == clipGameMainBg) return;

        bgMusicSource.clip   = clipGameMainBg;
        bgMusicSource.loop   = true;
        bgMusicSource.volume = _musicEnabled ? _musicVolume : 0f;
        bgMusicSource.Play();
    }

    internal void PlayMainBg() => PlayBgMusic();

    internal void StopBgMusic()
    {
        StopSource(bgMusicSource);
    }

    // 2. Bet Plus / Bet Minus (one for both)
    internal void PlayBetPlusMinus()
    {
        PlayUISound(clipBetPlusMinus);
    }

    internal void PlayBetPlus()  => PlayBetPlusMinus();
    internal void PlayBetMinus() => PlayBetPlusMinus();

    // 3. Max Bet Reached
    internal void PlayMaxBetReached()
    {
        PlayUISound(clipMaxBetReached);
    }

    // 6. DD manual Spin and free-spin Start share the spin button cue.
    internal void PlaySpinStart()
    {
        PlayUISound(clipPrimaryActionButton != null ? clipPrimaryActionButton : clipGeneralButtonClick);
    }

    internal void PlaySpinStop()     => PlayButton();
    internal void PlayTakeButton()   => PlayButton();
    internal void PlayAutoplayStop() => PlayButton();

    // 7. General Button Click
    internal void PlayButton()
    {
        PlayUISound(clipGeneralButtonClick);
    }

    internal void PlayGeneralButtonClick() => PlayButton();

    // 8. Popup Open Close Sound
    internal void PlayPopupOpenClose()
    {
        PlayUISound(clipPopupOpenClose != null ? clipPopupOpenClose : clipGeneralButtonClick);
    }

    internal void PlayPopupClose() => PlayPopupOpenClose();
    internal void PlayPopupOpen()  => PlayPopupOpenClose();

    // 9. DD uses the same cue when the autoplay panel opens or closes.
    internal void PlayAutoplayPanelToggle()
    {
        PlayUISound(clipAutoplayPanelOpen != null ? clipAutoplayPanelOpen : clipPopupOpenClose);
    }

    internal void PlayFreeSpinBg()
    {
        PlayLoop(bgMusicSource, clipDDFreeSpinBg != null ? clipDDFreeSpinBg : clipFreeSpinBg);
    }

    // 14. Slot Reel Column Stop Sound
    internal void PlayReelStop()
    {
        if (!_sfxEnabled || clipReelStop == null) return;

        if (wheelSegmentSource != null)
            wheelSegmentSource.PlayOneShot(clipReelStop);
        else
            PlayUISound(clipReelStop);
    }

    #region DD Reel SFX

    internal void StartDDReelSpinning() => PlayDDReelSound(clipDDReelSpinning, loop: true);

    internal void StopDDReelSpinning()
    {
        if (ddReelSource != null && ddReelSource.clip == clipDDReelSpinning)
            StopDDReelAudio();
    }

    internal void PlayDDReelLanding(bool hasInferno, bool hasBonus)
    {
        if (hasInferno)
            PlayUISound(clipDDInfernoLanding);
        else if (hasBonus)
            PlayUISound(clipDDBonusLanding);
    }

    internal void PlayDDWinLineReplay()
    {
        if (!_sfxEnabled) return;

        // Restart the cue on each replay without cutting the DD win/count-up source.
        PlayDDReelSound(clipDDWinLineReplay, loop: false);
    }

    private void PlayDDReelSound(AudioClip clip, bool loop)
    {
        if (ddReelSource == null) return;

        StopSource(ddReelSource);
        ddReelSource.clip = clip;
        if (clip == null) return;

        ddReelSource.loop = loop;
        ddReelSource.volume = _sfxEnabled ? _sfxVolume : 0f;
        ddReelSource.Play();
    }

    internal void StopDDReelAudio()
    {
        if (ddReelSource == null) return;

        StopSource(ddReelSource);
        ddReelSource.clip = null;
    }

    #endregion

    #region DD Win SFX

    internal void PlayDDWinCountUp()
    {
        PlayDDWinSound(clipDDWinning, loop: true);
    }

    internal void CompleteDDWinCountUp()
    {
        PlayDDWinSound(clipDDAfterWin);
    }

    internal void PlayDDTotalWin()
    {
        PlayDDWinSound(clipDDTotalWin);
    }

    internal void StopDDWinAudio()
    {
        StopSource(ddWinSource);
        if (ddWinSource != null)
            ddWinSource.clip = null;
    }

    private void PlayDDWinSound(AudioClip clip, bool loop = false)
    {
        StopDDWinAudio();
        if (!_sfxEnabled || ddWinSource == null || clip == null) return;

        // DD win playback follows the existing SFX controls.
        ddWinSource.clip = clip;
        ddWinSource.loop = loop;
        ddWinSource.volume = _sfxVolume;
        ddWinSource.Play();
    }

    #endregion

    private bool isForceMuted = false;

    internal void SetMuteAll(bool forceMute)
    {
        if (forceMute == isForceMuted) return;
        isForceMuted = forceMute;

        AudioListener.volume = forceMute ? 0f : 1f;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetMuteAll(!hasFocus);
    }

    private void OnApplicationPause(bool isPaused)
    {
        SetMuteAll(isPaused);
    }
}
