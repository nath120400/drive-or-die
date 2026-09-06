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

    public Entity Spawn(Vector3 position, Transform parent)
    {
        GameObject instance = _pool.Count > 0 ? _pool.Pop() : Instantiate(Prefab, parent);

        Entity entity = instance.GetComponent<Entity>();
        entity.Description = this;
        instance.transform.SetParent(parent);
        instance.transform.localPosition = position;
        instance.SetActive(true);
        return entity;
    }

    public void Release(Entity entity)
    {
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
