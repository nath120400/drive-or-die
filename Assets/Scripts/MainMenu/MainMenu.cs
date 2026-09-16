using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// The main menu screen: start, quit, and the persistent high scores
public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text _highScores;

    private void Start()
    {
        _highScores.text = HighScores.Format();
    }

    public void Play() => SceneManager.LoadScene(Scenes.GameRound);

    public void Quit() => Application.Quit();
}
