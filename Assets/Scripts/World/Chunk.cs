using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int Id;
    public float Length;
    public List<Entity> Entities = new List<Entity>();

    [SerializeField] private ChunkZone _zone;

    public ChunkZone Zone => _zone;
}
