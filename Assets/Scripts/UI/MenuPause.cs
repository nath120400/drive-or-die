using UnityEngine;

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
}
