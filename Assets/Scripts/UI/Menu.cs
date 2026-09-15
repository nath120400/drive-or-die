using UnityEngine;

// One abstract menu: every screen overrides Open/Close and owns its domain
public abstract class Menu : MonoBehaviour
{
    public virtual void Open() => gameObject.SetActive(true);
    public virtual void Hide() => gameObject.SetActive(false);
    public virtual void Close() => gameObject.SetActive(false);
}
