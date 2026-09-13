using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// The game over screen: reads the finished run results, retry or back to menu
public class GameOver : MonoBehaviour
{
    [SerializeField] private TMP_Text _score;
    [SerializeField] private TMP_Text _distance;

    private void Start()
    {
        _score.text = RunResult.Score.ToString("F0");

        // Meters: the run state accumulates raw units at car speed
        _distance.text = $"{RunResult.Distance:F0} m";
    }

    public void Retry() => SceneManager.LoadScene(Scenes.Game);

    public void Quit() => SceneManager.LoadScene(Scenes.MainMenu);
}
