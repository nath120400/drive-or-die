using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A settings button that cycles one value: a volume steps through
// off / low / mid / max, the fullscreen entry toggles on / off.
// Drop it on a Button, pick the kind, assign the label
[RequireComponent(typeof(Button))]
public class SettingsCycleButton : MonoBehaviour
{
    private enum Kind { MasterVolume, MusicVolume, SfxVolume, Fullscreen }

    [SerializeField] private Kind _kind;
    [SerializeField] private TMP_Text _label;

    // Optional text shown before the level, like "Musique : "
    [SerializeField] private string _prefix = "";

    private static readonly float[] Levels = { 0f, 0.25f, 0.5f, 1f };
    private static readonly string[] LevelNames = { "OFF", "FAIBLE", "MOYEN", "MAX" };

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Cycle);
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Cycle()
    {
        SettingsManager manager = SettingsManager.Current;
        if (manager == null)
        {
            return;
        }

        GameSettings settings = manager.Settings;

        switch (_kind)
        {
            case Kind.MasterVolume:
                manager.SetMasterVolume(NextLevel(settings.MasterVolume));
                break;

            case Kind.MusicVolume:
                manager.SetMusicVolume(NextLevel(settings.MusicVolume));
                break;

            case Kind.SfxVolume:
                manager.SetSfxVolume(NextLevel(settings.SfxVolume));
                break;

            case Kind.Fullscreen:
                manager.SetFullscreen(!settings.Fullscreen);
                break;
        }

        Refresh();
    }

    private void Refresh()
    {
        SettingsManager manager = SettingsManager.Current;
        if (_label == null || manager == null)
        {
            return;
        }

        GameSettings settings = manager.Settings;

        switch (_kind)
        {
            case Kind.MasterVolume:
                _label.text = _prefix + LevelNames[IndexOf(settings.MasterVolume)];
                break;

            case Kind.MusicVolume:
                _label.text = _prefix + LevelNames[IndexOf(settings.MusicVolume)];
                break;

            case Kind.SfxVolume:
                _label.text = _prefix + LevelNames[IndexOf(settings.SfxVolume)];
                break;

            case Kind.Fullscreen:
                _label.text = _prefix + (settings.Fullscreen ? "ON" : "OFF");
                break;
        }
    }

    // The next level after the current one, wrapping around at max
    private float NextLevel(float value)
    {
        return Levels[(IndexOf(value) + 1) % Levels.Length];
    }

    // The closest level to the given value: the saved volumes always come
    // from this cycle, so this only guards hand edited SO values
    private int IndexOf(float value)
    {
        int best = 0;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < Levels.Length; i++)
        {
            float distance = Mathf.Abs(value - Levels[i]);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = i;
            }
        }

        return best;
    }
}
