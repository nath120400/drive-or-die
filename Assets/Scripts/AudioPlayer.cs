using System.Collections;
using UnityEngine;

// The scene audio: the motor and the song ramp from silence when the run opens.
// The controller starts it once the world actually moves
public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource _motor;
    [SerializeField] private AudioSource _song;
    [SerializeField] private float _duration = 2f;

    public void Play()
    {
        _motor.Play();
        _song.Play();
        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        _motor.volume = 0f;
        _song.volume = 0f;

        float elapsed = 0f;
        while (elapsed < _duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / _duration);

            _motor.volume = progress;
            _song.volume = progress;
            yield return null;
        }

        _motor.volume = 1f;
        _song.volume = 1f;
    }
}
