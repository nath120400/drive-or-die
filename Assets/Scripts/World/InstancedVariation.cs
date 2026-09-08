using UnityEngine;

// Attached to the instanced prefab: random variations around its base transform.
// Rotation and scale deviations are relative to the base values; Radius is the
// ground footprint the instance may wander inside (uniform disc, world upright)
public class InstancedVariation : MonoBehaviour
{
    [Header("Rotation (degrees, relative to base)")]
    [SerializeField] private Vector3 _rotationMin = Vector3.zero;
    [SerializeField] private Vector3 _rotationMax = Vector3.zero;

    [Header("Ground footprint (radius the instance may wander inside)")]
    [SerializeField] private float _radius;

    [Header("Scale (multiplies the base scale)")]
    [SerializeField] private float _scaleMin = 1f;
    [SerializeField] private float _scaleMax = 1f;

    public float Radius => _radius;

    public void Apply(ref Matrix4x4 matrix)
    {
        Vector3 rotation = new Vector3(
            Random.Range(_rotationMin.x, _rotationMax.x),
            Random.Range(_rotationMin.y, _rotationMax.y),
            Random.Range(_rotationMin.z, _rotationMax.z));

        float scale = Random.Range(_scaleMin, _scaleMax);

        // The rotation deviation applies in the parent frame (cardinal axes),
        // the scale multiplies the mesh in its own frame
        matrix = Matrix4x4.Rotate(Quaternion.Euler(rotation)) * matrix * Matrix4x4.Scale(Vector3.one * scale);
    }
}
