using TMPro;
using UnityEngine;

public class Hud : MonoBehaviour
{
    [SerializeField] private RunManager _run;
    [SerializeField] private RectTransform _healthBackground;
    [SerializeField] private RectTransform _healthForeground;
    [SerializeField] private RectTransform _fuelBackground;
    [SerializeField] private RectTransform _fuelForeground;
    [SerializeField] private TMP_Text _score;

    private void Update()
    {
        SetBar(_healthBackground, _healthForeground, _run.Health / _run.MaxHealth);
        SetBar(_fuelBackground, _fuelForeground, _run.Fuel / _run.MaxFuel);

        _score.text = _run.Score.ToString("F0");
    }

    // The foreground keeps its anchor on the left edge: only its width follows the ratio
    private void SetBar(RectTransform background, RectTransform foreground, float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        foreground.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, background.rect.width * ratio);
    }
}
