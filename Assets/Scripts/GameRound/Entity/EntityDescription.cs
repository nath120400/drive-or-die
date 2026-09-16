using System.Collections.Generic;
using UnityEngine;

public abstract class EntityDescription : ScriptableObject
{
    // Front
    public string Description;
    public Sprite Icon;
    public GameObject Prefab;
    public List<EffectDescription> Effects = new List<EffectDescription>();

    // Stats
    public float MinWeight;
    public float MaxWeight;
    public float Radius;

    // Pool
    private readonly Stack<GameObject> _pool = new Stack<GameObject>();

    public Entity Spawn(Vector3 position, Chunk chunk)
    {
        // The pool outlives the scenes: destroyed instances die on the stack,
        // so pop until a live one comes out
        GameObject instance = null;
        while (_pool.Count > 0 && instance == null)
        {
            instance = _pool.Pop();
        }

        if (instance == null)
        {
            instance = Instantiate(Prefab, chunk.transform);
        }

        Entity entity = instance.GetComponent<Entity>();
        entity.Description = this;
        entity.Chunk = chunk;
        instance.transform.SetParent(chunk.transform);
        instance.transform.localPosition = position;
        instance.SetActive(true);
        return entity;
    }

    public void Release(Entity entity)
    {
        // Detach from its chunk so it can never be released twice
        entity.Chunk.Entities.Remove(entity);
        entity.gameObject.SetActive(false);
        _pool.Push(entity.gameObject);
    }

    public virtual void OnEntityBlueprint(ref EntityBlueprint blueprint) {}

    public virtual void OnCollide(ref TempState delta) {}

    public virtual void OnInventory(ref TempState delta) {}

    public virtual void OnScore(ref TempState delta) {}
}
