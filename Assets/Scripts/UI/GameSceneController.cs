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
            _menus.Close(_pause);
        }
    }

    private void Update()
    {
        if (_car.State.IsOver)
        {
            enabled = false;

            // Ship the results before the run state dies with the scene,
            // and make sure the next scene starts ticking even from a pause
            RunResult.Score = _car.State.Score;
            RunResult.Distance = _car.State.Distance;
            Time.timeScale = 1f;
            SceneManager.LoadScene(Scenes.GameOver);
        }
    }
}
