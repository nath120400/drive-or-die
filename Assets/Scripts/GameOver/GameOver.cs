using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// The game over screen: the final score, flagged when it beats the record,
// restart or back to menu
public class GameOver : MonoBehaviour
{
    [SerializeField] private TMP_Text _score;

    private void Start()
    {
        // The record check reads the board before the run lands in it
        bool newRecord = RunResult.Score > HighScores.BestScore();
        HighScores.Submit(RunResult.Score);

        _score.text = newRecord
            ? $"{RunResult.Score:F0} (new)"
            : RunResult.Score.ToString("F0");
    }

    public void Restart() => SceneManager.LoadScene(Scenes.GameRound);

    public void QuitToMenu() => SceneManager.LoadScene(Scenes.MainMenu);
}
