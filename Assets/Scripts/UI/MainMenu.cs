using UnityEngine;
using UnityEngine.SceneManagement;

// The main menu screen: start, settings, quit
public class MainMenu : Menu
{
    [SerializeField] private MenuManager _menus;
    [SerializeField] private Menu _settings;

    public void ShowSettings() => _menus.Show(_settings);

    public void Play() => SceneManager.LoadScene("GameScene");

    public void Quit() => Application.Quit();
}
