using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class InstancedMeshTest : MonoBehaviour
{
    [SerializeField] private ChunkZone _zone;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _count = 100;
    [SerializeField] private int _xCells = 10;
    [SerializeField] private int _yCells = 10;
    [SerializeField] private int _tries = 4;

    private readonly List<Matrix4x4> _matrices = new List<Matrix4x4>();

    // Occupancy grid, resized when the cell count changes
    private readonly List<bool> _cells = new List<bool>();

    private bool _dirty = true;

    // Cached from the prefab: mesh, material, its local transform and variation range
    private Mesh _mesh;
    private Material _material;
    private Matrix4x4 _shape;
    private InstancedVariation _variation;

    private void OnEnable()
    {
        _dirty = true;
    }

    private void OnValidate()
    {
        _dirty = true;
    }

    private void LateUpdate()
    {
        if (_dirty)
        {
            Rebuild();
        }

        if (_mesh == null || _material == null)
        {
            return;
        }

        RenderParams renderParams = new RenderParams(_material);
        for (int start = 0; start < _matrices.Count; start += 1023)
        {
            int count = Mathf.Min(1023, _matrices.Count - start);
            Graphics.RenderMeshInstanced(renderParams, _mesh, 0, _matrices, count, start);
        }
    }

    // Cells tile the zone rect; one mesh per filled cell, up to Count.
    // The prefab brings its own mesh, material, position offset, rotation and scale
    private void Rebuild()
    {
        _dirty = false;
        _matrices.Clear();

        MeshFilter filter = _prefab != null ? _prefab.GetComponentInChildren<MeshFilter>() : null;
        if (filter == null || filter.sharedMesh == null)
        {
            return;
        }

        _mesh = filter.sharedMesh;
        _material = _prefab.GetComponentInChildren<MeshRenderer>().sharedMaterial;
        _shape = Matrix4x4.TRS(
            filter.transform.localPosition,
            filter.transform.localRotation,
            filter.transform.localScale);
        _variation = _prefab.GetComponentInChildren<InstancedVariation>();

        int total = Mathf.Min(_count, _xCells * _yCells);
        float cellX = (_zone.Max.x - _zone.Min.x) / _xCells;
        float cellY = (_zone.Max.y - _zone.Min.y) / _yCells;

        while (_cells.Count < _xCells * _yCells)
        {
            _cells.Add(false);
        }
        for (int i = 0; i < _xCells * _yCells; i++)
        {
            _cells[i] = false;
        }

        // Dart throwing: each instance draws random cells until it finds a free one
        for (int placed = 0; placed < total; placed++)
        {
            for (int tryIndex = 0; tryIndex < _tries; tryIndex++)
            {
                int col = Random.Range(0, _xCells);
                int row = Random.Range(0, _yCells);
                int cell = row * _xCells + col;
                if (_cells[cell])
                {
                    continue;
                }

                _cells[cell] = true;

                Vector3 zoneLocal = new Vector3(
                    _zone.Min.x + (col + 0.5f) * cellX,
                    _zone.Altitude,
                    _zone.Min.y + (row + 0.5f) * cellY);

                // The footprint radius shrinks the free area: the instance may
                // wander anywhere inside its cell without overhanging
                if (_variation != null && _variation.Radius > 0f)
                {
                    float rangeX = Mathf.Max(0f, cellX * 0.5f - _variation.Radius);
                    float rangeY = Mathf.Max(0f, cellY * 0.5f - _variation.Radius);
                    zoneLocal.x += Random.Range(-rangeX, rangeX);
                    zoneLocal.z += Random.Range(-rangeY, rangeY);
                }

                Vector3 position = transform.InverseTransformPoint(_zone.transform.TransformPoint(zoneLocal));
                Matrix4x4 localMatrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one) * _shape;
                if (_variation != null)
                {
                    _variation.Apply(ref localMatrix);
                }

                _matrices.Add(transform.localToWorldMatrix * localMatrix);
                break;
            }
        }
    }
}
