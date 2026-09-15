using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

// The runtime holder of the game settings: the SO is the authoring copy,
// this one is the live copy loaded at startup and saved on demand
public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameSettings _settings;
    [SerializeField] private AudioMixer _mixer;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SFXVolume";
    private const string FullscreenKey = "Fullscreen";

    public GameSettings Settings => _settings;

    // The last loaded manager wins: it survives and kills the stale one,
    // so the scene local reference always stays valid
    private static SettingsManager _current;

    public static SettingsManager Current => _current;

    private void Awake()
    {
        if (_current != null && _current != this)
        {
            Destroy(_current.gameObject);
        }

        _current = this;
        DontDestroyOnLoad(gameObject);
        Apply();
    }

    // Slider hooks: they write the live copy and push it right away,
    // so the slider drags are audible in real time
    public void SetMasterVolume(float value)
    {
        _settings.MasterVolume = value;
        PushToMixer(MasterVolumeKey, value);
    }

    public void SetMusicVolume(float value)
    {
        _settings.MusicVolume = value;
        PushToMixer(MusicVolumeKey, value);
    }

    public void SetSfxVolume(float value)
    {
        _settings.SfxVolume = value;
        PushToMixer(SfxVolumeKey, value);
    }

    public void SetFullscreen(bool value)
    {
        _settings.Fullscreen = value;
        Screen.fullScreen = value;
    }

    public void Apply()
    {
        _settings.MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, _settings.MasterVolume);
        _settings.MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, _settings.MusicVolume);
        _settings.SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, _settings.SfxVolume);
        _settings.Fullscreen = PlayerPrefs.GetInt(FullscreenKey, _settings.Fullscreen ? 1 : 0) == 1;

        PushToMixer(MasterVolumeKey, _settings.MasterVolume);
        PushToMixer(MusicVolumeKey, _settings.MusicVolume);
        PushToMixer(SfxVolumeKey, _settings.SfxVolume);
        Screen.fullScreen = _settings.Fullscreen;
    }

    public void Save()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, _settings.MasterVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, _settings.MusicVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, _settings.SfxVolume);
        PlayerPrefs.SetInt(FullscreenKey, _settings.Fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    // The master volume ramps from silence to its saved value
    public void FadeIn(float duration)
    {
        StartCoroutine(FadeRoutine(0f, _settings.MasterVolume, duration));
    }

    // The master volume ramps down to silence
    public void FadeOut(float duration)
    {
        StartCoroutine(FadeRoutine(_settings.MasterVolume, 0f, duration));
    }

    private IEnumerator FadeRoutine(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            PushToMixer(MasterVolumeKey, Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }

        PushToMixer(MasterVolumeKey, to);
    }

    private void PushToMixer(string parameter, float volume)
    {
        // The mixer works in decibels: 0 maps to -80 dB, 1 maps to 0 dB
        _mixer.SetFloat(parameter, Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f);
    }
}
