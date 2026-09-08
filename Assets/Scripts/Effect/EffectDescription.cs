using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Description")]
public class EffectDescription : ScriptableObject
{
    [SerializeField] private Effect _prefab;

    // Self-managed pool: pop to spawn, push to release, instantiate on demand
    private readonly Stack<Effect> _pool = new Stack<Effect>();

    public Effect Spawn(Vector3 position, Transform parent)
    {
        Effect effect = _pool.Count > 0 ? _pool.Pop() : Instantiate(_prefab, parent);

        effect.Description = this;
        effect.transform.SetParent(parent, false);
        effect.transform.position = position;
        effect.gameObject.SetActive(true);
        effect.Play();
        return effect;
    }

    public void Release(Effect effect)
    {
        effect.gameObject.SetActive(false);
        _pool.Push(effect);
    }
}
