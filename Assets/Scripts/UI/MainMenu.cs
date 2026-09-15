using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// The main menu screen: start, settings, quit, and the persistent high scores
public class MainMenu : Menu
{
    [SerializeField] private MenuManager _menus;
    [SerializeField] private Menu _settings;
    [SerializeField] private TMP_Text _highScores;

    private void Start()
    {
        _highScores.text = HighScores.Format();
    }

    public void ShowSettings() => _menus.Show(_settings);

    public void Play() => SceneManager.LoadScene(Scenes.Game);

    public void Quit() => Application.Quit();
}
