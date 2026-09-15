using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// The game scene controller: the pause input toggles the pause menu,
// the run over state hops to the game over scene with the results
public class GameSceneController : MonoBehaviour
{
    [SerializeField] private MenuManager _menus;
    [SerializeField] private Menu _pause;
    [SerializeField] private InputActionReference _pauseAction;
    [SerializeField] private CarManager _car;
    [SerializeField] private SettingsManager _settings;
    [SerializeField] private ScreenFade _screenFade;
    [SerializeField] private AudioPlayer _audioPlayer;
    [SerializeField] private int _loadFrames = 3;

    private void Start()
    {
        // The scene opens in the dark and on pause: the world only starts
        // once a few frames have been rendered under the black
        Time.timeScale = 0f;
        StartCoroutine(Open());
    }

    private System.Collections.IEnumerator Open()
    {
        for (int i = 0; i < _loadFrames; i++)
        {
            yield return null;
        }

        Time.timeScale = 1f;
        _audioPlayer.Play();
        _settings.FadeIn(2f);
        _screenFade.FadeOut();
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
        if (Time.timeScale > 0f)
        {
            _menus.Show(_pause);
        }
        else
        {
            // Settings on top of the pause: escape first walks back down
            _menus.CloseTop();
        }
    }

    private bool _dying;

    private void Update()
    {
        // The death plays once: fade to black, then the results scene
        if (_car.State.IsOver && !_dying)
        {
            _dying = true;
            StartCoroutine(Death());
        }
    }

    private System.Collections.IEnumerator Death()
    {
        // The world keeps rolling under the fade while the sound sinks
        _screenFade.FadeIn();
        _settings.FadeOut(1.5f);

        yield return new WaitForSecondsRealtime(1.5f);

        // Ship the results before the run state dies with the scene,
        // and make sure the next scene starts ticking
        RunResult.Score = _car.State.Score;
        Time.timeScale = 1f;
        SceneManager.LoadScene(Scenes.GameOver);
    }
}
