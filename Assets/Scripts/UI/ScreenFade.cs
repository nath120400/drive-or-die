using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Full screen black overlay: it opens opaque, fades out when the run starts
// and fades back in when it ends
public class ScreenFade : MonoBehaviour
{
    [SerializeField] private Image _black;
    [SerializeField] private float _duration = 1f;

    private Coroutine _fade;

    private void Awake()
    {
        // The scene opens in the dark
        SetAlpha(1f);
    }

    public void FadeOut()
    {
        if (_fade == null)
        {
            _fade = StartCoroutine(FadeTo(0f));
        }
    }

    public void FadeIn()
    {
        if (_fade == null)
        {
            _fade = StartCoroutine(FadeTo(1f));
        }
    }

    private IEnumerator FadeTo(float target)
    {
        float start = _black.color.a;
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / _duration)));
            yield return null;
        }

        SetAlpha(target);
        _fade = null;
    }

    private void SetAlpha(float alpha)
    {
        Color color = _black.color;
        color.a = alpha;
        _black.color = color;
    }
}
