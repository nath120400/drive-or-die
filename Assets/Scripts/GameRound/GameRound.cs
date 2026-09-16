using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The round: the single brain of the game scene. The run ticks by itself,
// the escape input pauses, the loss fades to black before hopping to the
// game over scene — the pause panel is a mute GameObject, its buttons wire
// here, the fade is a black Image, active by default, whose color alpha moves
public class GameRound : MonoBehaviour
{
    [SerializeField] private InputActionReference _pauseAction;
    [SerializeField] private RunManager _run;
    [SerializeField] private GameObject _pausePanel;

    [Header("Fade")]
    [SerializeField] private Image _fade;
    [SerializeField] private float _fadeDuration = 0.4f;

    [Header("Audio")]
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private float _audioFadeDuration = 0.4f;

    private bool _over;
    private Coroutine _fading;
    private Coroutine _audioFading;

    private void Start()
    {
        // The scene opens covered: the Image clears, then shuts down
        FadeTo(0f, () => _fade.gameObject.SetActive(false));

        // The sound climbs from silence to the user level
        FadeAudio(Settings.Decibels, Settings.MuteDecibels);
    }

    private void OnEnable()
    {
        _pauseAction.action.performed += OnPause;
    }

    private void OnDisable()
    {
        _pauseAction.action.performed -= OnPause;
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        // Once over, the scene is already leaving: the pause is gone.
        // Same during a fade, whichever direction it runs
        if (_over || _fading != null)
        {
            return;
        }

        bool paused = !_pausePanel.activeSelf;
        _pausePanel.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;
    }

    public void QuitToMenu()
    {
        // Whatever the pause state was, the next scene starts ticking
        Time.timeScale = 1f;
        SceneManager.LoadScene(Scenes.MainMenu);
    }

    private void Update()
    {
        // The death plays once: ship the results, cover the screen,
        // hop with the timescale reset once covered
        if (_run.IsOver && !_over)
        {
            _over = true;
            RunResult.Score = _run.Score;
            _fade.gameObject.SetActive(true);
            FadeAudio(Settings.MuteDecibels);
            FadeTo(1f, () =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(Scenes.GameOver);
            });
        }
    }

    private void FadeTo(float target, Action done)
    {
        if (_fading != null)
        {
            StopCoroutine(_fading);
        }

        _fading = StartCoroutine(FadeRoutine(target, done));
    }

    // Only the alpha moves, the color keeps whatever it is
    private IEnumerator FadeRoutine(float target, Action done)
    {
        Color color = _fade.color;
        float start = color.a;
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / _fadeDuration)
        {
            color.a = Mathf.Lerp(start, target, t);
            _fade.color = color;
            yield return null;
        }

        color.a = target;
        _fade.color = color;
        done?.Invoke();
        _fading = null;
    }

    private void FadeAudio(float target, float? from = null)
    {
        if (_mixer == null)
        {
            return;
        }

        if (_audioFading != null)
        {
            StopCoroutine(_audioFading);
        }

        _audioFading = StartCoroutine(AudioFadeRoutine(target, from));
    }

    // The mixer rides decibels: the fade lerps between the start
    // (the explicit one, or the current value) and the target,
    // unscaled time so the pause never stalls it
    private IEnumerator AudioFadeRoutine(float target, float? from)
    {
        _mixer.GetFloat(Settings.VolumeParameter, out float start);
        if (from.HasValue)
        {
            start = from.Value;
        }

        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / _audioFadeDuration)
        {
            _mixer.SetFloat(Settings.VolumeParameter, Mathf.Lerp(start, target, t));
            yield return null;
        }

        _mixer.SetFloat(Settings.VolumeParameter, target);
        _audioFading = null;
    }
}
