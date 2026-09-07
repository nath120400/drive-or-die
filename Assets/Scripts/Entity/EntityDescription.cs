using System.Collections.Generic;
using UnityEngine;

public abstract class EntityDescription : ScriptableObject
{
    public string Description;
    public Sprite Icon;
    public GameObject Prefab;

    public float Weight;

    public int Prewarm;

    public InventorySlot InventorySlot;
    public Biome Biome;
    public EntityType Type;

    // Self-managed pool: pop to spawn, push to release, instantiate on demand
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

    public virtual void OnEntityBlueprint(ref EntityBlueprint blueprint)
    {
    }

    public virtual void OnCollide(ref DeltaStat delta)
    {
    }

    public virtual void InInventory(ref DeltaStat delta)
    {
    }

    public virtual void OnScore()
    {
    }
}
