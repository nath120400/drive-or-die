using UnityEngine;

// The description it spawned from and the chunk holding it: the pool needs both
public class Entity : MonoBehaviour
{
    [HideInInspector] public EntityDescription Description;
    [HideInInspector] public Chunk Chunk;
}
