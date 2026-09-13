using UnityEngine;
using UnityEngine.SceneManagement;

// The pause overlay: the run freezes and resumes with the menu
public class MenuPause : Menu
{
    public override void Open()
    {
        Time.timeScale = 0f;
        base.Open();
    }

    public override void Close()
    {
        Time.timeScale = 1f;
        base.Close();
    }

    public void QuitToMenu()
    {
        // Whatever the pause state was, the next scene starts ticking
        Time.timeScale = 1f;
        SceneManager.LoadScene(Scenes.MainMenu);
    }
}
