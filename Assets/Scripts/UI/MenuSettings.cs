using UnityEngine;

// The settings menu prefab: cycle buttons handle the volumes and fullscreen,
// the manager persists the settings on demand when the menu closes
public class MenuSettings : Menu
{
    [SerializeField] private SettingsManager _settingsManager;

    public override void Close()
    {
        // Closing keeps the settings: they persist rather than reload
        _settingsManager.Save();
        base.Close();
    }
}
