using UnityEngine;
using UnityEngine.Audio;

// The runtime holder of the game settings: the SO is the authoring copy,
// this one is the live copy loaded at startup and saved on demand
public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameSettings _settings;
    [SerializeField] private AudioMixer _mixer;

    private const string MasterVolumeKey = "Settings.MasterVolume";
    private const string MusicVolumeKey = "Settings.MusicVolume";
    private const string SfxVolumeKey = "Settings.SfxVolume";

    public GameSettings Settings => _settings;

    // Slider hooks: they write the live copy without touching the mixer yet
    public void SetMasterVolume(float value) => _settings.MasterVolume = value;
    public void SetMusicVolume(float value) => _settings.MusicVolume = value;
    public void SetSfxVolume(float value) => _settings.SfxVolume = value;

    public void Apply()
    {
        _settings.MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, _settings.MasterVolume);
        _settings.MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, _settings.MusicVolume);
        _settings.SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, _settings.SfxVolume);

        PushToMixer(MasterVolumeKey, _settings.MasterVolume);
        PushToMixer(MusicVolumeKey, _settings.MusicVolume);
        PushToMixer(SfxVolumeKey, _settings.SfxVolume);
    }

    public void Save()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, _settings.MasterVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, _settings.MusicVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, _settings.SfxVolume);
        PlayerPrefs.Save();
    }

    private void PushToMixer(string parameter, float volume)
    {
        // The mixer works in decibels: 0 maps to -80 dB, 1 maps to 0 dB
        _mixer.SetFloat(parameter, Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f);
    }
}
