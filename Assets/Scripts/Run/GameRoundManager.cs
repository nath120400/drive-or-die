using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// The round: the single brain of the game scene. The run ticks by itself,
// the escape input pauses, the loss hops to the game over scene with
// the results — the pause panel is a mute GameObject, its buttons wire here
public class GameRoundManager : MonoBehaviour
{
    [SerializeField] private InputActionReference _pauseAction;
    [SerializeField] private RunManager _run;
    [SerializeField] private GameObject _pausePanel;

    private bool _over;

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
        // Once over, the scene is already leaving: the pause is gone
        if (_over)
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
        // The death plays once: ship the results before the state
        // dies with the scene, then hop with the timescale reset
        if (_run.State.IsOver && !_over)
        {
            _over = true;
            RunResult.Score = _run.State.Score;
            Time.timeScale = 1f;
            SceneManager.LoadScene(Scenes.GameOver);
        }
    }
}
