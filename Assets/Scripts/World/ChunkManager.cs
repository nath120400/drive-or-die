using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public int AheadChunks;
    public int Target;
    public int Tries;
    public int XCells;
    public int YCells;
    public float Border;
    public List<GameObject> ChunkPrefabs = new List<GameObject>();
    public EntityDatabase Database;
    public List<Chunk> Chunks = new List<Chunk>();

    // The scene car: it drives the run state
    [SerializeField] private CarManager _car;

    private readonly List<EntityBlueprint> _blueprints = new List<EntityBlueprint>();

    // Flat grid, index = row * ChunkCols + col: allocated once, cleared every chunk, zero GC
    private readonly List<EntityDescription> _grid = new List<EntityDescription>();

    // Blocks recycling in Update until the ring is filled
    private bool _initialized;

    // One-time fill: the ring is built with AheadChunks brand new chunks,
    // recycling only starts after every chunk exists
    private void OnEnable()
    {
        for (int i = 0; i < AheadChunks; i++)
        {
            NewChunk(_car, CreateChunk());
        }

        _initialized = true;
    }

    // The chunk comes from outside: a fresh one during the fill, the recycled
    // head otherwise; this only places and seeds it
    public void NewChunk(CarManager car, Chunk chunk)
    {
        // Set its Id (last chunk + 1) and place it right after the previous chunk
        chunk.Id = Chunks.Count > 0 ? Chunks[Chunks.Count - 1].Id + 1 : 0;
        chunk.transform.localPosition = Chunks.Count > 0
            ? Chunks[Chunks.Count - 1].transform.localPosition + Vector3.forward * Chunks[Chunks.Count - 1].Length
            : Vector3.zero;
        Chunks.Add(chunk);

        // The prefab zone marker drives the grid: its horizontal rect divides into
        // XCells x YCells cells exactly, the zone depth is the chunk length
        ChunkZone zone = chunk.Zone;
        float zoneWidth = zone.Max.x - zone.Min.x;
        float zoneDepth = zone.Max.y - zone.Min.y;
        int chunkCols = XCells;
        int chunkRows = YCells;
        chunk.Length = zoneDepth;

        float cellSizeX = zoneWidth / chunkCols;
        float cellSizeY = zoneDepth / chunkRows;

        // Rebuild its blueprints:
        // 1. Clear the chunk entities and the blueprint list (no reallocation)
        chunk.Entities.Clear();
        _blueprints.Clear();
        // 2. Gather candidate EntityDescriptions, filtered by Biome
        // 3. For each candidate, create an EntityBlueprint (Description + base Weight),
        //    then the car inventory items adjust the weight
        foreach (EntityDescription description in Database.Descriptions)
        {
            EntityBlueprint blueprint = new EntityBlueprint { Description = description, Weight = description.Weight };

            FilterBlueprint(ref blueprint);

            foreach (EntityDescription item in car.Inventory)
            {
                item.OnEntityBlueprint(ref blueprint);
            }

            _blueprints.Add(blueprint);
        }
        // 4. Drop weights <= 0
        for (int i = _blueprints.Count - 1; i >= 0; i--)
        {
            if (_blueprints[i].Weight <= 0f)
            {
                _blueprints.RemoveAt(i);
            }
        }

        // 5. Total weight feeds the weighted random draw
        float totalWeight = 0f;
        for (int i = 0; i < _blueprints.Count; i++)
        {
            totalWeight += _blueprints[i].Weight;
        }

        // 6. Dart throwing: per spawned entity, a fresh budget of random tries,
        //    the grid holds one entity per cell; the size follows the zone,
        //    so it is rebuilt when the recycled chunk is a different variant
        if (_grid.Count != chunkCols * chunkRows)
        {
            _grid.Clear();
            for (int i = 0; i < chunkCols * chunkRows; i++)
            {
                _grid.Add(null);
            }
        }

        for (int i = 0; i < _grid.Count; i++)
        {
            _grid[i] = null;
        }

        for (int placed = 0; placed < Target; placed++)
        {
            for (int tryIndex = 0; tryIndex < Tries; tryIndex++)
            {
                int row = Random.Range(0, chunkRows);
                int col = Random.Range(0, chunkCols);
                int index = row * chunkCols + col;
                if (_grid[index] != null)
                {
                    continue;
                }

                _grid[index] = DrawBlueprint(totalWeight);
                break;
            }
        }

        // 7. Placement: each row shifts sideways by up to one cell (cell * random - cell / 2),
        //    the edge column in the shift direction is dropped so nothing overhangs,
        //    entities jitter inside their cell minus the border, two neighbors
        //    never come closer than 2 x Border
        float jitterX = cellSizeX * 0.5f - Border;
        float jitterY = cellSizeY * 0.5f - Border;
        for (int row = 0; row < chunkRows; row++)
        {
            float rowOffset = cellSizeX * Random.value - cellSizeX * 0.5f;
            if (rowOffset > 0f)
            {
                _grid[row * chunkCols + chunkCols - 1] = null;
            }
            else
            {
                _grid[row * chunkCols] = null;
            }

            for (int col = 0; col < chunkCols; col++)
            {
                EntityDescription description = _grid[row * chunkCols + col];
                if (description == null)
                {
                    continue;
                }

                // Cells tile the zone in its own local frame, so a tilted or rotated
                // marker carries the entities with it; Altitude is the zone base
                Vector3 zoneLocal = new Vector3(
                    zone.Min.x + (col + 0.5f) * cellSizeX + rowOffset + Random.Range(-jitterX, jitterX),
                    zone.Altitude,
                    zone.Min.y + (row + 0.5f) * cellSizeY + Random.Range(-jitterY, jitterY));
                Vector3 position = chunk.transform.InverseTransformPoint(zone.transform.TransformPoint(zoneLocal));

                Entity entity = description.Spawn(position, chunk);
                entity.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                chunk.Entities.Add(entity);
            }
        }
    }

    // The world slides toward the car at its forward speed; when the head chunk
    // slid its own length behind the local zero, NewChunk recycles it to the tail
    private void Update()
    {
        Vector3 motion = Vector3.back * (_car.ForwardSpeed * Time.deltaTime);
        for (int i = 0; i < Chunks.Count; i++)
        {
            Chunks[i].transform.localPosition += motion;
        }

        // Until the ring is filled, never recycle
        if (!_initialized)
        {
            return;
        }

        Chunk first = Chunks[0];
        if (first.transform.localPosition.z < 0f)
        {
            Chunks.RemoveAt(0);
            Debug.Log($"[ChunkManager] Recycled chunk {first.Id}: z={first.transform.localPosition.z:F1}");

            // Free its entities back to their pools:
            // Release detaches each one from the list, so walk it backwards
            for (int i = first.Entities.Count - 1; i >= 0; i--)
            {
                first.Entities[i].Description.Release(first.Entities[i]);
            }

            NewChunk(_car, first);
        }
    }

    // Odds weighted by the blueprint weights
    private EntityDescription DrawBlueprint(float totalWeight)
    {
        float roll = Random.value * totalWeight;
        for (int i = 0; i < _blueprints.Count; i++)
        {
            roll -= _blueprints[i].Weight;
            if (roll <= 0f)
            {
                return _blueprints[i].Description;
            }
        }

        return _blueprints[_blueprints.Count - 1].Description;
    }

    private void FilterBlueprint(ref EntityBlueprint blueprint)
    {
    }

    // A random prefab variant becomes a chunk
    private Chunk CreateChunk()
    {
        if (ChunkPrefabs.Count > 0)
        {
            GameObject prefab = ChunkPrefabs[Random.Range(0, ChunkPrefabs.Count)];
            return Instantiate(prefab, transform).GetComponent<Chunk>();
        }

        Chunk created = new GameObject("Chunk").AddComponent<Chunk>();
        created.transform.SetParent(transform);
        return created;
    }
}
