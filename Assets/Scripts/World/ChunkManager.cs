using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public int ChunkCols;
    public int ChunkRows;
    public int AheadChunks;
    public int BehindChunks;
    public int Target;
    public int Tries;
    public float CellSizeX;
    public float CellSizeY;
    public float Border;
    public EntityDatabase Database;
    public List<Chunk> Chunks = new List<Chunk>();

    // The scene car: it drives the run state
    [SerializeField] private CarManager _car;

    private readonly List<EntityBlueprint> _blueprints = new List<EntityBlueprint>();

    // Flat grid, index = row * ChunkCols + col: allocated once, cleared every chunk, zero GC
    private readonly List<EntityDescription> _grid = new List<EntityDescription>();

    public void NewChunk(CarManager car)
    {
        // Take a free chunk from the pool (or create one)
        Chunk chunk = TakeFreeChunk();

        // Set its Id (last chunk + 1) and place it after the previous chunk in the scene
        chunk.Id = Chunks.Count > 0 ? Chunks[Chunks.Count - 1].Id + 1 : 0;
        chunk.transform.position = Chunks.Count > 0
            ? Chunks[Chunks.Count - 1].transform.position + Vector3.forward * ChunkRows * CellSizeY
            : Vector3.zero;
        Chunks.Add(chunk);

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
        //    the grid holds one entity per cell
        if (_grid.Count != ChunkRows * ChunkCols)
        {
            for (int i = 0; i < ChunkRows * ChunkCols; i++)
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
                int row = Random.Range(0, ChunkRows);
                int col = Random.Range(0, ChunkCols);
                int index = row * ChunkCols + col;
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
        float halfWidth = ChunkCols * CellSizeX * 0.5f;
        float jitterX = CellSizeX * 0.5f - Border;
        float jitterY = CellSizeY * 0.5f - Border;
        for (int row = 0; row < ChunkRows; row++)
        {
            float rowOffset = CellSizeX * Random.value - CellSizeX * 0.5f;
            if (rowOffset > 0f)
            {
                _grid[row * ChunkCols + ChunkCols - 1] = null;
            }
            else
            {
                _grid[row * ChunkCols] = null;
            }

            for (int col = 0; col < ChunkCols; col++)
            {
                EntityDescription description = _grid[row * ChunkCols + col];
                if (description == null)
                {
                    continue;
                }

                float x = (col + 0.5f) * CellSizeX - halfWidth + rowOffset + Random.Range(-jitterX, jitterX);
                float y = (row + 0.5f) * CellSizeY + Random.Range(-jitterY, jitterY);

                Vector3 position = new Vector3(x, 0f, y);
                Entity entity = description.Spawn(position, chunk.transform);
                entity.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                chunk.Entities.Add(entity);
            }
        }
    }

    // The world slides toward the car at its forward speed, and we keep enough chunks ahead of it
    private void Update()
    {
        Vector3 motion = Vector3.back * (_car.ForwardSpeed * Time.deltaTime);
        for (int i = 0; i < Chunks.Count; i++)
        {
            Chunks[i].transform.position += motion;
        }

        if (Chunks.Count == 0)
        {
            NewChunk(_car);
            return;
        }

        float chunkLength = ChunkRows * CellSizeY;
        Chunk last = Chunks[Chunks.Count - 1];
        float lastEnd = last.transform.position.z + chunkLength;
        if (_car.transform.position.z + AheadChunks * chunkLength >= lastEnd)
        {
            Debug.Log($"[ChunkManager] NewChunk {last.Id + 1}: last.z={last.transform.position.z:F1} lastEnd={lastEnd:F1} car.z={_car.transform.position.z:F1}");
            NewChunk(_car);
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

    // The first chunk, far enough behind the car, is recycled to the front of the ring
    private Chunk TakeFreeChunk()
    {
        float chunkLength = ChunkRows * CellSizeY;
        if (Chunks.Count > 0)
        {
            Chunk oldest = Chunks[0];
            if (oldest.transform.position.z + chunkLength < _car.transform.position.z - BehindChunks * chunkLength)
            {
                Chunks.RemoveAt(0);
                Debug.Log($"[ChunkManager] Recycled chunk {oldest.Id}: z={oldest.transform.position.z:F1} car.z={_car.transform.position.z:F1}");

                // Free its entities back to their pools before reuse
                for (int i = 0; i < oldest.Entities.Count; i++)
                {
                    oldest.Entities[i].Description.Release(oldest.Entities[i]);
                }

                return oldest;
            }
        }

        Chunk created = new GameObject("Chunk").AddComponent<Chunk>();
        created.transform.SetParent(transform);
        return created;
    }
}
