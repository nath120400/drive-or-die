using System.Collections.Generic;
using UnityEngine;

// The menu router: exactly one menu open at a time, Show/CloseAll only
public class MenuManager : MonoBehaviour
{
    [SerializeField] private List<Menu> _menus = new List<Menu>();

    private Menu _current;

    private void Start()
    {
        // Boot clean: nothing open before the first Show
        CloseAll();
    }

    public void Show(Menu menu)
    {
        if (_current != null && _current != menu)
        {
            _current.Close();
        }

        _current = menu;
        menu.Open();
    }

    public void Close(Menu menu)
    {
        if (_current == menu)
        {
            _current = null;
        }

        menu.Close();
    }

    public void CloseAll()
    {
        for (int i = 0; i < _menus.Count; i++)
        {
            _menus[i].Close();
        }

        _current = null;
    }
}
