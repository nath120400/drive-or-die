using UnityEngine;
using UnityEngine.UI;

// The settings menu prefab: sliders read from and write to the live settings,
// the manager pushes them to the mixer and persists them on demand
public class MenuSettings : Menu
{
    [SerializeField] private SettingsManager _settingsManager;
    [SerializeField] private Slider _masterVolume;
    [SerializeField] private Slider _musicVolume;
    [SerializeField] private Slider _sfxVolume;

    public override void Open()
    {
        base.Open();
        Refresh();
    }

    public override void Close()
    {
        // Closing keeps the settings: they are simply pushed, not saved
        _settingsManager.Apply();
        base.Close();
    }

    private void Refresh()
    {
        GameSettings settings = _settingsManager.Settings;

        _masterVolume.value = settings.MasterVolume;
        _musicVolume.value = settings.MusicVolume;
        _sfxVolume.value = settings.SfxVolume;
    }
}
