using UnityEngine;

// Bounds of the entity spawn zone in the marker's own frame: Min/Max span the
// horizontal rect (x, z), Altitude sets the base height, tilt works via the marker
public class ChunkZone : MonoBehaviour
{
    public Vector2 Min;
    public Vector2 Max = new Vector2(0f, 10f);
    public float Altitude;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(
            new Vector3((Min.x + Max.x) * 0.5f, Altitude, (Min.y + Max.y) * 0.5f),
            new Vector3(Max.x - Min.x, 0f, Max.y - Min.y));
        Gizmos.matrix = Matrix4x4.identity;
    }
}
