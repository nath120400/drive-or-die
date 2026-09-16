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
        GameObject instance = _pool.Count > 0 ? _pool.Pop() : Instantiate(Prefab, chunk.transform);

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

    public virtual void OnCollide(ref DeltaStat delta) {}

    public virtual void OnInventory(ref DeltaStat delta) {}

    public virtual void OnScore(ref DeltaStat delta) {}
}
