using TMPro;
using UnityEngine;
using UnityEngine.Audio;

// The settings screen: the volume button cycles the level down, the
// fullscreen one flips the mode, both persist in PlayerPrefs. The level
// is static so every scene reads it; the mixer rides decibels and the
// linear levels convert with 20 * log10
public class Settings : MonoBehaviour
{
    private const string VolumeKey = "MasterVolume";
    private const string FullscreenKey = "Fullscreen";
    public const string VolumeParameter = "MasterVolume";
    public const float MuteDecibels = -80f;

    private static readonly float[] LevelLinear = { 0f, 0.33f, 0.66f, 1f };
    private static readonly string[] LevelNames = { "Sound Off", "Sound Low", "Sound Mid", "Sound High" };

    private static int _level;
    private static bool _loaded;

    // The chosen level: it loads lazily, PlayerPrefs refuses
    // to be called from the static constructor
    public static int Level
    {
        get
        {
            if (!_loaded)
            {
                _level = Mathf.Clamp(PlayerPrefs.GetInt(VolumeKey, 3), 0, LevelLinear.Length - 1);
                _loaded = true;
            }

            return _level;
        }
    }

    // The user level in decibels: the mixer target wherever audio fades
    public static float Decibels => Level == 0 ? MuteDecibels : Mathf.Log10(LevelLinear[Level]) * 20f;

    [Header("Volume")]
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private TMP_Text _volumeText;

    [Header("Fullscreen")]
    [SerializeField] private TMP_Text _fullscreenText;

    private void Awake()
    {
        // The saved states land in the mixer and the screen
        // even without pressing anything
        if (PlayerPrefs.HasKey(FullscreenKey))
        {
            Screen.fullScreen = PlayerPrefs.GetInt(FullscreenKey) == 1;
        }

        ApplyVolume();
        RefreshFullscreen();
    }

    // The volume button action: one step down, Sound Off wraps back to Sound High
    public void CycleVolume()
    {
        _level = (Level + LevelLinear.Length - 1) % LevelLinear.Length;
        _loaded = true;
        PlayerPrefs.SetInt(VolumeKey, _level);
        ApplyVolume();
    }

    public void ToggleFullscreen()
    {
        bool fullscreen = !Screen.fullScreen;
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
        RefreshFullscreen();
    }

    private void ApplyVolume()
    {
        if (_mixer != null)
        {
            _mixer.SetFloat(VolumeParameter, Decibels);
        }

        if (_volumeText != null)
        {
            _volumeText.text = LevelNames[Level];
        }
    }

    private void RefreshFullscreen()
    {
        if (_fullscreenText != null)
        {
            _fullscreenText.text = Screen.fullScreen ? "Fullscreen" : "Windowed";
        }
    }
}
