using UnityEngine;

public class Hud : MonoBehaviour
{
    [SerializeField] private CarManager _car;
    [SerializeField] private RectTransform _healthBackground;
    [SerializeField] private RectTransform _healthForeground;
    [SerializeField] private RectTransform _fuelBackground;
    [SerializeField] private RectTransform _fuelForeground;

    private void Update()
    {
        SetBar(_healthBackground, _healthForeground, _car.Health / _car.MaxHealth);
        SetBar(_fuelBackground, _fuelForeground, _car.Fuel / _car.MaxFuel);
    }

    // The foreground keeps its anchor on the left edge: only its width follows the ratio
    private void SetBar(RectTransform background, RectTransform foreground, float ratio)
    {
        ratio = Mathf.Clamp01(ratio);
        foreground.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, background.rect.width * ratio);
    }
}
