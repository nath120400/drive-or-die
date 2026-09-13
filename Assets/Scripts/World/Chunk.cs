using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    // The length is the depth of its zone: everything else derives from it
    public float Length => Zone.Max.y - Zone.Min.y;
    public List<Entity> Entities = new List<Entity>();

    [SerializeField] private ChunkZone _zone;

    public ChunkZone Zone => _zone;
}
