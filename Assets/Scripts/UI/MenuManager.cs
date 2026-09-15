using System.Collections.Generic;
using UnityEngine;

// The menu router: a stack of menus, the top one is the only one visible.
// Covered menus stay open underneath (a pause stays frozen behind settings),
// closing pops the top back to the one before it
public class MenuManager : MonoBehaviour
{
    [SerializeField] private List<Menu> _menus = new List<Menu>();

    // Open menus, bottom to top
    private readonly List<Menu> _open = new List<Menu>();

    private void Start()
    {
        CloseAll();
    }

    public void Show(Menu menu)
    {
        int index = _open.IndexOf(menu);

        if (index >= 0)
        {
            // Raising a covered menu closes everything stacked above it
            _open.RemoveRange(index + 1, _open.Count - index - 1);
            _open.RemoveAt(index);
        }

        _open.Add(menu);
        Refresh();
    }

    // Closes the menu and everything stacked above it
    public void Close(Menu menu)
    {
        int index = _open.IndexOf(menu);
        if (index >= 0)
        {
            _open.RemoveRange(index, _open.Count - index);
        }

        Refresh();
    }

    public void CloseTop()
    {
        if (_open.Count > 0)
        {
            Close(_open[_open.Count - 1]);
        }
    }

    public void CloseAll()
    {
        _open.Clear();
        Refresh();
    }

    // The top stays as it is, covered menus hide, closed menus close
    private void Refresh()
    {
        Menu top = _open.Count > 0 ? _open[_open.Count - 1] : null;

        for (int i = 0; i < _menus.Count; i++)
        {
            Menu menu = _menus[i];
            if (menu == top)
            {
                continue;
            }

            if (_open.Contains(menu))
            {
                menu.Hide();
            }
            else
            {
                menu.Close();
            }
        }

        top?.Open();
    }
}
